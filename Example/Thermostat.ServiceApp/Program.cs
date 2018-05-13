using Microsoft.Azure.IoT.TypeEdge.Proxy;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using ThermostatApplication.Modules;

namespace Thermostat.ServiceApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings_thermostat.json")
                .AddEnvironmentVariables()
                .Build();

            var proxy = new TypeEdgeProxy(configuration["deviceConnectionString"]);

            var module = proxy.GetModuleProxy<ITemperatureModule>();

            Console.WriteLine("Press <ENTER> to exit..");
            Console.ReadLine();
        }
    }
}
