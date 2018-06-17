
using Microsoft.Azure.IoT.TypeEdge.Modules.Messages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace ThermostatApplication.Messages
{
    public class Temperature : EdgeMessage
    {
        public TemperatureScale Scale { get; set; }
        public double Value { get; set; }
    }
}