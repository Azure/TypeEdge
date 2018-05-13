using Microsoft.Azure.IoT.TypeEdge.Modules;
using ThermostatApplication.Messages;
using ThermostatApplication.Twins;

namespace ThermostatApplication.Modules
{
    public interface INormalizeTemperatureModule
    {
        Output<TemperatureModuleOutput> NormalizedTemperature { get; set; }

        ModuleTwin<NormalizerTwin> Twin { get; set; }
    }
}