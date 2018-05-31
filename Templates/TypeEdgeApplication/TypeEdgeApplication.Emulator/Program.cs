using System;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.TypeEdge.Host;
using Microsoft.Azure.IoT.TypeEdge.Host.DovEnv;
using Microsoft.Extensions.Configuration;
using TypeEdgeApplication.Shared;

namespace TypeEdgeApplication
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
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

            //TODO: Define all cross-module subscriptions 
            host.Upstream.Subscribe(host.GetProxy<ITypeEdgeModule2>().Output);

            host.Build();

            await host.RunAsync();

            Console.WriteLine("Press <ENTER> to exit..");
            Console.ReadLine();
        }
    }
}