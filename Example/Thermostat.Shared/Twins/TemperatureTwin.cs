using Microsoft.Azure.IoT.TypeEdge.Twins;

namespace ThermostatApplication.Twins
{
    public class TemperatureTwin : TypeModuleTwin
    {
        public double DesiredMaximum { get; set; }
        public double DesiredMinimum { get; set; }
    }
}