using Microsoft.Azure.IoT.TypeEdge.Twins;

namespace ThermostatApplication.Twins
{
    public class TemperatureTwin : TypeModuleTwin
    {
        public int MaxLimit { get; set; }
    }
}