using Microsoft.Azure.TypeEdge.Attributes;
using Microsoft.Azure.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.TypeEdge.Twins;
using ThermostatApplication.Messages;
using ThermostatApplication.Twins;

namespace ThermostatApplication.Modules
{
    [TypeModule]
    public interface ITemperatureSensor
    {
        Output<Temperature> Temperature { get; set; }
        ModuleTwin<TemperatureTwin> Twin { get; set; }

        void GenerateAnomaly(int value);
    }
}