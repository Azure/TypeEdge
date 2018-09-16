using System.Threading.Tasks;
using Microsoft.Azure.Devices.Edge.Hub.Service;
using TypeEdge.Enums;
using TypeEdge.Modules;
using TypeEdge.Modules.Enums;
using Microsoft.Extensions.Configuration;
using Agent = Microsoft.Azure.Devices.Edge.Agent.Core;
using System.Threading;

namespace TypeEdge.Host.Hub
{
    internal class EdgeHub : TypeModule
    {
        public override string Name => Agent.Constants.EdgeHubModuleIdentityName;
        private IConfigurationRoot HubServiceConfiguration { get; set; }

        public override InitializationResult Init()
        {
            HubServiceConfiguration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            return InitializationResult.Ok;
        }

        public override async Task<ExecutionResult> RunAsync(CancellationToken cancellationToken)
        {
            if (await Program.MainAsync(HubServiceConfiguration) == 0)
                return ExecutionResult.Ok;
            return ExecutionResult.Error;
        }
    }
}