using Microsoft.Azure.IoT.TypeEdge.Modules;
using ThermostatApplication.Messages;

namespace ThermostatApplication.Modules
{
    public interface INormalizeTemperatureModule
    {
        Output<TemperatureModuleOutput> NormalizedTemperature { get; set; }
    }
}