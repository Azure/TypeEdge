
using TypeEdge.Modules.Messages;
using ThermostatApplication.Messages.Visualization;

namespace ThermostatApplication.Messages
{
    public class VisualizationData : EdgeMessage
    {
        public Chart Metadata { get; set; }
        public GraphData Data { get; set; }
    }
}
