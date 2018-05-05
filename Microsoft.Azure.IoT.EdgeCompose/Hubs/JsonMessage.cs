using Microsoft.Azure.IoT.EdgeCompose.Modules;

namespace Microsoft.Azure.IoT.EdgeCompose.Hubs
{
    public class JsonMessage : IModuleMessage
    {
        private string JsonData;

        public JsonMessage(string data)
        {
            JsonData = data;
        }
    }
}