using Microsoft.Azure.IoT.TypeEdge.Twins;

namespace ThermostatApplication.Twins
{
    public enum Routing {
        None, 
        Train, 
        Detect, 
        Both
    }
    public class PreprocessorTwin : TypeModuleTwin
    {
        public TemperatureScale Scale { get; set; }

        public Routing RoutingMode { get; set; }
    }
}