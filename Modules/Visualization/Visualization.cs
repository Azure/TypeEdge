using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Azure.TypeEdge.Enums;
using Microsoft.Azure.TypeEdge.Modules;
using Microsoft.Azure.TypeEdge.Modules.Enums;
using Microsoft.Azure.TypeEdge.Modules.Messages;
using Microsoft.Azure.TypeEdge.Twins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ThermostatApplication.Messages;
using ThermostatApplication.Messages.Visualization;
using ThermostatApplication.Modules;
using ThermostatApplication.Twins;
using VisualizationWeb;

namespace Modules
{
    public class Visualization : TypeModule, IVisualization
    {
        private readonly Dictionary<string, Chart> _chartDataDictionary;
        private readonly IConfigurationRoot _configuration;
        private readonly object _sync = new object();
        private HubConnection _connection;
        private IWebHost _webHost;

        public Visualization(IOrchestrator proxy, IConfigurationRoot configuration)
        {
            _configuration = configuration;

            _chartDataDictionary = new Dictionary<string, Chart>();

            proxy.Visualization.Subscribe(this, async e =>
            {
                await RenderAsync(e);
                return MessageResult.Ok;
            });

            Twin.Subscribe(twin =>
            {
                Logger.LogInformation("Twin update");

                ConfigureCharts(twin);
                return Task.FromResult(TwinResult.Ok);
            });
        }

        public ModuleTwin<VisualizationTwin> Twin { get; set; }

        public override InitializationResult Init()
        {
            _webHost = new WebHostBuilder()
                .UseConfiguration(_configuration)
                .UseKestrel()
                .UseContentRoot(Path.Combine(Directory.GetCurrentDirectory()))
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            _connection = new HubConnectionBuilder()
                .WithUrl($"{_configuration["server.urls"].Split(';')[0]}/visualizerhub").Build();

            return base.Init();
        }

        private void ConfigureCharts(VisualizationTwin twin)
        {
            if (twin == null || string.IsNullOrEmpty(twin.ChartName))
                return;
            lock (_sync)
            {
                _chartDataDictionary[twin.ChartName] = new Chart
                {
                    Append = twin.Append,
                    Headers = new[] {twin.XAxisLabel, twin.YAxisLabel},
                    Name = twin.ChartName,
                    X_Label = twin.XAxisLabel,
                    Y_Label = twin.YAxisLabel
                };
            }
        }

        public override async Task<ExecutionResult> RunAsync(CancellationToken cancellationToken)
        {
            ConfigureCharts(await Twin.GetAsync());
            await _webHost.StartAsync(cancellationToken);
            await _connection.StartAsync(cancellationToken);
            return ExecutionResult.Ok;
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

            var visualizationMessage = new VisualizationMessage
            {
                messages = new ChartData[1]
            };
            var chartData = new ChartData
            {
                Chart = chartConfig,
                Points = data.Values,
                IsAnomaly = data.Anomaly
            };

            visualizationMessage.messages[0] = chartData;

            // Todo:  make this an in-proc call, rather than SignalR
            try
            {
                await _connection.InvokeAsync("SendInput", JsonConvert.SerializeObject(visualizationMessage));
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}