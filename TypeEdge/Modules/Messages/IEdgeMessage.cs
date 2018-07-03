using System.Collections.Generic;

namespace TypeEdge.Modules.Messages
{
    public interface IEdgeMessage
    {
        IDictionary<string, string> Properties { get; set; }
        byte[] GetBytes();
        void SetBytes(byte[] bytes);
    }
}