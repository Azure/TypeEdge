using Microsoft.Azure.IoT.TypeEdge;
using Microsoft.Azure.IoT.TypeEdge.Host;
using Microsoft.Extensions.Configuration;
using Modules;
using System;
using System.Threading.Tasks;
using ThermostatApplication.Modules;

namespace ThermostatApplication.EdgeHost
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings_thermostat.json")
                .Build();

            var edgeApp = new Microsoft.Azure.IoT.TypeEdge.Host.TypeEdgeHost(configuration);

            edgeApp.RegisterModule<ITemperatureModule, TemperatureModule>();
            edgeApp.RegisterModule<INormalizeTemperatureModule, NormalizeTemperatureModule>();
            edgeApp.Build();

            await edgeApp.RunAsync();

            Console.WriteLine("Press <ENTER> to exit..");
            Console.ReadLine();
        }
    }
}
