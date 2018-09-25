using Microsoft.Azure.TypeEdge.Attributes;
using Microsoft.Azure.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.TypeEdge.Twins;
using ThermostatApplication.Messages;
using ThermostatApplication.Twins;
using Microsoft.Azure.TypeEdge.Modules.Messages;

namespace ThermostatApplication.Modules
{
    [TypeModule]
    public interface IOrchestrator
    {
        Output<Temperature> Training { get; set; }
        Output<Temperature> Detection { get; set; }
        Output<GraphData> Visualization { get; set; }
        Output<Model> Model { get; set; }
        ModuleTwin<OrchestratorTwin> Twin { get; set; }
    }
}