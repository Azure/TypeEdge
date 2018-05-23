using System.Threading.Tasks;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using Microsoft.Azure.IoT.TypeEdge;
using Shared.Messages;
using Shared.Twins;
using Shared;

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
                await Output.PublishAsync(new TypeEdgeModuleOutput() { });
                System.Threading.Thread.Sleep(1000);
            }
            return await base.RunAsync();
        }
    }
}
