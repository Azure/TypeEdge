using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.IoT.TypeEdge.Enums;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using Microsoft.Azure.IoT.TypeEdge.Modules.Enums;
using Microsoft.Azure.IoT.TypeEdge.Modules.Messages;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using ThermostatApplication.Messages;
using ThermostatApplication.Modules;
using VisualizationWeb;

namespace Modules
{
    public class Visualization : EdgeModule, IVisualization
    {
        IWebHost _webHost;  

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

        private async Task AddSampleAsync(Temperature e)
        {
            
        }
    }
}
