using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.TypeEdge.Modules.Messages
{
    public abstract class EdgeMessage : IEdgeMessage
    {
        [JsonIgnore] public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        public byte[] GetBytes()
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
        }

        public void SetBytes(byte[] bytes)
        {
            var obj = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(bytes), GetType());

            this.CopyFrom(obj);
        }
    }
}