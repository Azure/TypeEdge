using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Microsoft.Azure.TypeEdge.Modules.Enums
{
    //this code has been copied from the runtime

    public enum RestartPolicy
    {
        Never = 0,
        OnFailure = 1,
        OnUnhealthy = 2,
        Always = 3,
        Unknown
    }
}