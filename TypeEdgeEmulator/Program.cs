using System;
using System.Threading.Tasks;
using TypeEdge.Host;
using Microsoft.Extensions.Configuration;

namespace TypeEdgeEmulator
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            //TODO: Set your IoT Hub iothubowner connection string in appsettings.json
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var host = new TypeEdgeHost(configuration);

            //TODO: Register your TypeEdge Modules here
            //host.RegisterModule<ITemperatureModule, TemperatureModule>();

            //TODO: Define all cross-module subscriptions 
            //host.Upstream.Subscribe(host.GetProxy<INormalizeTemperatureModule>().NormalizedTemperature);

            host.Build();

            await host.RunAsync();

            Console.WriteLine("Press <ENTER> to exit..");
            Console.ReadLine();
        }
    }
}