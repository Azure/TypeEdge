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
    public class TypeEdgeModuleVsCode : TypeModule, ITypeEdgeModuleVsCode
    {
        public Output<TypeEdgeModuleVsCodeOutput> Output { get; set; }
        public ModuleTwin<TypeEdgeModuleVsCodeTwin> Twin { get; set; }

        public override async Task<ExecutionResult> RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Output.PublishAsync(new TypeEdgeModuleVsCodeOutput());
                await Task.Delay(1000);
            }    
            return ExecutionResult.Ok;
        }
    }
}