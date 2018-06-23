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
            var configuration = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json")
              .AddEnvironmentVariables()
              .Build();

            ProxyFactory.Configure(configuration["IotHubConnectionString"],
                configuration["DeviceId"]);


            while (true)
            {
                Console.WriteLine("Select Action: (T)win, (A)nomaly, (E)xit");

                var res = Console.ReadLine();
                switch (res.ToUpper())
                {
                    case "T":
                        await SetTwin();
                        break;
                    case "A":
                        ProxyFactory.GetModuleProxy<ITemperatureSensor>().GenerateAnomaly(40);
                        break;
                    case "E":
                        return;
                    default:
                        break;

                }
            }
        }

        private static async Task SetTwin()
        {
            ThermostatApplication.Twins.Routing routing = PromptRoutingMode();

            var processor = ProxyFactory.GetModuleProxy<IPreprocessor>();

            var twin = await processor.Twin.GetAsync();
            twin.Scale = TemperatureScale.Celsius;
            twin.RoutingMode = routing;
            await processor.Twin.PublishAsync(twin);
        }

        private static ThermostatApplication.Twins.Routing PromptRoutingMode()
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

            return routing;
        }
    }
}