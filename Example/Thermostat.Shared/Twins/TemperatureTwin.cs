using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.IoT.TypeEdge.Modules;

namespace ThermostatApplication.Twins
{
    public class TemperatureTwin : IModuleTwin
    {
        TwinCollection desiredProperties;
        public int MaxLimit { get; set; }
        public TwinCollection GetProperies()
        {
            return this.desiredProperties;
        }
        public void SetProperies(TwinCollection desiredProperties)
        {
            this.desiredProperties = desiredProperties;
        }
    }
}