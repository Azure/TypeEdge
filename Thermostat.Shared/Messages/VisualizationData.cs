
using TypeEdge.Modules.Messages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace ThermostatApplication.Messages
{
    public class VisualizationData : EdgeMessage
    {
        public Chart Metadata { get; set; }
        public GraphData Data { get; set; }
    }
}
