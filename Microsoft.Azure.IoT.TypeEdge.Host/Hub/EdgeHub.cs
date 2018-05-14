using Autofac;
using Microsoft.Azure.Devices.Edge.Hub.Service;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Agent = Microsoft.Azure.Devices.Edge.Agent.Core;

namespace Microsoft.Azure.IoT.TypeEdge.Host.Hub
{
    public class EdgeHub : EdgeModule
    {
        internal override string Name => Agent.Constants.EdgeHubModuleIdentityName;
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