using Microsoft.Azure.IoT.TypeEdge.Modules;
using Shared.Messages;
using Shared.Twins;

namespace Shared
{
    public interface ITypeEdgeModule
    {
        Output<TypeEdgeModuleOutput> Output { get; set; }
        ModuleTwin<TypeEdgeModuleTwin> Twin { get; set; }
    }
}