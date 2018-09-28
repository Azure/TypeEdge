using System;
using System.Threading.Tasks;
using Microsoft.Azure.TypeEdge.Proxy;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ThermostatApplication.Modules;
using ThermostatApplication.Twins;

namespace Thermostat.ServiceApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json")
                .AddEnvironmentVariables()
                .Build();

            ProxyFactory.Configure(configuration["IotHubConnectionString"],
                configuration["DeviceId"]);


            while (true)
            {
                Console.WriteLine("Select Action: (T)emperatureSensorTwin, (E)xit");

                var res = Console.ReadLine()?.ToUpper();
                switch (res)
                {
                    case "T":
                        await SetTemperatureSensorTwin();
                        break;
                    case "E":
                        return;
                }
            }
        }

        private static async Task SetTemperatureSensorTwin()
        {
            Console.WriteLine("Set Default?(Y/N)");
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
                twin.WaveType = (WaveformType) Enum.Parse(typeof(WaveformType), res);

            Console.WriteLine($"Set the VerticalShift:{twin.Offset}");
            res = Console.ReadLine();
            if (!string.IsNullOrEmpty(res))
                twin.Offset = double.Parse(res);


            Console.WriteLine(JsonConvert.SerializeObject(twin, Formatting.Indented));
            await temperatureSensor.Twin.PublishAsync(twin);
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
    }
}