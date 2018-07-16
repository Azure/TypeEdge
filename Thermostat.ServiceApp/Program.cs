using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using TypeEdge.Proxy;
using Microsoft.Extensions.Configuration;
using ThermostatApplication;
using ThermostatApplication.Modules;
using ThermostatApplication.Twins;

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
            var routing = PromptRoutingMode();

            var processor = ProxyFactory.GetModuleProxy<IOrchestrator>();

            var twin = await processor.Twin.GetAsync();
            twin.Scale = TemperatureScale.Celsius;
            twin.RoutingMode = routing;

            await processor.Twin.PublishAsync(twin);
        }

        private static Routing PromptRoutingMode()
        {
            Routing result = 0;

            Console.WriteLine("Select Processor routing modes (multiple choices are allowed) : (T)rain, (D)etect, (V)isualization, empty for none");
            var res = Console.ReadLine();
            if (!string.IsNullOrEmpty(res))
            {
                if (res.Contains("T"))
                    result |= Routing.Train;
                if (res.Contains("D"))
                    result |= Routing.Detect;
                if (res.Contains("V"))
                    result |= Routing.Visualize;
            }
            return result;
        }
    }
}