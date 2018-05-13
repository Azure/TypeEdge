using Microsoft.Azure.IoT.TypeEdge;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ThermostatApplication.Messages
{
    public class TemperatureModuleOutput : IEdgeMessage
    {
        public TemperatureScale Scale { get; set; }
        public double Temperature { get; set; }
        public IDictionary<string, string> Properties { get; set; }

        public byte[] GetBytes()
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
        }

        public void SetBytes(byte[] bytes)
        {
            var obj = JsonConvert.DeserializeObject<TemperatureModuleOutput>(Encoding.UTF8.GetString(bytes));
            Properties = obj.Properties;
            Scale = obj.Scale;
            Temperature = obj.Temperature;
        }
    }
}