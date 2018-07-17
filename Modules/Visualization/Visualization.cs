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
using ThermostatApplication.Messages.Visualization;
using ThermostatApplication.Twins;
using TypeEdge.Twins;

namespace Modules
{
    public class Visualization : EdgeModule, IVisualization
    {
        IWebHost _webHost;
        HubConnection _connection;
        Dictionary<string, Chart> _graphDataDictionary;

        public ModuleTwin<VisualizationTwin> Twin { get; set; }

        public override CreationResult Configure(IConfigurationRoot configuration)
        {
            _graphDataDictionary = new Dictionary<string, Chart>();
            /* Hi Archer, I hope you're having a wonderful day
             * Here's where I've hardcoded stuff. If you could write the twin to prompt the user
             * about the fields so they could update it, that'd be awesome! We might want to 
             * add something to detect how large the array is, too. */
            
            _graphDataDictionary["IOrchestrator.Sampling"] = new Chart()
            {
                Append = false,
                Headers = new string[2] { "TS", "val1" },
                Name = "IOrchestrator.Sampling",
                X_Label = "Timestamp",
                Y_Label = "Value"

            };
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
            await _connection.StartAsync();
            // You need to have a graph already registered to use this function (which is why hardcoding is bad)


            // Parse the chart and the update into an understandable message, then send it 
            if (_graphDataDictionary.ContainsKey(data.CorrelationID))
            {
                var chartConfig = _graphDataDictionary[data.CorrelationID];

                var visualizationMessage = new VisualizationMessage();
                visualizationMessage.messages = new ChartData[1];
                var chartData = new ChartData();

                chartData.Chart = chartConfig;
                chartData.Points = data.Values;
                chartData.IsAnomaly = data.Anomaly;

                visualizationMessage.messages[0] = chartData;

                // Todo: Directly make this call, rather than using SignalR to do it
                await _connection.InvokeAsync("SendInput", JsonConvert.SerializeObject(visualizationMessage));
            }

        }
    }
}
