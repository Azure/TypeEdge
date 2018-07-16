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

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WaveGenerator;
using Microsoft.Extensions.Configuration;

namespace Modules
{
    public class TemperatureSensor : EdgeModule, ITemperatureSensor
    {
        object _sync = new object();
        double _anomalyOffset = 0.0;
        double _minimum = 60.0;
        double _maximum = 80.0;
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

            _dataGenerator = new WaveGenerator.WaveGenerator(_waveConfiguration);
            
        }

        // This method connects to a url for SignalR to send data.
        public static async Task<HubConnection> ConnectAsync(string baseUrl)
        {
            // Keep trying to until we can start
            while (true)
            {
                var connection = new HubConnectionBuilder()
                                .WithUrl(baseUrl)
                                .Build();
                try
                {
                    await connection.StartAsync();
                    return connection;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    await Task.Delay(1000);
                }
            }
        }
     
        public void GenerateAnomaly(int value)
        {
            Console.WriteLine($"GenerateAnomaly called with value:{value}");
            lock (_sync)
                _anomalyOffset = value;
        }
        public override async Task<ExecutionResult> RunAsync()
        {

            var dateTime = new DateTime();
            while (true)
            {
                var newValue = _dataGenerator.Read();
                PublishResult publishResult = await Temperature.PublishAsync(new Temperature()
                {
                    Value = newValue,
                    TimeStamp = dateTime.Millisecond
                });


                await Task.Delay((int) (1.0 / _sampleRateHz));
            }
        }
    }
}