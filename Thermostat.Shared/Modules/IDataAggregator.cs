using TypeEdge.Attributes;
using TypeEdge.Modules.Endpoints;
using TypeEdge.Modules.Messages;
using ThermostatApplication.Messages;

namespace ThermostatApplication.Modules
{
    [TypeModule]
    public interface IDataAggregator 
    {
        Output<Reference<DataAggregate>> Aggregate { get; set; }
    }
}