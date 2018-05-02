namespace Microsoft.Azure.IoT.EdgeCompose
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