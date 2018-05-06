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

namespace Microsoft.Azure.IoT.EdgeCompose
{
    public abstract class IoTEdgeApplication
    {
        public IConfigurationRoot Configuration { get; }
        public IContainer Container { get; private set; }


        public EdgeHub Hub { get; set; }
        public ModuleCollection Modules { get; private set; }


        public IoTEdgeApplication(string configFile)
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile(configFile)
                .AddEnvironmentVariables()
                .Build();

            // add the framework services
            var services = new ServiceCollection().AddLogging();

            Modules = new ModuleCollection();

            Hub = new EdgeHub();
            Modules.Add(Hub);

            Compose();

            Container = BuildContainer(services);
        }


        IContainer BuildContainer(IServiceCollection services)
        {
            var builder = new ContainerBuilder();
            builder.Populate(services);
            builder.RegisterBuildCallback(c => { });
            var edgeDeviceConnectionString = Configuration.GetValue<string>(Constants.DeviceConnectionStringName);

            if (String.IsNullOrEmpty(edgeDeviceConnectionString))
                throw new Exception($"Missing {Constants.DeviceConnectionStringName} in configuration");

            var hubConnectionString = $"{edgeDeviceConnectionString};{Agent.Constants.ModuleIdKey}={Agent.Constants.EdgeHubModuleIdentityName}";

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

            Environment.SetEnvironmentVariable("IotHubConnectionString", hubConnectionString.ToString());

            foreach (var module in Modules)
            {
                module.RegisterOptions(builder, Configuration);
            }

            IContainer container = builder.Build();
            return container;
        }
        public abstract CompositionResult Compose();

        public async Task RunAsync()
        {
            await CreateAsync();

            await StartAsync();
        }

        private async Task CreateAsync()
        {
            //configure all modules
            foreach (var module in Modules)
            {
                await module.CreateAsync();
            }
        }

        private async Task StartAsync()
        {
            //start all modules
            foreach (var module in Modules)
            {
                await module.StartAsync();
            }
        }
    }
}
