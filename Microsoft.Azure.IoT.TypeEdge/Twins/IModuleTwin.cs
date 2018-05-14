using Microsoft.Azure.Devices.Shared;

namespace Microsoft.Azure.IoT.TypeEdge.Modules
{
    public interface IModuleTwin
    {
        Twin LastKnownTwin { get; set; }
        Twin GetTwin(string name, bool desired);
        void SetTwin(string name, Twin twin);
    }
}