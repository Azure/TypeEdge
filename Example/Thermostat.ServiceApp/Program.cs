using System;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.TypeEdge.Proxy;
using Microsoft.Extensions.Configuration;
using ThermostatApplication;
using ThermostatApplication.Modules;

namespace Thermostat.ServiceApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Press <ENTER> to start..");
            Console.ReadLine();

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings_thermostat.json")
                .AddEnvironmentVariables()
                .Build();

            ProxyFactory.Configure(configuration["IotHubConnectionString"],
                configuration["DeviceId"]);

            var normalizer = ProxyFactory.GetModuleProxy<INormalizeTemperatureModule>();

            var twin = await normalizer.Twin.GetAsync();
            twin.Scale = TemperatureScale.Celsius;
            await normalizer.Twin.PublishAsync(twin);

            var result = ProxyFactory.GetModuleProxy<ITemperatureModule>().ResetSensor(10);

            Console.WriteLine("Press <ENTER> to exit..");
            Console.ReadLine();
        }
    }
}