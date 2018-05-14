using Microsoft.Azure.Devices.Shared;

namespace Microsoft.Azure.IoT.TypeEdge.Modules
{
    public interface IModuleTwin
    {
        Twin LastKnownTwin { get; set; }
        Twin GetDesiredTwin(string name); 
        Twin GetReportedTwin(string name);
        void SetTwin(string name, Twin twin);
    }
}