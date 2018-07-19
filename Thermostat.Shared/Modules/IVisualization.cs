using TypeEdge.Attributes;
using TypeEdge.Modules.Endpoints;
using TypeEdge.Twins;
using ThermostatApplication.Messages;
using ThermostatApplication.Twins;

namespace ThermostatApplication.Modules
{
    [TypeModule]
    public interface IVisualization 
    {
        ModuleTwin<VisualizationTwin> Twin { get; set; }
    }
}