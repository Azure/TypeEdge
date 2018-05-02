using Microsoft.Azure.IoT.EdgeCompose;

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