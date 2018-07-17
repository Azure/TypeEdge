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
using WaveGenerator;

namespace Modules
{
    public class TemperatureSensor : EdgeModule, ITemperatureSensor
    {
        object _sync = new object();
        double _anomalyOffset = 0.0;
        double _sampleRateHz;

        WaveGenerator.WaveGenerator _dataGenerator;
        WaveGenerator.WaveConfig[] _waveConfiguration;

        public Output<Temperature> Temperature { get; set; }
        public ModuleTwin<TemperatureTwin> Twin { get; set; }

        public TemperatureSensor()
        {
            Twin.Subscribe(async twin =>
            {
                lock (_sync)
                {
                    _sampleRateHz = twin.SampleRateHz;
                    _waveConfiguration = new WaveConfig[] {
                        twin.WaveConfig
                    };
                }
                _dataGenerator = new WaveGenerator.WaveGenerator(_waveConfiguration);

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
            while (true)
            {
                double newValue;
                double offset;
                int sleepTimeMs = 0;

                lock (_sync)
                {
                    offset = _anomalyOffset;
                    newValue = _dataGenerator.Read();
                    sleepTimeMs = (int)(1.0 / _sampleRateHz);
                }

                PublishResult publishResult = await Temperature.PublishAsync(new Temperature()
                {
                    Value = newValue + offset,
                    TimeStamp = DateTime.Now.Millisecond
                });


                await Task.Delay(sleepTimeMs);
            }
        }
    }
}