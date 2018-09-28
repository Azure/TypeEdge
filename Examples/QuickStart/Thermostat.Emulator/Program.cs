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

            //register the modules
            host.RegisterModule<ITemperatureSensor, TemperatureSensor>();

            //host.RegisterExternalModule(new TypeEdge.Host.Docker.DockerModule("tempSensor",
            //    new HostingSettings("mcr.microsoft.com/azureiotedge-simulated-temperature-sensor:1.0", null),
            //    null,
            //    null));

            //add cross-module routes
            host.Upstream.Subscribe(host.GetProxy<ITemperatureSensor>().Temperature);

            //customize the runtime configuration
            var dockerRegistry = configuration.GetValue<string>("DOCKER_REGISTRY") ?? "";
            var manifest = host.GenerateDeviceManifest((e, settings) =>
            {
                //this is the opportunity for the host to change the hosting settings of the module e
                if (!settings.IsExternalModule)
                    settings.Config = new DockerConfig($"{dockerRegistry}{e}:1.0", settings.Config.CreateOptions);
                return settings;
            });
            File.WriteAllText("../../../manifest.json", manifest);

            //provision a new device with the new manifest
            var sasToken = host.ProvisionDevice(manifest);

            //build an emulated device in memory
            host.BuildEmulatedDevice(sasToken);

            //run the emulated device
            await host.RunAsync();

            Console.WriteLine("Press <ENTER> to exit..");
            Console.ReadLine();
        }
    }
}