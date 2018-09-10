using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Castle.DynamicProxy;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Azure.Devices.Edge.Agent.Core;
using Microsoft.Azure.Devices.Edge.Storage;
using Microsoft.Azure.Devices.Shared;
using TypeEdge.Host.Hub;
using TypeEdge.Modules;
using TypeEdge.Modules.Endpoints;
using TypeEdge.Modules.Messages;
using TypeEdge.Proxy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HubService = Microsoft.Azure.Devices.Edge.Hub.Service;
using Module = Microsoft.Azure.Devices.Module;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Globalization;
using System.Runtime.Loader;

namespace TypeEdge.Host
{
    public class TypeEdgeHost
    {
        private readonly IConfigurationRoot _configuration;
        private readonly ContainerBuilder _containerBuilder;
        private readonly EdgeHub _hub;
        private readonly TypeEdgeHostOptions _options;
        private readonly bool _inContainer;
        private string _manifest;
        private IContainer _container;
        private ModuleCollection _modules;

        const string DisableServerCertificateValidationKeyName =
            "Microsoft.Azure.Devices.DisableServerCertificateValidation";

        public TypeEdgeHost(IConfigurationRoot configuration)
        {
            _configuration = configuration;

            _options = new TypeEdgeHostOptions();
            configuration.GetSection("TypeEdgeHost").Bind(_options);

            if (string.IsNullOrEmpty(_options.IotHubConnectionString))
                throw new Exception($"Missing IotHubConnectionString value in configuration");

            if (string.IsNullOrEmpty(_options.DeviceId))
                throw new Exception($"Missing DeviceId value in configuration");

            _containerBuilder = new ContainerBuilder();
            _hub = new EdgeHub();

            Upstream = new Upstream<JsonMessage>(_hub);

            _inContainer = File.Exists(@"/.dockerenv");
        }

        public Upstream<JsonMessage> Upstream { get; set; }

        public void RegisterModule<TIModule, TTModule>()
            where TIModule : class
            where TTModule : class
        {
            var moduleName = typeof(TIModule).GetModuleName();

            _containerBuilder.RegisterType<TTModule>().WithParameter(
                (pi, ctx) => pi.ParameterType == typeof(IConfigurationRoot),
                (pi, ctx) => new ConfigurationBuilder()
                        .AddJsonFile($"{moduleName}Settings.json", true)
                        .AddJsonFile("appsettings.json", true)
                        .AddEnvironmentVariables()
                        .Build());

            _containerBuilder.RegisterInstance(new ProxyGenerator()
                .CreateInterfaceProxyWithoutTarget<TIModule>(new ModuleProxy<TIModule>()));
        }


        public void RegisterModule<TTModule>()
            where TTModule : class
        {
            _containerBuilder.RegisterType<TTModule>();
        }

        private void Init()
        {
            CleanUp();

            BuildDIContainer();

            _modules = CreateModules();
        }
        public string GenerateDeviceManifest(Func<string, string> moduleImageVersionCallback)
        {
            if (_modules == null)
                Init();

            return _GenerateManifest(moduleImageVersionCallback);
        }
        public string ProvisionDevice(string manifest)
        {
            if (_modules == null)
                Init();

            var deviceSasKey = ProvisionDeviceAsync(manifest).Result;

            return deviceSasKey;           
        }

        public void BuildEmulatedDevice(string deviceSasKey)
        {
            if (_modules == null)
                Init();

            ConfigureModules();

            BuildHub(deviceSasKey);
        }

        private void CleanUp()
        {
            if (!_inContainer || !Directory.Exists(TypeEdge.Constants.ComposeConfigurationPath))
                return;
            foreach (var file in Directory.EnumerateFiles(TypeEdge.Constants.ComposeConfigurationPath))
                File.Delete(file);
        }

        public async Task RunAsync()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cancellationTokenSource.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cancellationTokenSource.Cancel();

            await RunAsync(cancellationTokenSource.Token);
        }
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var tasks = new List<Task>
            {
                CreateTemporaryConnection(),
                _hub.RunAsync(cancellationToken)
            };

            //start all modules
            if (!_inContainer)
                foreach (var module in _modules)
                    tasks.Add(module._RunAsync(cancellationToken));

            tasks.Add(cancellationToken.WhenCanceled());

            await Task.WhenAll(tasks.ToArray());
        }

        private async Task CreateTemporaryConnection()
        {
            var moduleConnectionString =
                  GetModuleConnectionStringAsync(_options.IotHubConnectionString, _options.DeviceId, _modules[0].Name)
                      .Result;

            var tmpClient = ModuleClient.CreateFromConnectionString(moduleConnectionString,
                            new ITransportSettings[]
                            {
                                new AmqpTransportSettings(Microsoft.Azure.Devices.Client.TransportType.Amqp_Tcp_Only)
                                {
                                    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                                    OpenTimeout = new TimeSpan(1)
                                }
                            }
                        );

            try
            {
                await tmpClient.OpenAsync();
            }
            catch { }
        }

        public T GetProxy<T>()
            where T : class
        {
            var cb = new ContainerBuilder();
            cb.RegisterInstance(new ProxyGenerator()
                .CreateInterfaceProxyWithoutTarget<T>(new ModuleProxy<T>()));
            return cb.Build().Resolve<T>();
        }


        #region Build

        private void BuildDIContainer()
        {
            var services = new ServiceCollection().AddSingleton(new LoggerFactory()
                .AddConsole()
                .AddSerilog()
                .AddDebug());
            services.AddLogging();

            Log.Logger = new LoggerConfiguration()
                 .MinimumLevel.Debug()
                 .Enrich.FromLogContext()
                 .CreateLogger();

            _containerBuilder.Populate(services);
            _containerBuilder.RegisterBuildCallback(c => { });

            _containerBuilder.Register((ss, p) =>
            {
                return new ConfigurationBuilder()
                            .AddJsonFile($"Settings.json", true)
                            .AddJsonFile("appsettings.json", true)
                            .AddEnvironmentVariables()
                            .Build();
            });

            _container = _containerBuilder.Build();
        }

        private void BuildHub(string deviceSasKey)
        {
            //Calculate the Hub Enviroment Varialbes
            var currentLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            Environment.SetEnvironmentVariable(HubService.Constants.EdgeHubServerCertificateFileKey,
                Path.Combine(currentLocation, @"Certificates/edge-hub-server/cert/edge-hub-server.cert.pfx"));

            Environment.SetEnvironmentVariable(HubService.Constants.SslCertEnvName,
                "edge-hub-server.cert.pfx");

            Environment.SetEnvironmentVariable(HubService.Constants.SslCertPathEnvName,
                Path.Combine(currentLocation, @"Certificates/edge-hub-server/cert"));

            Environment.SetEnvironmentVariable(HubService.Constants.EdgeHubServerCAChainCertificateFileKey,
                Path.Combine(currentLocation, @"Certificates/edge-chain-ca/cert/edge-chain-ca.cert.pem"));

            var storageFolder = Path.Combine(currentLocation, @"Storage");

            var hubStorageFolder = Path.Combine(storageFolder, HubService.Constants.EdgeHubStorageFolder);

            if (!Directory.Exists(hubStorageFolder))
                Directory.CreateDirectory(hubStorageFolder);

            Environment.SetEnvironmentVariable("storageFolder", storageFolder);

            var csBuilder = Microsoft.Azure.Devices.IotHubConnectionStringBuilder.Create(_options.IotHubConnectionString);

            var edgeConnectionString =
                new ModuleConnectionStringBuilder(csBuilder.HostName, _options.DeviceId)
                    .Create(Microsoft.Azure.Devices.Edge.Agent.Core.Constants.EdgeHubModuleIdentityName)
                    .WithSharedAccessKey(deviceSasKey)
                    .Build();
            Environment.SetEnvironmentVariable(Microsoft.Azure.Devices.Edge.Agent.Core.Constants.EdgeHubConnectionStringKey,
                edgeConnectionString);
            Environment.SetEnvironmentVariable(variable: Microsoft.Azure.Devices.Edge.Agent.Core.Constants.IotHubConnectionStringKey,
                value: edgeConnectionString);

            var edgeHubConfiguration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddEnvironmentVariables()
                .Build();

            _hub._Init(edgeHubConfiguration, _container);
        }

        private void ConfigureModules()
        {
            var certificatePath = @"Certificates/edge-device-ca/cert/edge-device-ca-root.cert.pem";
            var currentLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            Environment.SetEnvironmentVariable(Microsoft.Azure.Devices.Edge.Agent.Core.Constants.EdgeModuleCaCertificateFileKey,
                Path.Combine(currentLocation,
                    certificatePath));

            foreach (var module in _modules)
            {
                var moduleConnectionString =
                    GetModuleConnectionStringAsync(_options.IotHubConnectionString, _options.DeviceId, module.Name)
                        .Result;

                if (_inContainer)
                {
                    System.Console.WriteLine("Emulator running in docker-compose mode.");

                    const string envPath = TypeEdge.Constants.ComposeConfigurationPath;
                    var path = Path.Combine(envPath, $"{module.Name}.env");
                    var certPath = Path.Combine(envPath, certificatePath);

                    var dotenvData = $"{Microsoft.Azure.Devices.Edge.Agent.Core.Constants.EdgeHubConnectionStringKey}={moduleConnectionString}";
                    dotenvData += $"{Environment.NewLine}{Microsoft.Azure.Devices.Edge.Agent.Core.Constants.EdgeModuleCaCertificateFileKey}={certPath}";

                    if (!Directory.Exists(Path.GetDirectoryName(certPath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(certPath));

                    if (!File.Exists(certPath))
                        File.Copy(Path.Combine(currentLocation, certificatePath), certPath);

                    File.WriteAllText(path, dotenvData);
                }
                else
                {
                    Environment.SetEnvironmentVariable(Microsoft.Azure.Devices.Edge.Agent.Core.Constants.EdgeHubConnectionStringKey,
                        moduleConnectionString);

                    var moduleConfiguration = new ConfigurationBuilder()
                        .AddJsonFile($"{module.Name}Settings.json", true)
                        .AddJsonFile("appsettings.json", true)
                        .AddEnvironmentVariables()
                        .Build();

                    module._Init(moduleConfiguration, _container);
                }
            }
        }

        private ModuleCollection CreateModules()
        {
            var modules = new ModuleCollection();
            using (var scope = _container.BeginLifetimeScope())
            {
                foreach (var moduleType in _container.ComponentRegistry.Registrations
                    .Where(r => typeof(EdgeModule).IsAssignableFrom(r.Activator.LimitType))
                    .Select(r => r.Activator.LimitType).Distinct())
                    if ((scope.Resolve(moduleType) is EdgeModule module))
                        modules.Add(module);
            }

            return modules;
        }

        private string _GenerateManifest(Func<string, string> moduleImageVersion)
        {
            ConfigurationContent configurationContent;
            using (var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("TypeEdge.Host.deviceconfig.json"))
            using (var reader = new StreamReader(stream))
            {
                var deviceconfig = reader.ReadToEnd();
                configurationContent = JsonConvert.DeserializeObject<ConfigurationContent>(deviceconfig);
            }
            var agentDesired = JObject.FromObject(configurationContent.ModulesContent["$edgeAgent"]["properties.desired"]);

            if (!agentDesired.TryGetValue("modules", out JToken modules))
                throw new Exception("Cannot read modules config from $edgeAgent");
            var modulesConfig = modules as JObject;
            var dockerRegistry = _configuration.GetValue<string>("DOCKER_REGISTRY") ?? "";

            foreach (var module in _modules)
            {
                var volumes = "";
                if (module.Volumes.Count > 0)
                {
                    var v = String.Join(',', module.Volumes.Select(e => $"\"{$"/env/{e.Key.ToLower()}"}\": {{}}"));
                    volumes = $", \"Volumes\": {{ {v} }}";
                }
                var imageVersion = moduleImageVersion != null ? ":" + moduleImageVersion(module.Name.ToLower()) : "";
                modulesConfig?.Add(module.Name.ToLower(), JObject.FromObject(new
                {
                    version = "1.0",
                    type = "docker",
                    status = "running",
                    restartPolicy = "on-failure",
                    settings = new
                    {
                        image = dockerRegistry + module.Name.ToLower() + imageVersion,
                        createOptions = "{\n  \"Env\":[\n     \"" + TypeEdge.Constants.ModuleNameConfigName + "=" +
                                            module.Name.ToLower() + $"\"\n  ]\n {volumes} }}"
                    }
                }));
            }
            agentDesired["modules"] = modulesConfig;
            configurationContent.ModulesContent["$edgeAgent"]["properties.desired"] = agentDesired;

            var routes = new Dictionary<string, string>();

            foreach (var module in _modules)
                foreach (var route in module.Routes)
                    routes[$"route{routes.Count}"] = route;

            foreach (var route in _hub.Routes) routes[$"route{routes.Count}"] = route;

            configurationContent.ModulesContent["$edgeHub"] = new Dictionary<string, object>
            {
                ["properties.desired"] = new
                {
                    schemaVersion = "1.0",
                    routes,
                    storeAndForwardConfiguration = new
                    {
                        timeToLiveSecs = 20
                    }
                }
            };

            _manifest = @"// This content was automatically generated by TypeEdge Emulator. Do not edit directly." + Environment.NewLine + Environment.NewLine;
            _manifest += JToken.Parse(configurationContent.ToJson()).ToString(Formatting.Indented);

            if (_options.PrintDeploymentJson.HasValue && _options.PrintDeploymentJson.Value)
                Console.WriteLine(_manifest);

            return _manifest;
        }
        private async Task<string> ProvisionDeviceAsync(string manifest)
        {
            Microsoft.Azure.Devices.IotHubConnectionStringBuilder.Create(_options.IotHubConnectionString);

            var registryManager = RegistryManager.CreateFromConnectionString(_options.IotHubConnectionString);
            string sasKey;
            var device = await registryManager.GetDeviceAsync(_options.DeviceId);
            if (device == null)
            {
                device = await registryManager.AddDeviceAsync(
                new Device(_options.DeviceId) { Capabilities = new DeviceCapabilities { IotEdge = true } });
            }
            sasKey = device.Authentication.SymmetricKey.PrimaryKey;

            try
            {
                var oldModules = await registryManager.GetModulesOnDeviceAsync(_options.DeviceId);
                foreach (var oldModule in oldModules)
                    if (!oldModule.Id.StartsWith("$"))
                        await registryManager.RemoveModuleAsync(oldModule);
            }
            catch
            {
            }

            foreach (var module in _modules)
            {
                try
                {
                    await registryManager.AddModuleAsync(new Module(_options.DeviceId, module.Name));
                }
                catch (ModuleAlreadyExistsException)
                {
                }
            }

            await registryManager.ApplyConfigurationContentOnDeviceAsync(_options.DeviceId, 
                manifest.FromJson<ConfigurationContent>());

            return sasKey;
        }

        private async Task<string> GetModuleConnectionStringAsync(string iotHubConnectionString, string deviceId,
            string moduleName)
        {
            var csBuilder = Microsoft.Azure.Devices.IotHubConnectionStringBuilder.Create(iotHubConnectionString);
            var registryManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
            var module = await registryManager.GetModuleAsync(deviceId, moduleName);
            var sasKey = module.Authentication.SymmetricKey.PrimaryKey;

            return new ModuleConnectionStringBuilder(csBuilder.HostName, deviceId)
                .Create(moduleName)
                .WithGatewayHostName(Environment.MachineName)
                .WithSharedAccessKey(sasKey)
                .Build();
        }

        #endregion
    }
}