using Microsoft.Azure.TypeEdge.Attributes;
using Microsoft.Azure.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.TypeEdge.Twins;
using TypeEdgeApplication.Shared.Messages;
using TypeEdgeApplication.Shared.Twins;

namespace TypeEdgeApplication.Shared
{
    [TypeModule]
    public interface ITypeEdgeModule2
    {
        Output<TypeEdgeModule2Output> Output { get; set; }
        ModuleTwin<TypeEdgeModule2Twin> Twin { get; set; }
    }
}