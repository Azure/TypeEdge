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
using System.Collections.Generic;


namespace Modules
{
    public class Visualization : EdgeModule, IVisualization
    {
        IWebHost _webHost;
        HubConnection _connection;
        Dictionary<string, Chart> _graphDataDictionary;

        public override CreationResult Configure(IConfigurationRoot configuration)
        {
            _graphDataDictionary = new Dictionary<string, Chart>();
            _webHost = new WebHostBuilder()
                .UseConfiguration(configuration)
                .UseKestrel()
                .UseContentRoot(Path.Combine(Directory.GetCurrentDirectory()))
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            _connection = new HubConnectionBuilder().WithUrl("http://127.0.0.1:5000/visualizerhub").Build();

            return base.Configure(configuration);
        }
        public override async Task<ExecutionResult> RunAsync()
        {
            await _webHost.RunAsync();
            await _connection.StartAsync();

            return ExecutionResult.Ok;
        }
        public Visualization(IOrchestrator proxy)
        {
            proxy.Visualization.Subscribe(this, async (e) =>
            {
                await RenderAsync(e);
                return MessageResult.Ok;
            });
        }

        private void RegisterGraph(Chart metadata, string correlationID)
        {
            _graphDataDictionary[correlationID] = metadata;
        }

        private async Task RenderAsync(GraphData data)
        {
            // You need to have a graph already registered to use this function


            // Parse the chart and the update into an understandable message, then send it 
            if (_graphDataDictionary.ContainsKey(data.CorrelationID))
            {
                Chart chart = _graphDataDictionary[data.CorrelationID];
                VisualizationMessage visualizationMessage = new VisualizationMessage();
                RenderData m1 = new RenderData();
                visualizationMessage.messages = new RenderData[1];

                m1.chartName = chart.Name;
                m1.xlabel = chart.X_Label;
                m1.ylabel = chart.Y_Label;
                m1.headers = chart.Headers;
                m1.append = chart.Append;

                m1.points = new double[1][];
                m1.points[0] = data.Values;
                m1.anomaly = data.Anomaly;

                visualizationMessage.messages[0] = m1;

                // Todo: Directly make this call, rather than using SignalR to do it
                await _connection.InvokeAsync("SendInput", JsonConvert.SerializeObject(visualizationMessage));
            }

        }
    }
}
