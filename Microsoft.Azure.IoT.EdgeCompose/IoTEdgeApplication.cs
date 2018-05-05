using Microsoft.Azure.IoT.EdgeCompose.Hubs;
using Microsoft.Azure.IoT.EdgeCompose.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StructureMap;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.EdgeCompose
{
    public abstract class IoTEdgeApplication
    {
        public MemoryHub Hub { get; set; }
        public Container Container { get; set; }
        public ModuleCollection Modules { get; private set; }
        public IoTEdgeApplication()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config/appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // add the framework services
            var services = new ServiceCollection().AddLogging();

            // add StructureMap
            Container = new Container();
            Container.Configure(config =>
            {
                // Register stuff in container, using the StructureMap APIs...
                config.Scan(_ =>
                {
                    _.AssembliesAndExecutablesFromApplicationBaseDirectory();
                    _.WithDefaultConventions();
                });
                // Populate the container using the service collection
                config.Populate(services);
            });

            Modules = new ModuleCollection();
            Hub = new MemoryHub(Container);

        }

        public abstract CompositionResult Compose();

        public async Task RunAsync() {

            Compose();

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
            //first the hub
            await Hub.StartAsync();
            
            //start all modules
            foreach (var module in Modules)
            {
                await module.StartAsync();
            }
        }
    }
}
