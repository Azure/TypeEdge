using System;
using System.Threading.Tasks;
using TypeEdge.Enums;
using TypeEdge.Modules;
using TypeEdge.Modules.Endpoints;
using TypeEdge.Modules.Enums;
using TypeEdge.Twins;
using ThermostatApplication.Messages;
using ThermostatApplication.Modules;
using ThermostatApplication.Twins;

using Microsoft.AspNetCore.SignalR.Client;
using WaveGenerator;
namespace Modules
{
    public class TemperatureSensor : EdgeModule, ITemperatureSensor
    {
        object _sync = new object();
        double _anomalyOffset = 0.0;
        double _minimum = 60.0;
        double _maximum = 80.0;
        double _sampleRateHz = 100.0;
        WaveGenerator.WaveGenerator _dataGenerator;
        WaveConfig[] _waveConfiguration = new WaveConfig[] {
             new WaveConfig()
        };

        public Output<Temperature> Temperature { get; set; }
        public ModuleTwin<TemperatureTwin> Twin { get; set; }

        public TemperatureSensor()
        {
            _dataGenerator = new WaveGenerator.WaveGenerator(_waveConfiguration);

            Twin.Subscribe(async twin =>
            {
                lock (_sync)
                {
                    _minimum = twin.DesiredMaximum;
                    _maximum = twin.DesiredMaximum;
                    _sampleRateHz = twin.SampleRateHz;
                    _waveConfiguration = new WaveConfig[] {
                        twin.WaveConfig
                    };
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
            double i = 0;
            while (true)
            {
                var newValue = _dataGenerator.Read();
                PublishResult publishResult = await Temperature.PublishAsync(new Temperature()
                {
                    Value = newValue,
                    TimeStamp = i
                });

                var t3 = Task.Delay(1000);
                await Task.WhenAll(t3);
                i += 1;
            }
        }
    }
}