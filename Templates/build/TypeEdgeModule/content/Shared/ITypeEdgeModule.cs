using Microsoft.Azure.IoT.TypeEdge.Attributes;
using Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.IoT.TypeEdge.Twins;
using Shared.Messages;
using Shared.Twins;

namespace Shared
{
    [TypeModule(Name = "TypeEdgeModule")]
    public interface ITypeEdgeModule
    {
        Output<TypeEdgeModuleOutput> Output { get; set; }
        ModuleTwin<TypeEdgeModuleTwin> Twin { get; set; }
    }
}