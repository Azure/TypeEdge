using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Castle.DynamicProxy;
using Microsoft.Azure.Devices; 
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Azure.Devices.Edge.Agent.Core;
using Microsoft.Azure.Devices.Edge.Storage;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.IoT.TypeEdge.Host.Hub;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.IoT.TypeEdge.Modules.Messages;
using Microsoft.Azure.IoT.TypeEdge.Proxy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HubService = Microsoft.Azure.Devices.Edge.Hub.Service;
using Module = Microsoft.Azure.Devices.Module;

namespace Microsoft.Azure.IoT.TypeEdge.Host
{
    public class TypeEdgeHost
    {
        private readonly IConfigurationRoot _configuration;
        private IContainer _container;
        private readonly ContainerBuilder _containerBuilder;
        private readonly EdgeHub _hub;
        private ModuleCollection _modules;
        private readonly TypeEdgeHostOptions _options;


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
        }

        public Upstream<JsonMessage> Upstream { get; set; }

        public void RegisterModule<TIModule, TTModule>()
            where TIModule : class
            where TTModule : class
        {
            _containerBuilder.RegisterType<TTModule>();
            _containerBuilder.RegisterInstance(new ProxyGenerator()
                .CreateInterfaceProxyWithoutTarget<TIModule>(new ModuleProxy<TIModule>()));
        }

        public void RegisterModule<TTModule>()
            where TTModule : class
        {
            _containerBuilder.RegisterType<TTModule>();
        }

        public void Build()
        {
            //setup the container
            BuildContainer();

            _modules = CreateModules();

            var deviceSasKey = ProvisionDeviceAsync().Result;

            ConfigureModules();

            BuildHub(deviceSasKey);
        }


        public async Task RunAsync()
        {
            var tasks = new List<Task>
            {
                _hub.RunAsync()
            };
            //start all modules
            foreach (var module in _modules)
                tasks.Add(module.InternalRunAsync());

            await Task.WhenAll(tasks.ToArray());
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

        private void BuildContainer()
        {
            var services = new ServiceCollection().AddLogging();
            _containerBuilder.Populate(services);
            _containerBuilder.RegisterBuildCallback(c => { });

            _container = _containerBuilder.Build();
        }

        private void BuildHub(string deviceSasKey)
        {
            //Calculate the Hub Enviroment Varialbes
            var currentLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            Environment.SetEnvironmentVariable(HubService.Constants.SslCertEnvName,
                "edge-hub-server.cert.pfx");
            Environment.SetEnvironmentVariable(HubService.Constants.SslCertPathEnvName,
                Path.Combine(currentLocation, @"Certificates/edge-hub-server/cert"));

            Environment.SetEnvironmentVariable("EdgeModuleHubServerCAChainCertificateFile",
                Path.Combine(currentLocation, @"Certificates/edge-chain-ca/cert/edge-chain-ca.cert.pem"));

            var storageFolder = Path.Combine(currentLocation, @"Storage");

            var hubStorageFolder = Path.Combine(storageFolder, HubService.Constants.EdgeHubStorageFolder);

            if (!Directory.Exists(hubStorageFolder))
                Directory.CreateDirectory(hubStorageFolder);

            Environment.SetEnvironmentVariable("storageFolder", storageFolder);

            var csBuilder = IotHubConnectionStringBuilder.Create(_options.IotHubConnectionString);
            var edgeConnectionString =
                new ModuleConnectionString.ModuleConnectionStringBuilder(csBuilder.HostName, _options.DeviceId)
                    .WithModuleId(Devices.Edge.Agent.Core.Constants.EdgeHubModuleName)
                    .WithModuleId(Devices.Edge.Agent.Core.Constants.EdgeHubModuleIdentityName)
                    .WithSharedAccessKey(deviceSasKey)
                    .Build();
            Environment.SetEnvironmentVariable(Devices.Edge.Agent.Core.Constants.EdgeHubConnectionStringKey,
                edgeConnectionString);
            Environment.SetEnvironmentVariable(Devices.Edge.Agent.Core.Constants.IotHubConnectionStringKey,
                edgeConnectionString);

            var edgeHubConfiguration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            _hub.InternalConfigure(edgeHubConfiguration);
        }

        private void ConfigureModules()
        {
            var currentLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            foreach (var module in _modules)
            {
                var moduleConnectionString =
                    GetModuleConnectionStringAsync(_options.IotHubConnectionString, _options.DeviceId, module.Name)
                        .Result;
                Environment.SetEnvironmentVariable(Devices.Edge.Agent.Core.Constants.EdgeHubConnectionStringKey,
                    moduleConnectionString);
                Environment.SetEnvironmentVariable(Devices.Edge.Agent.Core.Constants.EdgeModuleCaCertificateFileKey,
                    Path.Combine(currentLocation, @"Certificates/edge-device-ca/cert/edge-device-ca-root.cert.pem"));

                var moduleConfiguration = new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .Build();

                module.InternalConfigure(moduleConfiguration);
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
                {
                    if (!(scope.Resolve(moduleType) is EdgeModule module))
                        continue;
                    modules.Add(module);
                }
            }

            return modules;
        }

        private async Task<string> ProvisionDeviceAsync()
        {
            IotHubConnectionStringBuilder.Create(_options.IotHubConnectionString);

            var registryManager = RegistryManager.CreateFromConnectionString(_options.IotHubConnectionString);
            string sasKey;
            try
            {
                var device = await registryManager.AddDeviceAsync(
                    new Device(_options.DeviceId) {Capabilities = new DeviceCapabilities {IotEdge = true}});
                sasKey = device.Authentication.SymmetricKey.PrimaryKey;
            }
            catch (DeviceAlreadyExistsException)
            {
                var device = await registryManager.GetDeviceAsync(_options.DeviceId);
                sasKey = device.Authentication.SymmetricKey.PrimaryKey;
            }

            ConfigurationContent configurationContent;
            using (var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Microsoft.Azure.IoT.TypeEdge.Host.deviceconfig.json"))
            using (var reader = new StreamReader(stream))
            {
                var deviceconfig = reader.ReadToEnd();
                configurationContent = JsonConvert.DeserializeObject<ConfigurationContent>(deviceconfig);
            }

            var modulesConfig =
                configurationContent.ModuleContent["$edgeAgent"].TargetContent["modules"] as JObject;

            var dockerRegistry = Environment.GetEnvironmentVariable("DOCKER_REGISTRY") ?? "";

            foreach (var module in _modules)
            {
                modulesConfig?.Add(module.Name.ToLower(), JObject.FromObject(new
                {
                    version = "1.0",
                    type = "docker",
                    status = "running",
                    restartPolicy = "on-failure",
                    settings = new
                    {
                        image = dockerRegistry + module.Name.ToLower(),
                        createOptions = "{\n  \"Env\":[\n     \"" + TypeEdge.Constants.ModuleNameConfigName + "=" +
                                        module.Name.ToLower() + "\"\n  ]\n}"
                    }
                }));

                try
                {
                    await registryManager.AddModuleAsync(new Module(_options.DeviceId, module.Name));
                }
                catch (ModuleAlreadyExistsException)
                {
                }
            }

            var twinContent = new TwinContent();
            configurationContent.ModuleContent["$edgeHub"] = twinContent;


            var routes = new Dictionary<string, string>();

            foreach (var module in _modules)
            foreach (var route in module.Routes)
                routes[$"route{routes.Count}"] = route;

            foreach (var route in _hub.Routes) routes[$"route{routes.Count}"] = route;

            var desiredProperties = new
            {
                schemaVersion = "1.0",
                routes,
                storeAndForwardConfiguration = new
                {
                    timeToLiveSecs = 20
                }
            };
            var patch = JsonConvert.SerializeObject(desiredProperties);

            twinContent.TargetContent = new TwinCollection(patch);
            await registryManager.ApplyConfigurationContentOnDeviceAsync(_options.DeviceId, configurationContent);

            if (_options.PrintDeploymentJson.HasValue && _options.PrintDeploymentJson.Value)
                Console.WriteLine(JToken.Parse(configurationContent.ToJson()).ToString(Formatting.Indented));

            return sasKey;
        }

        private async Task<string> GetModuleConnectionStringAsync(string iotHubConnectionString, string deviceId,
            string moduleName)
        {
            var csBuilder = IotHubConnectionStringBuilder.Create(iotHubConnectionString);
            var registryManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
            var module = await registryManager.GetModuleAsync(deviceId, moduleName);
            var sasKey = module.Authentication.SymmetricKey.PrimaryKey;

            return new ModuleConnectionString.ModuleConnectionStringBuilder(csBuilder.HostName, deviceId)
                .WithGatewayHostName(Environment.MachineName)
                .WithModuleId(moduleName)
                .WithSharedAccessKey(sasKey)
                .Build();
        }

        #endregion
    }
}