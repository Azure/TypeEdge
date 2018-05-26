using Microsoft.Azure.IoT.TypeEdge.Attributes;
using Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.IoT.TypeEdge.Twins;
using TypeEdgeApplication.Shared.Messages;
using TypeEdgeApplication.Shared.Twins;

namespace TypeEdgeApplication.Shared
{
    [TypeModule(Name = "TypeEdgeModule1")]
    public interface ITypeEdgeModule1
    {
        Output<TypeEdgeModule1Output> Output { get; set; }
        ModuleTwin<TypeEdgeModule1Twin> Twin { get; set; }

        bool ResetModule(int sensorThreshold);
    }
}