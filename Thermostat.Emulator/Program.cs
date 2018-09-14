using System;
using System.Threading.Tasks;
using TypeEdge.Host;
using Microsoft.Extensions.Configuration;
using Modules;
using ThermostatApplication.Modules;
using TypeEdge.DovEnv;
using System.IO;

namespace ThermostatApplication
{
    internal class Program
    { 
        private static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables() 
                .AddDotenvFile()
                .AddCommandLine(args)
                .Build();

            var host = new TypeEdgeHost(configuration);

            host.RegisterModule<ITemperatureSensor, TemperatureSensor>();
            host.RegisterInstance(new Twins.TemperatureTwin()
            {
                SamplingHz = 11,
                Amplitude = 11,
                Frequency = 3,
                WaveType = Twins.WaveformType.Sine,
                Offset = 61
            });

            host.Upstream.Subscribe(host.GetProxy<ITemperatureSensor>().Temperature);

            var manifest = host.GenerateDeviceManifest((e) =>
            {
                return "1.0";
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