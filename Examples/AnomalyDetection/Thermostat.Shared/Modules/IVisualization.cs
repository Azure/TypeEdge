using Microsoft.Azure.TypeEdge.Attributes;
using Microsoft.Azure.TypeEdge.Twins;
using ThermostatApplication.Twins;

namespace ThermostatApplication.Modules
{
    [TypeModule]
    public interface IVisualization
    {
        ModuleTwin<VisualizationTwin> Twin { get; set; }
    }
}