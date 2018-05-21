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
            twin.Scale = ThermostatApplication.TemperatureScale.Celsius;
            await normalizer.Twin.PublishAsync(twin);

            var result = ProxyFactory.GetModuleProxy<ITemperatureModule>().ResetSensor(10);

            Console.WriteLine("Press <ENTER> to exit..");
            Console.ReadLine();
        }
    }
}
