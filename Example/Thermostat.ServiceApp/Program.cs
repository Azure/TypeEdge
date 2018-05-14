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

            TestDirectMethods(configuration);
            //await TestTwins(configuration);

            Console.WriteLine("Press <ENTER> to exit..");
            Console.ReadLine();
        }

        private static async Task TestTwins(IConfigurationRoot configuration)
        {
            var normalizer = ProxyFactory.GetModuleProxy<INormalizeTemperatureModule>(
                            configuration["IotHubConnectionString"],
                            configuration["DeviceId"]);

            var twin = await normalizer.Twin.GetAsync();
            twin.Scale = ThermostatApplication.TemperatureScale.Celsius;
            await normalizer.Twin.PublishAsync(twin);
        }

        private static void TestDirectMethods(IConfigurationRoot configuration)
        {
            var temeprature = ProxyFactory.GetModuleProxy<ITemperatureModule>(
                            configuration["IotHubConnectionString"],
                            configuration["DeviceId"]);

            var result = temeprature.ResetSensor(10);
        }
    }
}
