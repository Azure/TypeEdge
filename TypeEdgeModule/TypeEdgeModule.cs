using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.TypeEdge.Enums;
using Microsoft.Azure.TypeEdge.Modules;
using Microsoft.Azure.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.TypeEdge.Twins;
using Shared;
using Shared.Messages;
using Shared.Twins;

namespace Modules
{
    public class TypeEdgeModule : TypeModule, ITypeEdgeModule
    {
        public Output<TypeEdgeModuleOutput> Output { get; set; }
        public ModuleTwin<TypeEdgeModuleTwin> Twin { get; set; }

        public override async Task<ExecutionResult> RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Output.PublishAsync(new TypeEdgeModuleOutput());
                await Task.Delay(1000, cancellationToken);
            }

            return ExecutionResult.Ok;
        }
    }
}