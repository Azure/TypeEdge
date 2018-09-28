using Microsoft.Azure.TypeEdge.Modules.Messages;

namespace ThermostatApplication.Messages
{
    public class Anomaly : EdgeMessage
    {
        public Temperature Temperature { get; set; }
    }
}