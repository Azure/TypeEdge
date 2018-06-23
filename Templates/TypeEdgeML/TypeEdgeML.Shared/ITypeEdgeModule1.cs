using Microsoft.Azure.IoT.TypeEdge.Attributes;
using Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.IoT.TypeEdge.Twins;
using TypeEdgeML.Shared.Messages;
using TypeEdgeML.Shared.Twins;

namespace TypeEdgeML.Shared
{
    [TypeModule]
    public interface ITypeEdgeModule1
    {
        Output<TypeEdgeModule1Output> Output { get; set; }
        ModuleTwin<TypeEdgeModule1Twin> Twin { get; set; }

        bool ResetModule(int sensorThreshold);
    }
}