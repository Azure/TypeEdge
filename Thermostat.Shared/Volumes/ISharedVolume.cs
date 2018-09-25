using Microsoft.Azure.TypeEdge.Volumes;
using ThermostatApplication.Messages;

namespace ThermostatApplication.Modules
{
    public interface ISharedVolume
    {
        Volume<DataAggregate> Aggregates { get; set; }
    }
}