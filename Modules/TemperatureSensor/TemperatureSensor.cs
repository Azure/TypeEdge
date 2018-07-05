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
            double frequency = 2.0;
            int amplitute = 5;
            int samplingRate = 100;
            // Connect to Host
            HubConnection connection = await ConnectAsync("http://127.0.0.1:5000/visualizerhub");
            int i = 0;
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
                    //var text = new string('-', (int)((value - +(_maximum + _minimum) / 2) / amplitute * left / 2) + left);
                    //Console.WriteLine($"{value.ToString("F2")} {text}");
                    //Console.WriteLine(i);
                    var val = new int[] { i, ((int)((value - +(_maximum + _minimum) / 2) / amplitute * left / 2) + left), 3 };
                    var headers = new string[] { "Timestamp", "Value1", "Value2" };
                    connection.InvokeAsync("SendInput", headers, val);
                    i = i + 1;
                    System.Threading.Thread.Sleep(50);
                }
                await Temperature.PublishAsync(message);

                Thread.Sleep(1000 / samplingRate);
            }
            return await base.RunAsync();
        }

    }
}