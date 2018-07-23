using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using ThermostatApplication;
using ThermostatApplication.Modules;
using ThermostatApplication.Twins;
using TypeEdge.Proxy;

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
                Console.WriteLine("Select Action: (D)efault Twins, (T)emperatureSensorTwin, (O)rchestratorTwin, (V)isualizerTwin, (A)nomaly, (M)odelTrainer, (E)xit");

                var res = Console.ReadLine()?.ToUpper();
                switch (res)
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
                    case "M":
                        await SetTrainerDefaults();
                        return;
                    case "E":
                        return;
                    case "D":
                        await SetDefaultsAsync();
                        break;
                    default:
                        break;

                }
            }
        }

        private static async Task SetDefaultsAsync()
        {
            //the order matters, reverse dependencies
            await SetVisualizerDefaults();
            await SetTrainerDefaults();
            await SetOrchestratorDefaults();
            await SetTemperatureDefaults();
        }

        private static async Task SetTemperatureSensorTwin()
        {
            Console.WriteLine($"Set Default?(Y/N)");
            var res = Console.ReadLine()?.ToUpper();
            if (res == "Y")
            {
                await SetTemperatureDefaults();
                return;
            }
            var temperatureSensor = ProxyFactory.GetModuleProxy<ITemperatureSensor>();
            var twin = await temperatureSensor.Twin.GetAsync();

            Console.WriteLine($"Set the SamplingHz:{twin.SamplingHz}");
            res = Console.ReadLine();
            if (!string.IsNullOrEmpty(res))
                twin.SamplingHz = double.Parse(res);

            Console.WriteLine($"Set the Amplitude:{twin.Amplitude}");
            res = Console.ReadLine();
            if (!string.IsNullOrEmpty(res))
                twin.Amplitude = double.Parse(res);

            Console.WriteLine($"Set the Frequency:{twin.Frequency}");
            res = Console.ReadLine();
            if (!string.IsNullOrEmpty(res))
                twin.Frequency = double.Parse(res);

            Console.WriteLine($"Set the WaveType:{twin.WaveType.ToString()}");
            res = Console.ReadLine();
            if (!string.IsNullOrEmpty(res))
                twin.WaveType = (WaveformType)Enum.Parse(typeof(WaveformType), res);

            Console.WriteLine($"Set the VerticalShift:{twin.Offset}");
            res = Console.ReadLine();
            if (!string.IsNullOrEmpty(res))
                twin.Offset = double.Parse(res);


            Console.WriteLine(JsonConvert.SerializeObject(twin, Formatting.Indented));
            await temperatureSensor.Twin.PublishAsync(twin);
        }

        private static async Task SetOrchestratorTwin()
        {
            Console.WriteLine($"Set Default?(Y/N)");
            var res = Console.ReadLine()?.ToUpper();
            if (res == "Y")
            {
                await SetOrchestratorDefaults();
                return;
            }
            var routing = PromptRoutingMode();

            var processor = ProxyFactory.GetModuleProxy<IOrchestrator>();

            var twin = await processor.Twin.GetAsync();
            twin.Scale = TemperatureScale.Celsius;

            twin.RoutingMode = routing;
            await processor.Twin.PublishAsync(twin);
        }

        private static async Task SetVisualizerTwin()
        {
            Console.WriteLine($"Set Default?(Y/N)");
            var res = Console.ReadLine()?.ToUpper();
            if (res == "Y")
            {
                await SetVisualizerDefaults();
                return;
            }

            var routing = PromptRoutingMode();

            var processor = ProxyFactory.GetModuleProxy<IVisualization>();

            var twin = await processor.Twin.GetAsync();

            Console.WriteLine("Enter Chart name:");
            twin.ChartName = Console.ReadLine();
            Console.WriteLine("Enter x-axis label");
            twin.XAxisLabel = Console.ReadLine();
            Console.WriteLine("Enter y-axis label");
            twin.YAxisLabel = Console.ReadLine();

            Console.WriteLine("Enter \"F\" to replace all chart data upon reception of new data package. Otherwise data will be shifted in a rolling window.");
            string Append = Console.ReadLine()?.ToUpper();

            if (Append == ("F"))
                twin.Append = false;
            else
                twin.Append = true;

            await processor.Twin.PublishAsync(twin);
        }

        private static Routing PromptRoutingMode()
        {
            Routing result = 0;

            Console.WriteLine("Select Processor routing modes (multiple choices are allowed) : (T)rain, (D)etect, Visualize(S)ource, (F)eatureExtraction, (V)isualizeFeature,  empty for None");
            var res = Console.ReadLine()?.ToUpper();
            if (!string.IsNullOrEmpty(res))
            {
                if (res.Contains("T"))
                    result |= Routing.Train;
                if (res.Contains("D"))
                    result |= Routing.Detect;
                if (res.Contains("S"))
                    result |= Routing.VisualizeSource;
                if (res.Contains("V"))
                    result |= Routing.VisualizeFeature;
                if (res.Contains("F"))
                    result |= Routing.FeatureExtraction;
            }
            return result;
        }

        private static async Task SetVisualizerDefaults()
        {
            Console.WriteLine("Setting VisualizerDefaults");

            var processor = ProxyFactory.GetModuleProxy<IVisualization>();

            var twin = await processor.Twin.GetAsync();

            twin.Append = true;
            twin.ChartName = "Source";
            twin.XAxisLabel = "Timestamp";
            twin.YAxisLabel = "Value";

            var res = await processor.Twin.PublishAsync(twin);
            Console.WriteLine(JsonConvert.SerializeObject(twin, Formatting.Indented));
        }
        private static async Task SetTemperatureDefaults()
        {
            Console.WriteLine("Setting TemperatureDefaults");

            var temperatureSensor = ProxyFactory.GetModuleProxy<ITemperatureSensor>();

            var twin = await temperatureSensor.Twin.GetAsync();
            twin.SamplingHz = 10;
            twin.Amplitude = 10;
            twin.Frequency = 2;
            twin.WaveType = WaveformType.Sine;
            twin.Offset = 60;

            var res = await temperatureSensor.Twin.PublishAsync(twin);
            Console.WriteLine(JsonConvert.SerializeObject(twin, Formatting.Indented));
        }
        private static async Task SetTrainerDefaults()
        {
            Console.WriteLine("Setting TrainerDefaults");

            var dataTrainer = ProxyFactory.GetModuleProxy<IModelTraining>();

            var twin = await dataTrainer.Twin.GetAsync();
            twin.TumblingWindowPercentage = 10;
            twin.AggregationSize = 100;

            var res = await dataTrainer.Twin.PublishAsync(twin);
            Console.WriteLine(JsonConvert.SerializeObject(res, Formatting.Indented));
        }
        private static async Task SetOrchestratorDefaults()
        {
            Console.WriteLine("Setting Orchestrator");

            var processor = ProxyFactory.GetModuleProxy<IOrchestrator>();

            var twin = await processor.Twin.GetAsync();
            twin.Scale = TemperatureScale.Celsius;
            twin.RoutingMode = Routing.VisualizeSource | Routing.Train | Routing.Detect;

            var res = await processor.Twin.PublishAsync(twin);
            Console.WriteLine(JsonConvert.SerializeObject(res, Formatting.Indented));
        }
    }
}