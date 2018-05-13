using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.IoT.TypeEdge.Modules;

namespace ThermostatApplication.Twins
{
    public class TemperatureTwin : IModuleTwin
    {
        private Twin twin;
        public int MaxLimit { get; set; }

        public Twin GetTwin()
        {
            //todo: set the properties
            return twin;
        }

        public void SetTwin(Twin twin)
        {
            //todo: set the properties
            this.twin = twin;
        }
    }
}