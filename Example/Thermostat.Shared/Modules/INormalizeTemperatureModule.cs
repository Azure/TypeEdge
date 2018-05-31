using Microsoft.Azure.IoT.TypeEdge.Attributes;
using Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.IoT.TypeEdge.Twins;
using ThermostatApplication.Messages;
using ThermostatApplication.Twins;

namespace ThermostatApplication.Modules
{
    [TypeModule]
    public interface INormalizeTemperatureModule
    {
        Output<TemperatureModuleOutput> NormalizedTemperature { get; set; }

        ModuleTwin<NormalizerTwin> Twin { get; set; }
    }
}