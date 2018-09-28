using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.TypeEdge.Modules.Messages;
using Newtonsoft.Json;

namespace Shared.Messages
{
    public class TypeEdgeModuleOutput : IEdgeMessage
    {
        public string Data { get; set; }
        public IDictionary<string, string> Properties { get; set; }

        public byte[] GetBytes()
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
        }

        public void SetBytes(byte[] bytes)
        {
            var obj = JsonConvert.DeserializeObject<TypeEdgeModuleOutput>(Encoding.UTF8.GetString(bytes));
            Properties = obj.Properties;

            Data = obj.Data;
        }
    }
}