using TypeEdge.Attributes;
using TypeEdge.Modules.Endpoints;
using TypeEdge.Twins;
using ThermostatApplication.Messages;
using ThermostatApplication.Twins;
using TypeEdge.Modules.Messages;

namespace ThermostatApplication.Modules
{
    [TypeModule]
    public interface IOrchestrator
    {
        Output<Temperature> Training { get; set; }
        Output<Temperature> Detection { get; set; }
        Output<GraphData> Visualization { get; set; }
        Output<Reference<Model>> Model { get; set; }
        ModuleTwin<OrchestratorTwin> Twin { get; set; }
    }
}