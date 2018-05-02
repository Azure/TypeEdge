using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StructureMap;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.EdgeCompose
{
    public abstract class IoTEdgeApplication
    {
        public Hub Hub { get; set; }
        public Container Container { get; set; }
        public ModuleCollection Modules { get; private set; }
        public IoTEdgeApplication()
        {
            // add the framework services
            var services = new ServiceCollection()
                .AddLogging();

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
        }

        public abstract ApplicationInitializationResult InitializeApplication(Container container);
    }
}
