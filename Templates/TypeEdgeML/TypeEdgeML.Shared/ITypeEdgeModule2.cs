using TypeEdge.Attributes;
using TypeEdge.Modules.Endpoints;
using TypeEdge.Twins;
using TypeEdgeML.Shared.Messages;
using TypeEdgeML.Shared.Twins;

namespace TypeEdgeML.Shared
{
    [TypeModule]
    public interface ITypeEdgeModule2
    {
        Output<TypeEdgeModule2Output> Output { get; set; }
        ModuleTwin<TypeEdgeModule2Twin> Twin { get; set; }
    }
}