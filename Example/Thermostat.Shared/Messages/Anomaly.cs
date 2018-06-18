using Microsoft.Azure.IoT.TypeEdge.Modules.Messages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace ThermostatApplication.Messages
{
    public class Anomaly : EdgeMessage
    {
        public Temperature Temperature { get; set; }
    }
}
