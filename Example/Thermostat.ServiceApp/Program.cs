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

            var module = TypeEdgeProxy.GetModuleProxy<INormalizeTemperatureModule>(configuration["IotHubConnectionString"], configuration["DeviceId"]);

            var twin = await module.Twin.GetAsync();
            twin.Scale = ThermostatApplication.TemperatureScale.Celsius;
            await module.Twin.PublishAsync(twin);

            Console.WriteLine("Press <ENTER> to exit..");
            Console.ReadLine();
        }
    }
}
