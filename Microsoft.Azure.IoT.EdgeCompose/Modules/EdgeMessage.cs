using System.Collections.Generic;

namespace Microsoft.Azure.IoT.EdgeCompose.Modules
{
    public interface IEdgeMessage
    {
        IDictionary<string, string> Properties { get; set; }
        byte[] GetBytes();
    }
}