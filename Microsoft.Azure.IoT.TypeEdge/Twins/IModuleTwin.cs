using Microsoft.Azure.Devices.Shared;

namespace Microsoft.Azure.IoT.TypeEdge.Modules
{
    public interface IModuleTwin
    {
        void SetProperies(TwinCollection desiredProperties);
    }
}