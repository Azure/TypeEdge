using System.Collections.Generic;

namespace Microsoft.Azure.TypeEdge.Modules.Messages
{
    public interface IEdgeMessage
    {
        IDictionary<string, string> Properties { get; set; }
        byte[] GetBytes();
        void SetBytes(byte[] bytes);
    }
}