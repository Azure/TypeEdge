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
using System.Configuration;
using Agent = Microsoft.Azure.Devices.Edge.Agent.Core;
using Microsoft.Azure.Devices.Common.Exceptions;
using Newtonsoft.Json;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json.Linq;
using System.Linq;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;

namespace Microsoft.Azure.IoT.TypeEdge
{
    public class TypeEdgeApplication
    {
        IConfigurationRoot configuration;
        IContainer container;
        ContainerBuilder containerBuilder;
        ModuleCollection modules;
        EdgeHub hub;

        public void RegisterModule<_IModule, _TModule>()
            where _IModule : class
            where _TModule : class
        {
            //containerBuilder.RegisterType<_TModule>();
            //containerBuilder.RegisterInstance(new ProxyGenerator().CreateInterfaceProxyWithoutTarget<_IModule>(new ModuleProxy()) as _IModule);
            containerBuilder.RegisterType<_TModule>().AsSelf().As<_IModule>();
        }


        public TypeEdgeApplication(IConfigurationRoot configuration)
        {
            this.configuration = configuration;
            this.containerBuilder = new ContainerBuilder();
            hub = new EdgeHub();

        }
        public void Build()
        {
            //read the configuration first
            var (iotHubConnectionString, deviceId) = ReadConfiguration();

            //setup the container
            BuildContainer();

            this.modules = CreateModules();

            var deviceSasKey = ProvisionDeviceAsync(iotHubConnectionString, deviceId, this.modules).Result;

            ConfigureModules(iotHubConnectionString, deviceId);

            BuildHub(iotHubConnectionString, deviceId, deviceSasKey);
        }

        #region Build
        private void BuildContainer()
        {
            var services = new ServiceCollection().AddLogging();
            containerBuilder.Populate(services);
            containerBuilder.RegisterBuildCallback(c => { });

            container = containerBuilder.Build();
        }

        private (string iotHubConnectionString, string deviceId) ReadConfiguration()
        {
            var iotHubConnectionString = configuration.GetValue<string>(Agent.Constants.IotHubConnectionStringKey);
            if (String.IsNullOrEmpty(iotHubConnectionString))
                throw new Exception($"Missing {Agent.Constants.IotHubConnectionStringKey} value in configuration");

            var deviceId = configuration.GetValue<string>("DeviceId");
            if (String.IsNullOrEmpty(deviceId))
                throw new Exception($"Missing DeviceId value in configuration");

            return (iotHubConnectionString, deviceId);
        }

        private void BuildHub(string iotHubConnectionString, string deviceId, string deviceSasKey)
        {
            //Calculate the Hub Enviroment Varialbes
            var currentLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            Environment.SetEnvironmentVariable(HubService.Constants.SslCertEnvName,
                "edge-hub-server.cert.pfx");
            Environment.SetEnvironmentVariable(HubService.Constants.SslCertPathEnvName,
                Path.Combine(currentLocation, @"Certificates\edge-hub-server\cert\"));

            Environment.SetEnvironmentVariable("EdgeModuleHubServerCAChainCertificateFile",
                Path.Combine(currentLocation, @"Certificates\edge-chain-ca\cert\edge-chain-ca.cert.pem"));

            var storageFolder = Path.Combine(currentLocation, @"Storage");

            var hubStorageFolder = Path.Combine(storageFolder, HubService.Constants.EdgeHubStorageFolder);

            if (!Directory.Exists(hubStorageFolder))
                Directory.CreateDirectory(hubStorageFolder);

            Environment.SetEnvironmentVariable("storageFolder", storageFolder);

            var csBuilder = IotHubConnectionStringBuilder.Create(iotHubConnectionString);
            var edgeConnectionString = new Agent.ModuleConnectionString.ModuleConnectionStringBuilder(csBuilder.HostName, deviceId)
                .WithModuleId(Agent.Constants.EdgeHubModuleName)
                .WithModuleId(Agent.Constants.EdgeHubModuleIdentityName)
                .WithSharedAccessKey(deviceSasKey)
                .Build();
            Environment.SetEnvironmentVariable(Agent.Constants.EdgeHubConnectionStringKey, edgeConnectionString);
            Environment.SetEnvironmentVariable(Agent.Constants.IotHubConnectionStringKey, edgeConnectionString);

            var edgeHubConfiguration = new ConfigurationBuilder()
               .AddEnvironmentVariables()
               .Build();

            hub.InternalConfigure(edgeHubConfiguration);
        }

        private void ConfigureModules(string iotHubConnectionString, string deviceId)
        {
            foreach (var module in this.modules)
            {
                var moduleConnectionString = GetModuleConnectionStringAsync(iotHubConnectionString, deviceId, module.Name).Result;

                Environment.SetEnvironmentVariable(Agent.Constants.EdgeHubConnectionStringKey, moduleConnectionString);

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

                    module.ConfigureSubscriptions();
                    modules.Add(module);
                }
            }
            
            return modules;
        }


        private async Task<string> ProvisionDeviceAsync(string iotHubConnectionString, string deviceId, ModuleCollection modules)
        {
            var csBuilder = IotHubConnectionStringBuilder.Create(iotHubConnectionString);

            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
            string sasKey = null;
            try
            {
                var device = await registryManager.AddDeviceAsync(new Devices.Device(deviceId) { Capabilities = new Microsoft.Azure.Devices.Shared.DeviceCapabilities() { IotEdge = true } });
                sasKey = device.Authentication.SymmetricKey.PrimaryKey;
            }
            catch (DeviceAlreadyExistsException)
            {
                var device = await registryManager.GetDeviceAsync(deviceId);
                sasKey = device.Authentication.SymmetricKey.PrimaryKey;
            }

            try
            {
                var config = JsonConvert.DeserializeObject<ConfigurationContent>(File.ReadAllText("deviceconfig.json"));
                var modulesConfig = config.ModuleContent["$edgeAgent"].TargetContent["modules"] as JObject;
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
                            createOptions = $" -e __MODULE_NAME='{module.Name}' "
                        }
                    }));

                    try
                    {
                        await registryManager.AddModuleAsync(new Devices.Module(deviceId, module.Name));
                    }
                    catch (ModuleAlreadyExistsException)
                    {
                    }
                }

                var twinContent = new TwinContent();
                config.ModuleContent["$edgeHub"] = twinContent;


                var routes = new Dictionary<string, string>();
                foreach (var route in hub.Routes)
                {
                    routes[$"route{routes.Count}"] = route;
                }
                foreach (var module in this.modules)
                {
                    foreach (var route in module.Routes)
                    {
                        routes[$"route{routes.Count}"] = route;
                    }
                }

                var desiredProperties = new
                {
                    schemaVersion = "1.0",
                    routes = routes,
                    storeAndForwardConfiguration = new
                    {
                        timeToLiveSecs = 20
                    }
                };
                string patch = JsonConvert.SerializeObject(desiredProperties);

                twinContent.TargetContent = new TwinCollection(patch);
                await registryManager.ApplyConfigurationContentOnDeviceAsync(deviceId, config);
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
            return new Agent.ModuleConnectionString.ModuleConnectionStringBuilder(csBuilder.HostName, deviceId)
                .WithGatewayHostName(Environment.MachineName)
                .WithModuleId(moduleName)
                .WithSharedAccessKey(sasKey)
                .Build();
        }
        #endregion

        public async Task RunAsync()
        {
            List<Task> tasks = new List<Task>();

            tasks.Add(hub.RunAsync());
            //start all modules
            foreach (var module in modules)
                tasks.Add(module.InternalRunAsync());

            await Task.WhenAll(tasks.ToArray());
        }
    }
}
