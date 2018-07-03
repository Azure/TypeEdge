using Microsoft.Azure.IoT.TypeEdge.Attributes;
using Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.IoT.TypeEdge.Twins;
using ThermostatApplication.Messages;
using ThermostatApplication.Twins;

namespace ThermostatApplication.Modules
{
    [TypeModule]
    public interface IOrchestrator
    {
        Output<Temperature> Training { get; set; }
        Output<Temperature> Detection { get; set; }
        Output<Temperature> Visualization { get; set; }

        ModuleTwin<OrchestratorTwin> Twin { get; set; }
    }
}