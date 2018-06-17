using Microsoft.Azure.IoT.TypeEdge.Attributes;
using Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.IoT.TypeEdge.Modules.Messages;
using ThermostatApplication.Messages;

namespace ThermostatApplication.Modules
{
    [TypeModule]
    public interface IAnomalyDetection 
    {
        Input<Temperature> Temperature { get; set; }
        Input<Reference<Sample>> Samples { get; set; }

        Output<Anomaly> Anomaly { get; set; }
    }
}