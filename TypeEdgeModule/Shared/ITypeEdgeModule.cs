using TypeEdge.Attributes;
using TypeEdge.Modules.Endpoints;
using TypeEdge.Twins;
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