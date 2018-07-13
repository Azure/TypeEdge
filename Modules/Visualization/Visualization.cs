using Microsoft.AspNetCore.Hosting;
using TypeEdge.Enums;
using TypeEdge.Modules;
using TypeEdge.Modules.Enums;
using TypeEdge.Modules.Messages;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using ThermostatApplication.Messages;
using ThermostatApplication.Modules;
using VisualizationWeb;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Modules
{
    public class Visualization : EdgeModule, IVisualization
    {
        IWebHost _webHost;
        HubConnection connection;

        public override CreationResult Configure(IConfigurationRoot configuration)
        {
            _webHost = new WebHostBuilder()
                .UseConfiguration(configuration)
                .UseKestrel()
                .UseContentRoot(Path.Combine(Directory.GetCurrentDirectory()))
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            return base.Configure(configuration);
        }
        public override async Task<ExecutionResult> RunAsync()
        {
            await _webHost.RunAsync();
            //Set up the connection
            connection = await ConnectAsync("http://127.0.0.1:5000/visualizerhub");
            return ExecutionResult.Ok;
        }
        public Visualization(IOrchestrator proxy)
        {
            proxy.Visualization.Subscribe(this, async (e) =>
            {
                await AddSampleAsync(e);
                return MessageResult.Ok;
            });
        }

        // This method connects to a url for SignalR to send data.
        public static async Task<HubConnection> ConnectAsync(string baseUrl)
        {
            // Keep trying to until we can start
            while (true)
            {
                var connection = new HubConnectionBuilder().WithUrl(baseUrl).Build();
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

        // Send an array of messages and whether or not they are anomalies to get back an upate object.
        public static Update CreateUpdate(Temperature[] Temperatures, Boolean anomaly)
        {
            Update update = new Update
            {
                anomaly = anomaly
            };
            double[] pointsArray = new double[Temperatures.Length];
            for(int i = 0; i < Temperatures.Length; i++)
            {
                pointsArray[i] = Temperatures[i].Value;
            }
            update.points = pointsArray;
            return update;
        }

        // Sets up a new chart
        public static Chart SetupChart(String chartName, String xlabel, String ylabel, String[] headers, Boolean append)
        {
            Chart newChart = new Chart
            {
                chartName = chartName,
                xlabel = xlabel,
                ylabel = ylabel,
                headers = headers,
                append = append
            };
            return newChart;
        }

        private async Task AddSampleAsync(VisualizationData data)
        {
            // Todo: Generalize this!
            // Parse the chart and the update into an understandable message, then send it 

            VisMessage visualizationMessage = new VisMessage();
            Message m1 = new Message();
            visualizationMessage.messages = new Message[1]; // This method only allows for a single message, but users could create their own with more if needed

            m1.chartName = data.chart.ylabel;
            m1.xlabel = data.chart.xlabel;
            m1.ylabel = data.chart.ylabel;
            m1.headers = data.chart.headers;
            m1.append = data.chart.append;

            m1.points = new double[1][];
            m1.points[0] = data.update.points;
            m1.anomaly = data.update.anomaly;

            visualizationMessage.messages[0] = m1;

            // Todo: Directly make this call, rather than using SignalR to do it
            await connection.InvokeAsync("SendInput", JsonConvert.SerializeObject(visualizationMessage));

            /*
            VisMessage visMessages = new VisMessage();
            Message m1 = new Message();
            visMessages.messages = new Message[10];

            m1.points = new double[10][];
            for (int i = 0; i < 10; i++)
            {
                
            }
            m1.xlabel = "Timestamp";
            m1.ylabel = "Value";
            m1.headers = new string[]
            {
                    "Timestamp",
                    "value1",
                    "Val2"
            };
            m1.append = true;
            m1.chartName = "Chart1";
            for (int i = 0; i < 10; i++)
            {
                visMessages.messages[i] = m1;
            }

            await connection.InvokeAsync("SendInput", JsonConvert.SerializeObject(visMessages));
            await Task.Delay(100);*/
        }
    }
}
