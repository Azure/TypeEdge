using System;
using System.Threading.Tasks;
using TypeEdge.Proxy;
using Microsoft.Extensions.Configuration;
using TypeEdgeApplication.Shared;

namespace TypeEdgeApplication.Proxy
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Press <ENTER> to start..");
            Console.ReadLine();

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            ProxyFactory.Configure(configuration["IotHubConnectionString"],
                configuration["DeviceId"]);

            //TODO: Get your module proxies by contract
            var proxy = ProxyFactory.GetModuleProxy<ITypeEdgeModule1>();
            proxy.ResetModule(100);

            Console.WriteLine("Press <ENTER> to exit..");
            Console.ReadLine();
        }
    }
}