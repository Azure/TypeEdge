using Microsoft.Azure.IoT.TypeEdge.Modules;

namespace ThermostatApplication.Twins
{
    public class NormalizerTwin : TypeModuleTwin
    {
        public TemperatureScale Scale { get; set; }
    }
}