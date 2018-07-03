using TypeEdge.Attributes;
using TypeEdge.Modules.Endpoints;
using TypeEdge.Twins;
using TypeEdgeML.Shared.Messages;
using TypeEdgeML.Shared.Twins;

namespace TypeEdgeML.Shared
{
    [TypeModule]
    public interface ITypeEdgeModule3
    {
        Output<TypeEdgeModule3Output> Output { get; set; }
        ModuleTwin<TypeEdgeModule3Twin> Twin { get; set; }
    }
}