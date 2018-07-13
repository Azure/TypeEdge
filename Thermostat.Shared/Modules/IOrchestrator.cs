using TypeEdge.Attributes;
using TypeEdge.Modules.Endpoints;
using TypeEdge.Twins;
using ThermostatApplication.Messages;
using ThermostatApplication.Twins;

namespace ThermostatApplication.Modules
{
    [TypeModule]
    public interface IOrchestrator
    {
        Output<Temperature> Training { get; set; }
        Output<Temperature> Detection { get; set; }
        Output<VisualizationData> Visualization { get; set; }

        ModuleTwin<OrchestratorTwin> Twin { get; set; }
    }
}