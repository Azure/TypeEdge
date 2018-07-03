using System;
using System.Threading.Tasks;
using TypeEdge.DovEnv;
using TypeEdge.Host;
using Microsoft.Extensions.Configuration;
using TypeEdgeML.Shared;

namespace TypeEdgeML
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Emulator..");

            //TODO: Set your IoT Hub iothubowner connection string in appsettings.json
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddDotenvFile()
                .AddCommandLine(args)
                .Build();

            var host = new TypeEdgeHost(configuration);

            //TODO: Register your TypeEdge Modules here
            host.RegisterModule<ITypeEdgeModule1, Modules.TypeEdgeModule1>();
            host.RegisterModule<ITypeEdgeModule2, Modules.TypeEdgeModule2>();
            host.RegisterModule<ITypeEdgeModule3, Modules.TypeEdgeModule3>();

            //TODO: Define all cross-module subscriptions 
            host.Upstream.Subscribe(host.GetProxy<ITypeEdgeModule3>().Output);

            host.Build();

            await host.RunAsync();

            Console.WriteLine("Press <ENTER> to exit..");
            Console.ReadLine();
        }
    }
}