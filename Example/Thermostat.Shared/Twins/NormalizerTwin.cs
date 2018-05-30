using Microsoft.Azure.IoT.TypeEdge.Twins;

namespace ThermostatApplication.Twins
{
    public class NormalizerTwin : TypeModuleTwin
    {
        public TemperatureScale Scale { get; set; }
    }
}