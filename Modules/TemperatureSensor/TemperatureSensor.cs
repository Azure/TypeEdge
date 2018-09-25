using System;
using System.Threading.Tasks;
using Microsoft.Azure.TypeEdge.Enums;
using Microsoft.Azure.TypeEdge.Modules;
using Microsoft.Azure.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.TypeEdge.Modules.Enums;
using Microsoft.Azure.TypeEdge.Twins;
using ThermostatApplication.Messages;
using ThermostatApplication.Modules;
using ThermostatApplication.Twins;
using WaveGenerator;
using Newtonsoft.Json;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Modules
{
    public class TemperatureSensor : TypeModule, ITemperatureSensor
    {
        readonly object _sync = new object();
        readonly DateTime _startTimeStamp;
        //default values
        double _anomalyOffset = 0.0;
        double _samplingRateHz = 1.0;

        WaveGenerator.WaveGenerator _dataGenerator;

        public Output<Temperature> Temperature { get; set; }
        public ModuleTwin<TemperatureTwin> Twin { get; set; }

        public TemperatureSensor()
        {
            _startTimeStamp = DateTime.Now;

            Twin.Subscribe(async twin =>
            {
                Logger.LogInformation($"Twin update");

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
                    FrequencyInKilohertz = twin.Frequency/1000,
                    WaveType = (WaveType)(int)twin.WaveType,
                    Offset = twin.Offset,
                } };

                _dataGenerator = new WaveGenerator.WaveGenerator(waveConfiguration);
            }
        }
        public void GenerateAnomaly(int value)
        {
            Logger.LogInformation($"GenerateAnomaly called with value:{value}");            

            lock (_sync)
                _anomalyOffset = value;
        }

        public override async Task<ExecutionResult> RunAsync(CancellationToken cancellationToken)
        {
            var twin = await Twin.GetAsync();
            ConfigureGenerator(twin);

            while (!cancellationToken.IsCancellationRequested)
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
                        sleepTimeMs = 1000.0 / _samplingRateHz;
                        _anomalyOffset = 0.0;
                    }
                    var message = new Temperature()
                    {
                        Value = newValue + offset,
                        TimeStamp = DateTime.Now.Subtract(_startTimeStamp).TotalMilliseconds / 1000
                    };

                    await Temperature.PublishAsync(message);
                }
                await Task.Delay((int)sleepTimeMs);
            }
            return ExecutionResult.Ok;
        }
    }
}