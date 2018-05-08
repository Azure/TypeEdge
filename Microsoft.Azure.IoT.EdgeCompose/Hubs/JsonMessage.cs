using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.IoT.EdgeCompose.Modules;

namespace Microsoft.Azure.IoT.EdgeCompose.Hubs
{
    public class JsonMessage : IEdgeMessage
    {
        private string JsonData;

        public JsonMessage(string data)
        {
            JsonData = data;
        }

        public IDictionary<string, string> Properties { get; set; }

        public byte[] GetBytes()
        {
            return Encoding.UTF8.GetBytes(JsonData);
        }
    }
}