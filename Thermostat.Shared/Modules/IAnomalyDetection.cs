using Microsoft.Azure.TypeEdge.Attributes;
using Microsoft.Azure.TypeEdge.Modules.Endpoints;
using ThermostatApplication.Messages;

namespace ThermostatApplication.Modules
{
    [TypeModule]
    public interface IAnomalyDetection
    {
        Output<Anomaly> Anomaly { get; set; }
    }
}