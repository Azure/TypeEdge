using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.IoT.TypeEdge.Modules;

namespace ThermostatApplication.Twins
{
    public class NormalizerTwin : IModuleTwin
    {
        public TemperatureScale? Scale { get; set; }

        public Twin GetTwin()
        {
            return new Twin(new TwinProperties()
            {
                Desired = new TwinCollection() { }
            });
        }

        public void SetTwin(Twin twin)
        {

        }
    }
}