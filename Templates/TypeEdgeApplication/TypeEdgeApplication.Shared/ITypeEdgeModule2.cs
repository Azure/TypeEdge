using Microsoft.Azure.IoT.TypeEdge.Attributes;
using Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.IoT.TypeEdge.Twins;
using TypeEdgeApplication.Shared.Messages;
using TypeEdgeApplication.Shared.Twins;

namespace TypeEdgeApplication.Shared
{
    [TypeModule]
    public interface ITypeEdgeModule2
    {
        Output<TypeEdgeModule2Output> Output { get; set; }
        Input<TypeEdgeModule1Output> Input { get; set; }
        ModuleTwin<TypeEdgeModule2Twin> Twin { get; set; }
    }
}