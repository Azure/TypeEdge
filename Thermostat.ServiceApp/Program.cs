using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using TypeEdge.Proxy;
using Microsoft.Extensions.Configuration;
using ThermostatApplication;
using ThermostatApplication.Modules;
using ThermostatApplication.Twins;
using ThermostatApplication.Messages;
using ThermostatApplication.Messages.Visualization;

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
                Console.WriteLine("Select Action: (T)emperatureSensorTwin, (O)rchestratorTwin, (V)isualizerTwin, (A)nomaly, (E)xit");

                var res = Console.ReadLine();
                switch (res.ToUpper())
                {
                    case "O":
                        await SetOrchestratorTwin();
                        break;
                    case "T":
                        await SetTemperatureSensorTwin();
                        break;
                    case "V":
                        await SetVisualizerTwin();
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

        private static async Task SetTemperatureSensorTwin()
        {
            var temperatureSensor = ProxyFactory.GetModuleProxy<ITemperatureSensor>();

            TemperatureTwin twin = await temperatureSensor.Twin.GetAsync();

            Console.WriteLine($"Set Default?(Y/N)");
            var res = Console.ReadLine();
            if (!string.IsNullOrEmpty(res) && res.ToUpper() == "Y")
            {
                twin.SamplingHz = 5;
                //twin.Waveform = new Waveform();
                twin.Amplitude = 10;
                twin.Frequency = 2;
                twin.WaveType = WaveformType.Sine;
                twin.VerticalShift = 60;

                await temperatureSensor.Twin.PublishAsync(twin);
            }
        }

        private static async Task SetOrchestratorTwin()
        {
            var routing = PromptRoutingMode();

            var processor = ProxyFactory.GetModuleProxy<IOrchestrator>();

            var twin = await processor.Twin.GetAsync();
            twin.Scale = TemperatureScale.Celsius;

            twin.RoutingMode = routing;
            await processor.Twin.PublishAsync(twin);
        }


        private static async Task SetVisualizerTwin()
        {
            var routing = PromptRoutingMode();

            var processor = ProxyFactory.GetModuleProxy<IVisualization>();

            var twin = await processor.Twin.GetAsync();


            Console.WriteLine("Enter Chart name:");
            twin.ChartName = Console.ReadLine();
            Console.WriteLine("Enter x-axis label");
            twin.XAxisLabel = Console.ReadLine();
            Console.WriteLine("Enter y-axis label");
            twin.YAxisLabel = Console.ReadLine();
            Dictionary<int, string> Headers = new Dictionary<int, string>();
            string Header = "";
            int count = 0;
            do
            {
                Console.WriteLine("Enter next series header, or None to finish");
                Header = Console.ReadLine();
                Headers.Add(count, Header);
                count++;
            } while (!string.IsNullOrEmpty(Header));
            twin.Headers = Headers;
            Console.WriteLine("Enter \"F\" or \"f\" to replace all chart data upon reception of new data package. Otherwise data will be shifted in a rolling window.");
            string Append = Console.ReadLine();
            Append.ToUpper();
            if (!string.IsNullOrEmpty(Append))
            {
                if (Append.Equals("F"))
                {
                    twin.Append = false;
                }
                else
                {
                    twin.Append = true;
                }
            }
            else
            {
                twin.Append = true;
            }

            await processor.Twin.PublishAsync(twin);
        }

        private static Routing PromptRoutingMode()
        {
            Routing result = 0;

            Console.WriteLine("Select Processor routing modes (multiple choices are allowed) : (S)ampling, (D)etect, (V)isualize, (F)eatureExtraction,  empty for none");
            var res = Console.ReadLine();
            if (!string.IsNullOrEmpty(res))
            {
                if (res.Contains("S"))
                    result |= Routing.Sampling;
                if (res.Contains("D"))
                    result |= Routing.Detect;
                if (res.Contains("V"))
                    result |= Routing.VisualizeSource;
                if (res.Contains("F"))
                    result |= Routing.FeatureExtraction;

            }
            return result;
        }
    }
}