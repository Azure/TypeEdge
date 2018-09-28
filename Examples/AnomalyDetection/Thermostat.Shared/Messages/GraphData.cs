using Microsoft.Azure.TypeEdge.Modules.Messages;

namespace ThermostatApplication.Messages
{
    public class GraphData : EdgeMessage
    {
        public bool Anomaly { get; set; }
        public double[][] Values { get; set; }
        public string CorrelationID { get; set; }
    }
}