using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ThermostatApplication.Messages;
using ThermostatApplication.Messages.Visualization;
using ThermostatApplication.Modules;
using ThermostatApplication.Twins;
using TypeEdge.Enums;
using TypeEdge.Modules;
using TypeEdge.Modules.Enums;
using TypeEdge.Modules.Messages;
using TypeEdge.Twins;
using VisualizationWeb;

namespace Modules
{
    public class Visualization : EdgeModule, IVisualization
    {
        object _sync = new object();

        IWebHost _webHost;
        HubConnection _connection;
        Dictionary<string, Chart> _chartDataDictionary;

        public ModuleTwin<VisualizationTwin> Twin { get; set; }

        public override CreationResult Configure(IConfigurationRoot configuration)
        {
            _webHost = new WebHostBuilder()
                .UseConfiguration(configuration)
                .UseKestrel()
                .UseContentRoot(Path.Combine(Directory.GetCurrentDirectory()))
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            _connection = new HubConnectionBuilder().WithUrl($"{configuration["server.urls"].Split(';')[0]}/visualizerhub").Build();
            
            return base.Configure(configuration);
        }

        public Visualization(IOrchestrator proxy)
        {
            _chartDataDictionary = new Dictionary<string, Chart>();

            proxy.Visualization.Subscribe(this, async (e) =>
            {
                await RenderAsync(e);
                return MessageResult.Ok;
            });

            Twin.Subscribe(async twin =>
            {
                Console.WriteLine($"{typeof(Visualization).Name}::Twin update");

                ConfigureCharts(twin);
                return TwinResult.Ok;
            });
        }

        private void ConfigureCharts(VisualizationTwin twin)
        {
            if (twin == null|| string.IsNullOrEmpty(twin.ChartName))
                return;
            lock (_sync)
                _chartDataDictionary[twin.ChartName] = new Chart()
                {
                    Append = twin.Append,
                    Headers = new string[2] { twin.XAxisLabel, twin.YAxisLabel },
                    Name = twin.ChartName,
                    X_Label = twin.XAxisLabel,
                    Y_Label = twin.YAxisLabel
                };
        }

        public override async Task<ExecutionResult> RunAsync()
        {
            ConfigureCharts(await Twin.GetAsync());
            await _webHost.StartAsync();
            await _connection.StartAsync();
            return await base.RunAsync();
        }

        private async Task RenderAsync(GraphData data)
        {
            Chart chartConfig;
            lock (_sync)
            {
                if (!_chartDataDictionary.ContainsKey(data.CorrelationID))
                    return;
                chartConfig = _chartDataDictionary[data.CorrelationID];
            }

            var visualizationMessage = new VisualizationMessage();
            visualizationMessage.messages = new ChartData[1];
            var chartData = new ChartData();

            chartData.Chart = chartConfig;
            chartData.Points = data.Values;
            chartData.IsAnomaly = data.Anomaly;

            visualizationMessage.messages[0] = chartData;

            // Todo:  make this an in-proc call, rather than SignalR

            await _connection.InvokeAsync("SendInput", JsonConvert.SerializeObject(visualizationMessage));

        }
    }
}
