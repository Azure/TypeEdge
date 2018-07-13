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
        HubConnection _connection;

        public override CreationResult Configure(IConfigurationRoot configuration)
        {
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

        private async Task RenderAsync(VisualizationData data)
        {
            // Todo: Generalize this!
            // Parse the chart and the update into an understandable message, then send it 

            VisMessage visualizationMessage = new VisMessage();
            Message m1 = new Message();
            visualizationMessage.messages = new Message[1]; // This method only allows for a single message, but users could create their own with more if needed

            m1.chartName = data.Metadata.Y_Label;
            m1.xlabel = data.Metadata.X_Label;
            m1.ylabel = data.Metadata.Y_Label;
            m1.headers = data.Metadata.Headers;
            m1.append = data.Metadata.Append;

            m1.points = new double[1][];
            m1.points[0] = data.Data.Values;
            m1.anomaly = data.Data.Anomaly;

            visualizationMessage.messages[0] = m1;

            // Todo: Directly make this call, rather than using SignalR to do it
            await _connection.InvokeAsync("SendInput", JsonConvert.SerializeObject(visualizationMessage));
        }
    }
}
