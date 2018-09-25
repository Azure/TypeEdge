
using Microsoft.Azure.TypeEdge.Modules.Messages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace ThermostatApplication.Messages
{
    public class Temperature : EdgeMessage
    {
        public double Value { get; set; }
        public double TimeStamp { get; set; }
    }
}