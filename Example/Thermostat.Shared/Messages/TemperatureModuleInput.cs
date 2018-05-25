using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using Newtonsoft.Json;

namespace ThermostatApplication.Messages
{
    public class TemperatureModuleInput : IEdgeMessage
    {
        public int MyData { get; set; }
        public IDictionary<string, string> Properties { get; set; }

        public byte[] GetBytes()
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
        }

        public void SetBytes(byte[] bytes)
        {
            var obj = JsonConvert.DeserializeObject<TemperatureModuleInput>(Encoding.UTF8.GetString(bytes));
            Properties = obj.Properties;
            MyData = obj.MyData;
        }
    }
}