using Microsoft.Azure.TypeEdge.Modules.Messages;

namespace ThermostatApplication.Messages
{
    public class DataAggregate : EdgeMessage
    {
        public double[][] Values { get; set; }
        public double SamplingRateHz { get; set; }
        public string CorrelationID { get; set; }
    }
}