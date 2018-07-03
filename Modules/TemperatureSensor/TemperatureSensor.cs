using System;
using System.Threading;
using System.Threading.Tasks;
using TypeEdge.Enums;
using TypeEdge.Modules;
using TypeEdge.Modules.Endpoints;
using TypeEdge.Modules.Enums;
using TypeEdge.Twins;
using ThermostatApplication;
using ThermostatApplication.Messages;
using ThermostatApplication.Modules;
using ThermostatApplication.Twins;

namespace Modules
{
    public class TemperatureSensor : EdgeModule, ITemperatureSensor
    {
        object _sync = new object();
        double _anomalyOffset = 0.0;
        double _minimum = 60.0;
        double _maximum = 80.0;


        public Output<Temperature> Temperature { get; set; }
        public ModuleTwin<TemperatureTwin> Twin { get; set; }

        public TemperatureSensor()
        {
            Twin.Subscribe(async twin =>
            {
                lock (_sync)
                {
                    _minimum = twin.DesiredMaximum;
                    _maximum = twin.DesiredMaximum;
                }
                await Twin.ReportAsync(twin);
                return TwinResult.Ok;
            });
        }

        public void GenerateAnomaly(int value)
        {
            Console.WriteLine($"GenerateAnomaly called with value:{value}");
            lock (_sync)
                _anomalyOffset = value;
        }
        public override async Task<ExecutionResult> RunAsync()
        {
            double frequency = 2.0;
            int amplitute = 5;
            int samplingRate = 100;

            while (true)
            {
                Temperature message = null;
                lock (_sync)
                {
                    var sin = Math.Sin(2 * Math.PI * frequency * DateTime.Now.TimeOfDay.TotalSeconds);
                    var value = amplitute * sin + (_maximum + _minimum) / 2 + _anomalyOffset;
                    _anomalyOffset = 0.0;

                    message = new Temperature
                    {
                        Scale = TemperatureScale.Celsius,
                        Value = value,
                        Minimum = _minimum,
                        Maximum = _maximum
                    };

                    int left = 40;
                    var text = new string('-', (int)((value - +(_maximum + _minimum) / 2) / amplitute * left / 2) + left);
                    Console.WriteLine($"{value.ToString("F2")} {text}");
                }
                await Temperature.PublishAsync(message);

                Thread.Sleep(1000 / samplingRate);
            }
            return await base.RunAsync();
        }
    }
}