using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.IoT.TypeEdge;
using Microsoft.Azure.IoT.TypeEdge.Hubs;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using Newtonsoft.Json;

namespace ThermpostatEdgeApplication
{
    public class TemperatureModuleInput : IEdgeMessage
    {
        public int MyData { get; set; }
        public IDictionary<string, string> Properties { get; set; }

        public byte[] GetBytes()
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
        }
    }
}