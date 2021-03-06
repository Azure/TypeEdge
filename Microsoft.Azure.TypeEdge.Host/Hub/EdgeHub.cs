﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Edge.Hub.Service;
using Microsoft.Azure.TypeEdge.Enums;
using Microsoft.Azure.TypeEdge.Modules;
using Microsoft.Azure.TypeEdge.Modules.Enums;
using Microsoft.Extensions.Configuration;
using Agent = Microsoft.Azure.Devices.Edge.Agent.Core;

namespace Microsoft.Azure.TypeEdge.Host.Hub
{
   
    internal class EdgeHub : TypeModule, IEdgeHub
    {
        public override string Name => Agent.Constants.EdgeHubModuleIdentityName;

        public override async Task<ExecutionResult> RunAsync(CancellationToken cancellationToken)
        {
            if (await Task.Run(() => Program.Main()) == 0)
                return ExecutionResult.Ok;
            return ExecutionResult.Error;
        }
    }
}