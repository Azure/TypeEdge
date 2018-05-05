using Microsoft.Azure.IoT.EdgeCompose;
using Microsoft.Azure.IoT.EdgeCompose.Hubs;
using Microsoft.Azure.IoT.EdgeCompose.Modules;

namespace ThermpostatEdgeApplication
{
    public class TemperatureModuleInput : IModuleMessage
    {
        private JsonMessage msg;

        public TemperatureModuleInput(JsonMessage msg)
        {
            this.msg = msg;
        }
    }
}