using System;
using TypeEdge.Modules.Messages;
using System.Collections.Generic;
using System.Text;

namespace ThermostatApplication.Messages
{
    public class GraphData : EdgeMessage
    {
        public Boolean Anomaly { get; set; }
        public double[][] Values { get; set; }
        public string CorrelationID { get; set; }
    }
}
