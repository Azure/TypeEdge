using Microsoft.Azure.IoT.TypeEdge;
using Microsoft.Azure.IoT.TypeEdge.Host;
using Microsoft.Extensions.Configuration;
using Modules;
using System;
using System.Threading.Tasks;
using ThermostatApplication.Modules;

namespace ThermostatApplication
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings_thermostat.json")
                .Build();

            var host = new TypeEdgeHost(configuration);

            host.RegisterModule<ITemperatureModule, TemperatureModule>();
            host.RegisterModule<INormalizeTemperatureModule, NormalizeTemperatureModule>();

            host.Build();

            await host.RunAsync();

            Console.WriteLine("Press <ENTER> to exit..");
            Console.ReadLine();
        }
    }
}
