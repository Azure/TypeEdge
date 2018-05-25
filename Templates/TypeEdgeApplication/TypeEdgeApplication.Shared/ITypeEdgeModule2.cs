using Microsoft.Azure.IoT.TypeEdge.Attributes;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using TypeEdgeApplication.Shared.Messages;
using TypeEdgeApplication.Shared.Twins;

namespace TypeEdgeApplication.Shared
{
    [TypeModule(Name = "TypeEdgeModule2")]
    public interface ITypeEdgeModule2
    {
        Output<TypeEdgeModule2Output> Output { get; set; }
        Input<TypeEdgeModule1Output> Input { get; set; }
        ModuleTwin<TypeEdgeModule2Twin> Twin { get; set; }
    }
}