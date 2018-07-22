using System;
using System.Collections.Generic;
using System.Text;
using TypeEdge.Modules.Messages;

namespace ThermostatApplication.Messages
{
    public class Model : EdgeMessage
    {
        public Algorithm Algorithm { get; set; }
        public string DataJson { get; set; }
    }
}
