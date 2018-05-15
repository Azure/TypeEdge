using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Azure.IoT.TypeEdge.Hubs;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using HubService = Microsoft.Azure.Devices.Edge.Hub.Service;
using System.Reflection;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Newtonsoft.Json;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json.Linq;
using System.Linq;
using Microsoft.Azure.IoT.TypeEdge.Host.Hub;
using Microsoft.Azure.Devices.Edge.Agent.Core;
using Core = Microsoft.Azure.Devices.Edge.Agent.Core;
using Castle.DynamicProxy;

namespace Microsoft.Azure.IoT.TypeEdge.Host
{
    public class TypeEdgeHost
    {
        public Upstream<JsonMessage> Upstream { get; set; }

        readonly IConfigurationRoot configuration;
        IContainer container;
        ContainerBuilder containerBuilder;
        ModuleCollection modules;
        EdgeHub hub;
        TypeEdgeHostOptions options;


        public TypeEdgeHost(IConfigurationRoot configuration)
        {
            this.configuration = configuration;

            options = new TypeEdgeHostOptions();
            configuration.GetSection("TypeEdgeHost").Bind(options);

            if (String.IsNullOrEmpty(options.IotHubConnectionString))
                throw new Exception($"Missing IotHubConnectionString value in configuration");

            if (String.IsNullOrEmpty(options.DeviceId))
                throw new Exception($"Missing DeviceId value in configuration");

            this.containerBuilder = new ContainerBuilder();
            hub = new EdgeHub();

            Upstream = new Upstream<JsonMessage>(hub);
        }

        public void RegisterModule<_IModule, _TModule>()
            where _IModule : class
            where _TModule : class
        {
            containerBuilder.RegisterType<_TModule>();
            containerBuilder.RegisterInstance(new ProxyGenerator()
                .CreateInterfaceProxyWithoutTarget<_IModule>(new ModuleProxy<_IModule>()) as _IModule);
        }

        public void RegisterModule<_TModule>()
            where _TModule : class
        {
            containerBuilder.RegisterType<_TModule>();
        }


        #region Build
        private void BuildContainer()
        {
            var services = new ServiceCollection().AddLogging();
            containerBuilder.Populate(services);
            containerBuilder.RegisterBuildCallback(c => { });

            container = containerBuilder.Build();
        }

        private void BuildHub(string deviceSasKey)
        {
            //Calculate the Hub Enviroment Varialbes
            var currentLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

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

            var csBuilder = IotHubConnectionStringBuilder.Create(options.IotHubConnectionString);
            var edgeConnectionString = new ModuleConnectionString.ModuleConnectionStringBuilder(csBuilder.HostName, options.DeviceId)
                .WithModuleId(Core.Constants.EdgeHubModuleName)
                .WithModuleId(Core.Constants.EdgeHubModuleIdentityName)
                .WithSharedAccessKey(deviceSasKey)
                .Build();
            Environment.SetEnvironmentVariable(Core.Constants.EdgeHubConnectionStringKey, edgeConnectionString);
            Environment.SetEnvironmentVariable(Core.Constants.IotHubConnectionStringKey, edgeConnectionString);

            var edgeHubConfiguration = new ConfigurationBuilder()
               .AddEnvironmentVariables()
               .Build();

            hub.InternalConfigure(edgeHubConfiguration);
        }

        private void ConfigureModules()
        {
            var currentLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (var module in this.modules)
            {
                var moduleConnectionString = GetModuleConnectionStringAsync(options.IotHubConnectionString, options.DeviceId, module.Name).Result;
                Environment.SetEnvironmentVariable(Core.Constants.EdgeHubConnectionStringKey, moduleConnectionString);
                Environment.SetEnvironmentVariable(Core.Constants.EdgeModuleCaCertificateFileKey, 
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
            using (var scope = container.BeginLifetimeScope())
            {
                foreach (var moduleType in container.ComponentRegistry.Registrations.Where(r => typeof(EdgeModule).IsAssignableFrom(r.Activator.LimitType)).Select(r => r.Activator.LimitType).Distinct())
                {
                    var module = scope.Resolve(moduleType) as EdgeModule;

                    module.BuildSubscriptions();
                    modules.Add(module);
                }
            }

            return modules;
        }

        private async Task<string> ProvisionDeviceAsync()
        {
            var csBuilder = IotHubConnectionStringBuilder.Create(options.IotHubConnectionString);

            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(options.IotHubConnectionString);
            string sasKey = null;
            try
            {
                var device = await registryManager.AddDeviceAsync(new Devices.Device(options.DeviceId) { Capabilities = new Microsoft.Azure.Devices.Shared.DeviceCapabilities() { IotEdge = true } });
                sasKey = device.Authentication.SymmetricKey.PrimaryKey;
            }
            catch (DeviceAlreadyExistsException)
            {
                var device = await registryManager.GetDeviceAsync(options.DeviceId);
                sasKey = device.Authentication.SymmetricKey.PrimaryKey;
            }

            try
            {
                ConfigurationContent configurationContent;
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microsoft.Azure.IoT.TypeEdge.Host.deviceconfig.json"))
                using (StreamReader reader = new StreamReader(stream))
                {
                    var deviceconfig = reader.ReadToEnd();
                    configurationContent = JsonConvert.DeserializeObject<ConfigurationContent>(deviceconfig);
                }
                var modulesConfig = configurationContent.ModuleContent["$edgeAgent"].TargetContent["modules"] as JObject;
                foreach (var module in modules)
                {
                    modulesConfig.Add(module.Name, JObject.FromObject(new
                    {
                        version = "1.0",
                        type = "docker",
                        status = "running",
                        restartPolicy = "on-failure",
                        settings = new
                        {
                            image = "devimage",
                            createOptions = $" -e {Microsoft.Azure.IoT.TypeEdge.Constants.ModuleNameConfigName}='{module.Name}' "
                        }
                    }));

                    try
                    {
                        await registryManager.AddModuleAsync(new Devices.Module(options.DeviceId, module.Name));
                    }
                    catch (ModuleAlreadyExistsException)
                    {
                    }
                }

                var twinContent = new TwinContent();
                configurationContent.ModuleContent["$edgeHub"] = twinContent;


                var routes = new Dictionary<string, string>();
              
                foreach (var module in this.modules)
                {
                    foreach (var route in module.Routes)
                    {
                        routes[$"route{routes.Count}"] = route;
                    }
                }

                foreach (var route in hub.Routes)
                {
                    routes[$"route{routes.Count}"] = route;
                }

                var desiredProperties = new
                {
                    schemaVersion = "1.0",
                    routes,
                    storeAndForwardConfiguration = new
                    {
                        timeToLiveSecs = 20
                    }
                };
                string patch = JsonConvert.SerializeObject(desiredProperties);

                twinContent.TargetContent = new TwinCollection(patch);
                await registryManager.ApplyConfigurationContentOnDeviceAsync(options.DeviceId, configurationContent);
            }
            catch
            {
                throw;
            }

            return sasKey;
        }

        private async Task<string> GetModuleConnectionStringAsync(string iotHubConnectionString, string deviceId, string moduleName)
        {
            var csBuilder = IotHubConnectionStringBuilder.Create(iotHubConnectionString);
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
            string sasKey = null;
            try
            {
                var module = await registryManager.GetModuleAsync(deviceId, moduleName);
                sasKey = module.Authentication.SymmetricKey.PrimaryKey;
            }
            catch
            {
                throw;
            }
            return new Core.ModuleConnectionString.ModuleConnectionStringBuilder(csBuilder.HostName, deviceId)
                .WithGatewayHostName(Environment.MachineName)
                .WithModuleId(moduleName)
                .WithSharedAccessKey(sasKey)
                .Build();
        }
        #endregion

        public void Build()
        {
            //setup the container
            BuildContainer();

            this.modules = CreateModules();

            var deviceSasKey = ProvisionDeviceAsync().Result;

            ConfigureModules();

            BuildHub(deviceSasKey);
        }


        public async Task RunAsync()
        {
            List<Task> tasks = new List<Task>
            {
                hub.RunAsync()
            };
            //start all modules
            foreach (var module in modules)
                tasks.Add(module.InternalRunAsync());

            await Task.WhenAll(tasks.ToArray());
        }

        public T GetProxy<T>()
            where T:class
        {
            var cb = new ContainerBuilder();
                cb.RegisterInstance(new ProxyGenerator()
                .CreateInterfaceProxyWithoutTarget<T>(new ModuleProxy<T>()) as T);
            return cb.Build().Resolve<T>();
        }
    }
}
