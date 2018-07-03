using Microsoft.Azure.IoT.TypeEdge.Modules.Messages;

namespace ThermostatApplication.Messages
{
    public class Sample : EdgeMessage
    {
        public Temperature[] Data { get; set; }
    }
}
