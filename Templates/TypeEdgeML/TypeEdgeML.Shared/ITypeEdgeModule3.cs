using Microsoft.Azure.IoT.TypeEdge.Attributes;
using Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.IoT.TypeEdge.Twins;
using TypeEdgeML.Shared.Messages;
using TypeEdgeML.Shared.Twins;

namespace TypeEdgeML.Shared
{
    [TypeModule]
    public interface ITypeEdgeModule3
    {
        Output<TypeEdgeModule3Output> Output { get; set; }
        Input<TypeEdgeModule2Output> Input { get; set; }
        ModuleTwin<TypeEdgeModule3Twin> Twin { get; set; }
    }
}