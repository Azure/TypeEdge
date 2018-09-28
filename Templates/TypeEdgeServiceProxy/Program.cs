using System;
using Microsoft.Azure.TypeEdge.Proxy;
using Microsoft.Extensions.Configuration;

namespace TypeEdgeProxy
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Press <ENTER> to start..");
            Console.ReadLine();

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json")
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