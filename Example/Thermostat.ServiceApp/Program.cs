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
            ThermostatApplication.Twins.Routing routing = ThermostatApplication.Twins.Routing.None;
            while (true)
            {
                Console.WriteLine("Select Processor routing mode : (N)one, (T)rain, (D)etect, (B)oth");
                var res = Console.ReadLine();
                switch (res.ToUpper())
                {
                    case "T":
                        routing = ThermostatApplication.Twins.Routing.Train;
                        break;
                    case "N":
                        break;
                    case "D":
                        routing = ThermostatApplication.Twins.Routing.Detect;
                        break;
                    case "B":
                        routing = ThermostatApplication.Twins.Routing.Both;
                        break;
                    default:
                        continue;
                }
                break;
            }
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings_thermostat.json")
                .AddEnvironmentVariables()
                .Build();

            ProxyFactory.Configure(configuration["IotHubConnectionString"],
                configuration["DeviceId"]);

            var normalizer = ProxyFactory.GetModuleProxy<IPreprocessor>();

            var twin = await normalizer.Twin.GetAsync();
            twin.Scale = TemperatureScale.Celsius;
            twin.RoutingMode = routing;
            await normalizer.Twin.PublishAsync(twin);

            //var result = ProxyFactory.GetModuleProxy<ITemperatureSensor>().ResetSensor(10);

            Console.WriteLine("Press <ENTER> to exit..");
            Console.ReadLine();
        }
    }
}