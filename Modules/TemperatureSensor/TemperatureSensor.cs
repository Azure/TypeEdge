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
        double _samplingRateHz = 0.0;

        WaveGenerator.WaveGenerator _dataGenerator;

        public Output<Temperature> Temperature { get; set; }
        public ModuleTwin<TemperatureTwin> Twin { get; set; }

        public TemperatureSensor()
        {
            Twin.Subscribe(async twin =>
            {
                Console.WriteLine($"TemperatureSensor:: new twin update");

                ConfigureGenerator(twin);
                await Twin.ReportAsync(twin);
                return TwinResult.Ok;
            });
        }


        void ConfigureGenerator(TemperatureTwin twin)
        {
            lock (_sync)
            {
                if (twin == null
                    || twin.SamplingHz <= 0
                    || twin.Amplitude <= 0
                    || twin.Frequency <= 0)
                    return;
                _samplingRateHz = twin.SamplingHz;
                var waveConfiguration = new WaveConfig[] { new WaveConfig() {
                    Amplitude = twin.Amplitude,
                    Frequency = twin.Frequency,
                    WaveType = (WaveType)(int)twin.WaveType,
                    VerticalShift = twin.VerticalShift,
                } };
                _dataGenerator = new WaveGenerator.WaveGenerator(waveConfiguration);
            }

        }
        public void GenerateAnomaly(int value)
        {
            Console.WriteLine($"TemperatureSensor::GenerateAnomaly called with value:{value}");

            lock (_sync)
                _anomalyOffset = value;
        }

        public override async Task<ExecutionResult> RunAsync()
        {
            var twin = Twin.GetAsync().Result;
            ConfigureGenerator(twin);

            while (true)
            {
                double newValue;
                double offset;
                double sleepTimeMs = 0;
                if (_dataGenerator != null)
                {
                    lock (_sync)
                    {
                        offset = _anomalyOffset;
                        newValue = _dataGenerator.Read();
                        sleepTimeMs = 1.0 / _samplingRateHz;
                        _anomalyOffset = 0.0;
                    }

                    PublishResult publishResult = await Temperature.PublishAsync(new Temperature()
                    {
                        Value = newValue + offset,
                        TimeStamp = DateTime.Now.Millisecond
                    });
                }
                await Task.Delay((int)sleepTimeMs);
            }
            return ExecutionResult.Ok;
        }
    }
}