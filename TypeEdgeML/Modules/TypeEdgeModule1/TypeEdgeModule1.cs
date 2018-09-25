using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.TypeEdge.Enums;
using Microsoft.Azure.TypeEdge.Modules;
using Microsoft.Azure.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.TypeEdge.Twins;
using TypeEdgeML.Shared;
using TypeEdgeML.Shared.Messages;
using TypeEdgeML.Shared.Twins;
using Microsoft.Extensions.Logging;

namespace Modules
{
    public class TypeEdgeModule1 : TypeModule, ITypeEdgeModule1
    {
        public Output<TypeEdgeModule1Output> Output { get; set; }
        public ModuleTwin<TypeEdgeModule1Twin> Twin { get; set; }

        public bool ResetModule(int sensorThreshold)
        {
            Logger.LogInformation($"New sensor threshold:{sensorThreshold}");
            return true;
        }

        public override async Task<ExecutionResult> RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Output.PublishAsync(new TypeEdgeModule1Output {Data = new Random().NextDouble().ToString(CultureInfo.InvariantCulture)});
                Logger.LogInformation($"Generated Message");
                await Task.Delay(1000);
            }
            return ExecutionResult.Ok;
        }
    }
}