using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.TypeEdge.Enums;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.IoT.TypeEdge.Twins;
using ThermostatApplication;
using ThermostatApplication.Messages;
using ThermostatApplication.Modules;
using ThermostatApplication.Twins;

namespace Modules
{
    public class TemperatureSensor : EdgeModule, ITemperatureSensor
    {
        public Output<Temperature> Temperature { get; set; }
        public ModuleTwin<TemperatureTwin> Twin { get; set; }

        public bool ResetSensor(int sensitivity)
        {
            Console.WriteLine($"ResetSensor called with sensitivity:{sensitivity}");
            return true;
        }

        public override async Task<ExecutionResult> RunAsync()
        {
            double frequency = 0.5;
            int offset = 70;
            int amplitute = 10;
            int samplingRate = 25;

            while (true)
            {
                var sin = Math.Sin(2 * Math.PI * frequency * DateTime.Now.TimeOfDay.TotalSeconds);
                var value = amplitute
                    * sin
                    + offset;

                await Temperature.PublishAsync(new Temperature
                {
                    Scale = TemperatureScale.Celsius,
                    Value = value
                });


                int left = 40;
                left = (int)(sin * left) + left;
                var text = new string('-', left);
                Console.WriteLine($"{value.ToString("F2")} {text}");

                Thread.Sleep(1000 / samplingRate);
            }
            return await base.RunAsync();
        }
    }
}