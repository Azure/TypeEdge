using Microsoft.Azure.TypeEdge.Attributes;
using Microsoft.Azure.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.TypeEdge.Twins;
using Shared.Messages;
using Shared.Twins;

namespace Shared
{
    [TypeModule]
    public interface ITypeEdgeModule
    {
        Output<TypeEdgeModuleOutput> Output { get; set; }
        ModuleTwin<TypeEdgeModuleTwin> Twin { get; set; }
    }
}