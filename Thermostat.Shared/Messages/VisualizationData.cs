
using TypeEdge.Modules.Messages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace ThermostatApplication.Messages
{
    public class VisualizationData : EdgeMessage
    {
        public Chart chart { get; set; }
        public Update update { get; set; }
    }
}
