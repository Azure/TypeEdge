using TypeEdge.Volumes;
using ThermostatApplication.Messages;

namespace ThermostatApplication.Modules
{
    public interface ISharedVolume
    {
        Volume<Sample> Samples { get; set; }
    }
}