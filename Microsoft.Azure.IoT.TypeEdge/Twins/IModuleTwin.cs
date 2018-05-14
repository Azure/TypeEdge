using Microsoft.Azure.Devices.Shared;

namespace Microsoft.Azure.IoT.TypeEdge.Modules
{
    public interface IModuleTwin
    {
        Twin LastKnownTwin { get; set; }
        Twin GetTwin(bool desired);
        void SetTwin(Twin twin);
    }
}