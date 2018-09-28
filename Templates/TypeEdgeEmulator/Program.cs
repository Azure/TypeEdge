using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Edge.Agent.Docker;
using Microsoft.Azure.TypeEdge.Host;
using Microsoft.Extensions.Configuration;

namespace TypeEdgeEmulator
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            //TODO: Set your IoT Hub iothubowner connection string in appsettings.json
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json")
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var host = new TypeEdgeHost(configuration);

            //TODO: Register your TypeEdge Modules here
            //host.RegisterModule<ITemperatureModule, TemperatureModule>();

            //TODO: Define all cross-module subscriptions 
            //host.Upstream.Subscribe(host.GetProxy<INormalizeTemperatureModule>().NormalizedTemperature);

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