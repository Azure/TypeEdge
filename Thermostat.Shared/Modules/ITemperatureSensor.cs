using TypeEdge.Attributes;
using TypeEdge.Modules.Endpoints;
using TypeEdge.Twins;
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