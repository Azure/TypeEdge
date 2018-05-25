using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.IoT.TypeEdge.Modules.Messages
{
    public class JsonMessage : IEdgeMessage
    {
        private string _jsonData;

        public JsonMessage(string data)
        {
            _jsonData = data;
        }

        public IDictionary<string, string> Properties { get; set; }

        public byte[] GetBytes()
        {
            return Encoding.UTF8.GetBytes(_jsonData);
        }

        public void SetBytes(byte[] bytes)
        {
            _jsonData = Encoding.UTF8.GetString(bytes);
        }
    }
}