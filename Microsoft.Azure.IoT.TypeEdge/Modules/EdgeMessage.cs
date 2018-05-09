using System.Collections.Generic;

namespace Microsoft.Azure.IoT.TypeEdge.Modules
{
    public interface IEdgeMessage
    {
        IDictionary<string, string> Properties { get; set; }
        byte[] GetBytes();
        void SetBytes(byte[] bytes);
    }
}