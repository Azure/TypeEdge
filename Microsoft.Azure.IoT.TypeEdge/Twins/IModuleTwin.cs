using Microsoft.Azure.Devices.Shared;

namespace Microsoft.Azure.IoT.TypeEdge.Modules
{
    public interface IModuleTwin
    {
        void SetTwin(Twin twin);
        Twin GetTwin();
    }
}