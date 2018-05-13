using Microsoft.Azure.IoT.TypeEdge.Attributes;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using ThermostatApplication.Messages;
using ThermostatApplication.Twins;

namespace ThermostatApplication.Modules
{
    [TypeModule(Name = "NormalizeTemperature")]
    public interface INormalizeTemperatureModule
    {
        Output<TemperatureModuleOutput> NormalizedTemperature { get; set; }

        ModuleTwin<NormalizerTwin> Twin { get; set; }
    }
}