using System.Threading;
using System.Threading.Tasks;
using TypeEdge.Enums;
using TypeEdge.Modules;
using TypeEdge.Modules.Endpoints;
using TypeEdge.Twins;
using Shared;
using Shared.Messages;
using Shared.Twins;

namespace Modules
{
    public class TypeEdgeModule : EdgeModule, ITypeEdgeModule
    {
        public Output<TypeEdgeModuleOutput> Output { get; set; }
        public ModuleTwin<TypeEdgeModuleTwin> Twin { get; set; }

        public override async Task<ExecutionResult> RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Output.PublishAsync(new TypeEdgeModuleOutput());
                await Task.Delay(1000);
            }    
            return ExecutionResult.Ok;
        }
    }
}