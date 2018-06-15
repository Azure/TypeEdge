using Microsoft.Azure.IoT.TypeEdge.Attributes;
using Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.IoT.TypeEdge.Twins;
using TypeEdgeML.Shared.Messages;
using TypeEdgeML.Shared.Twins;

namespace TypeEdgeML.Shared
{
    [TypeModule]
    public interface ITypeEdgeModule2
    {
        Output<TypeEdgeModule2Output> Output { get; set; }
        Input<TypeEdgeModule1Output> Input { get; set; }
        ModuleTwin<TypeEdgeModule2Twin> Twin { get; set; }
    }
}