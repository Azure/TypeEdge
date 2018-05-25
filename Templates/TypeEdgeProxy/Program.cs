using System;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.TypeEdge.Proxy;
using Microsoft.Extensions.Configuration;

namespace TypeEdgeProxy
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
            //var result = ProxyFactory.GetModuleProxy<ITemperatureModule>().ResetSensor(10);

            Console.WriteLine("Press <ENTER> to exit..");
            Console.ReadLine();
        }
    }
}