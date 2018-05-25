using Microsoft.Azure.IoT.TypeEdge.Modules;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace TypeEdgeApplication.Shared.Messages
{
    public class TypeEdgeModule2Output : IEdgeMessage
    {
        public IDictionary<string, string> Properties { get; set; }

        public string Data { get; set; }
        public string Metadata { get; set; }

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