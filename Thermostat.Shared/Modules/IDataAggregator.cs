using TypeEdge.Attributes;
using TypeEdge.Modules.Endpoints;
using TypeEdge.Modules.Messages;
using ThermostatApplication.Messages;
using TypeEdge.Twins;
using ThermostatApplication.Twins;

namespace ThermostatApplication.Modules
{
    [TypeModule]
    public interface IDataAggregator 
    {
        Output<Reference<DataAggregate>> Aggregate { get; set; }
        ModuleTwin<DataAggregatorTwin> Twin { get; set; }

    }
}