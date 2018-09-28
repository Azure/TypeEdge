using Microsoft.Azure.TypeEdge.Modules.Messages;

namespace ThermostatApplication.Messages
{
    public class Temperature : EdgeMessage
    {
        public double Value { get; set; }
        public double TimeStamp { get; set; }
    }
}