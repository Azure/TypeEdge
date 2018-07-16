using System;
using TypeEdge.Modules.Messages;
using System.Collections.Generic;
using System.Text;

namespace ThermostatApplication.Messages
{
    // Update will be created whenever a user wants to provide an update. All they have to do after
    // The initial startup cost is provide the points and whether or not they're anomalous.
    public class GraphData : EdgeMessage
    {
        public Boolean Anomaly { get; set; }
        public double[] Values { get; set; }
        public string CorrelationID { get; set; }
    }
}
