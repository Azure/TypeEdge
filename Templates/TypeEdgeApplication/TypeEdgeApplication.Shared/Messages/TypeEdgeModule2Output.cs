using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.TypeEdge.Modules.Messages;
using Newtonsoft.Json;

namespace TypeEdgeApplication.Shared.Messages
{
    public class TypeEdgeModule2Output : IEdgeMessage
    {
        public string Data { get; set; }
        public string Metadata { get; set; }
        public IDictionary<string, string> Properties { get; set; }

        public byte[] GetBytes()
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
        }

        public void SetBytes(byte[] bytes)
        {
            var obj = JsonConvert.DeserializeObject<TypeEdgeModule1Output>(Encoding.UTF8.GetString(bytes));
            Properties = obj.Properties;

            Data = obj.Data;
        }
    }
}