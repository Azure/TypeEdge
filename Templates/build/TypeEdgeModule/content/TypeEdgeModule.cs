using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.TypeEdge.Enums;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.IoT.TypeEdge.Twins;
using Shared;
using Shared.Messages;
using Shared.Twins;

namespace Modules
{
    public class TypeEdgeModule : EdgeModule, ITypeEdgeModule
    {
        public Output<TypeEdgeModuleOutput> Output { get; set; }
        public ModuleTwin<TypeEdgeModuleTwin> Twin { get; set; }

        public override async Task<ExecutionResult> RunAsync()
        {
            while (true)
            {
                await Output.PublishAsync(new TypeEdgeModuleOutput());
                Thread.Sleep(1000);
            }

            return await base.RunAsync();
        }
    }
}