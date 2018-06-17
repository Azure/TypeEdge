using Microsoft.Azure.IoT.TypeEdge.Attributes;
using Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.IoT.TypeEdge.Twins;
using ThermostatApplication.Messages;
using ThermostatApplication.Twins;

namespace ThermostatApplication.Modules
{
    [TypeModule]
    public interface ITemperatureSensor
    {
        Output<Temperature> Temperature { get; set; }
        ModuleTwin<TemperatureTwin> Twin { get; set; }

        bool ResetSensor(int sensitivity);
    }
}