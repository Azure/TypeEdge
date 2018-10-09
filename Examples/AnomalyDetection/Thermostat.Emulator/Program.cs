using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Edge.Agent.Docker;
using Microsoft.Azure.TypeEdge;
using Microsoft.Azure.TypeEdge.Host;
using Microsoft.Extensions.Configuration;
using Modules;
using ThermostatApplication.Modules;

namespace ThermostatApplication
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json")
                .AddEnvironmentVariables()
                .AddDotΕnv()
                .AddCommandLine(args)
                .Build();

            var host = new TypeEdgeHost(configuration);

            host.RegisterModule<ITemperatureSensor, TemperatureSensor>();
            host.RegisterModule<IOrchestrator, Orchestrator>();
            host.RegisterModule<IModelTraining, ModelTraining>();
            host.RegisterModule<IVisualization, Visualization>();
            host.RegisterModule<IAnomalyDetection, AnomalyDetection>();
            
            host.Upstream.Subscribe(host.GetProxy<IAnomalyDetection>().Anomaly);

            var dockerRegistry = configuration.GetValue<string>("DOCKER_REGISTRY") ?? "";
            var manifest = host.GenerateDeviceManifest((e, settings) =>
            {
                //this is the opportunity of the host to change the hosting settings of the module e
                settings.Config = new DockerConfig($"{dockerRegistry}{e}:1.0", settings.Config.CreateOptions);
                return settings;
            });
            var sasToken = host.ProvisionDevice(manifest);
            host.BuildEmulatedDevice(sasToken);

            File.WriteAllText("../../../manifest.json", manifest);

            await host.RunAsync();

            Console.WriteLine("Press <ENTER> to exit..");
            Console.ReadLine();
        }
    }
}