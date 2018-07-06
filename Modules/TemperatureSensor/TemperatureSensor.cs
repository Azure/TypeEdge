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
            HubConnection connection = await ConnectAsync("http://127.0.0.1:5000/visualizerhub");

            //begin simple generator
            var comps = new WaveConfig[3];
            comps[0] = new WaveConfig(WaveType.Sine, 0.0001, 70);
            comps[1] = new WaveConfig(WaveType.Sine, 0.001, 30);
            comps[2] = new WaveConfig(WaveType.Flat, 1, 1)
            {
                VerticalShift = 130
            };
            var dataGenerator = new WaveGenerator.WaveGenerator(comps);

            //intitialize FFT object, which encapsulates the whole business
            FFT fft = new FFT(128, 10);

            var valueCounter = 0;
            while (true)
            {
                var newValue = dataGenerator.Read();
                VisClientMessage m = new VisClientMessage();
                m.NewVal.Headers = new String[]
                {
                    "Timestamp",
                    "Value"
                };

                m.NewVal.Inputs = new double[]
                {
                    valueCounter,
                    newValue
                };
             

                fft.Next(newValue);
                m.FFTResult = fft.curResult;
                m.SubtractionGraphResult = new double[128];

                for (int i = 0; i < 128; i++)
                {
                    m.SubtractionGraphResult[i] = 0;
                }

                await connection.InvokeAsync("SendInput", JsonConvert.SerializeObject(m));
                await Task.Delay(100);

                valueCounter++;

            }
        }
    }
}