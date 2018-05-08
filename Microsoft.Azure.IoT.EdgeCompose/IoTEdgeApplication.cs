using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Azure.IoT.EdgeCompose.Hubs;
using Microsoft.Azure.IoT.EdgeCompose.Modules;
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

namespace Microsoft.Azure.IoT.EdgeCompose
{
    public abstract class IoTEdgeApplication
    {
        public IConfigurationRoot Configuration { get; }
        public IContainer Container { get; private set; }


        public EdgeHub Hub { get; set; }
        public ModuleCollection Modules { get; private set; }


        public IoTEdgeApplication(IConfigurationRoot configuration)
        {
            Configuration = configuration;

            // add the framework services
            var services = new ServiceCollection().AddLogging();

            Modules = new ModuleCollection();

            Hub = new EdgeHub();

            Compose();

            Container = BuildContainer(services);
        }


        IContainer BuildContainer(IServiceCollection services)
        {
            var builder = new ContainerBuilder();
            builder.Populate(services);
            builder.RegisterBuildCallback(c => { });

            var iotHubConnectionString = Configuration.GetValue<string>(Agent.Constants.IotHubConnectionStringKey);
            if (String.IsNullOrEmpty(iotHubConnectionString))
                throw new Exception($"Missing {Agent.Constants.IotHubConnectionStringKey} value in configuration");

            var deviceId = Configuration.GetValue<string>("DeviceId");
            if (String.IsNullOrEmpty(deviceId))
                throw new Exception($"Missing DeviceId value in configuration");

            #region edge hub config

            //configure the edge hub

            
            Environment.SetEnvironmentVariable(HubService.Constants.SslCertEnvName, "edge-hub-server.cert.pfx");
            Environment.SetEnvironmentVariable(HubService.Constants.SslCertPathEnvName, Path.Combine(
                 Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                @"Certificates\edge-hub-server\cert\"));

            Environment.SetEnvironmentVariable("EdgeModuleHubServerCAChainCertificateFile", Path.Combine(
                 Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                @"Certificates\edge-chain-ca\cert\edge-chain-ca.cert.pem"));

            var storageFolder = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
               @"Storage");


            var hubStorageFolder = Path.Combine(storageFolder, HubService.Constants.EdgeHubStorageFolder);

            if (!Directory.Exists(hubStorageFolder))
                Directory.CreateDirectory(hubStorageFolder);

            Environment.SetEnvironmentVariable("storageFolder", storageFolder);
            Environment.SetEnvironmentVariable("IotHubConnectionString", iotHubConnectionString);
            #endregion


            var deviceSasKey = ProvisionDeviceAsync(iotHubConnectionString, deviceId, Modules).Result;

            foreach (var module in Modules)
            {
                var moduleConnectionString = GetModuleConnectionStringAsync(iotHubConnectionString, deviceId, module.Name).Result;

                Environment.SetEnvironmentVariable(Agent.Constants.EdgeHubConnectionStringKey, moduleConnectionString);

                var moduleConfiguration = new ConfigurationBuilder()
                   .AddEnvironmentVariables()
                   .Build();

                module.InternalConfigure(moduleConfiguration);
            }

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

            Hub.InternalConfigure(edgeHubConfiguration);

            IContainer container = builder.Build();
            return container;
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
                }


                var twinContent = new TwinContent();
                config.ModuleContent["$edgeHub"] = twinContent;

                var desiredProperties = new
                {
                    schemaVersion = "1.0",
                    routes = new Dictionary<string, string>
                    {
                        ["route1"] = "from /* INTO $upstream",
                        ["route2"] = "from /modules/module1 INTO BrokeredEndpoint(\"/modules/module2/inputs/input1\")",
                        ["route3"] = "from /modules/module2 INTO BrokeredEndpoint(\"/modules/module3/inputs/input1\")",
                        ["route4"] = "from /modules/module3 INTO BrokeredEndpoint(\"/modules/module4/inputs/input1\")",
                    },
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

        //private async Task<string> ProvisionModuleAsync(string iotHubConnectionString, string deviceId, string moduleName)
        //{
        //    var csBuilder = IotHubConnectionStringBuilder.Create(iotHubConnectionString);
        //    RegistryManager registryManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
        //    string sasKey = null;
        //    try
        //    {
        //        var module = await registryManager.AddModuleAsync(new Devices.Module(deviceId, moduleName));
        //        sasKey = module.Authentication.SymmetricKey.PrimaryKey;
        //    }
        //    catch (ModuleAlreadyExistsException)
        //    {
        //        var module = await registryManager.GetModuleAsync(deviceId, moduleName);
        //        sasKey = module.Authentication.SymmetricKey.PrimaryKey;
        //    }
        //    return new Agent.ModuleConnectionString.ModuleConnectionStringBuilder(csBuilder.IotHubName, deviceId)
        //        .WithGatewayHostName(Environment.MachineName)
        //        .WithModuleId(moduleName)
        //        .WithSharedAccessKey(sasKey)
        //        .Build();
        //}
        public abstract CompositionResult Compose();

        public async Task RunAsync()
        {
            List<Task> tasks = new List<Task>();

            tasks.Add(Hub.RunAsync());
            //start all modules
            foreach (var module in Modules)
            {
                tasks.Add(module.InternalRunAsync());
            }

            await Task.WhenAll(tasks.ToArray());
        }

    }
}
