using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.IoT.TypeEdge.Modules;

namespace ThermostatApplication.Twins
{
    public class TemperatureTwin : TypeModuleTwin
    {
        public int? MaxLimit { get; set; }
    }
}