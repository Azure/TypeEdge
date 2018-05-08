using Autofac;
using Microsoft.Azure.Devices.Edge.Hub.Service;
using Microsoft.Azure.IoT.EdgeCompose.Modules;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Agent = Microsoft.Azure.Devices.Edge.Agent.Core;

namespace Microsoft.Azure.IoT.EdgeCompose.Hubs
{
    public class EdgeHub : EdgeModule
    {
        public override string Name => Agent.Constants.EdgeHubModuleIdentityName;

        public Input<JsonMessage> Upstream { get; set; }
        public Output<JsonMessage> Downstream { get; set; }

        private IConfigurationRoot HubServiceConfiguration { get; set; }

        public override CreationResult Configure(IConfigurationRoot configuration)
        {
            HubServiceConfiguration = new ConfigurationBuilder()
               .AddJsonFile(Constants.ConfigFileName)
               .AddEnvironmentVariables()
               .Build();

            return CreationResult.OK;
        }

        public override async Task<ExecutionResult> RunAsync()
        {
            var res = await Program.MainAsync(HubServiceConfiguration);
            if (res == 0)
                return ExecutionResult.OK;
            return ExecutionResult.Error;
        }
    }
}