using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Microsoft.Azure.TypeEdge.Modules.Enums
{
    //this code has been copied from the runtime

    public enum ModuleStatus
    {
        Unknown, 
        Backoff,
        Running,
        Unhealthy,
        Stopped,
        Failed
    }
}