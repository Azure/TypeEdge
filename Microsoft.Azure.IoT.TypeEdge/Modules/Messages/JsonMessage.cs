using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.IoT.TypeEdge.Modules;

namespace Microsoft.Azure.IoT.TypeEdge.Hubs
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

        public void SetBytes(byte[] bytes)
        {
            JsonData =  Encoding.UTF8.GetString(bytes);
        }
    }
}