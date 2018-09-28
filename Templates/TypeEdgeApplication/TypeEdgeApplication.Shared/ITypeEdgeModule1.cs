using Microsoft.Azure.TypeEdge.Attributes;
using Microsoft.Azure.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.TypeEdge.Twins;
using TypeEdgeApplication.Shared.Messages;
using TypeEdgeApplication.Shared.Twins;

namespace TypeEdgeApplication.Shared
{
    [TypeModule]
    public interface ITypeEdgeModule1
    {
        Output<TypeEdgeModule1Output> Output { get; set; }
        ModuleTwin<TypeEdgeModule1Twin> Twin { get; set; }

        bool ResetModule(int sensorThreshold);
    }
}