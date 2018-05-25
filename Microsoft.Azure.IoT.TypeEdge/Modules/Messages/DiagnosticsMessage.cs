using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.IoT.TypeEdge.Modules.Messages
{
    public class DiagnosticsMessage : IEdgeMessage
    {
        private string[] _records;

        public DiagnosticsMessage(string[] data)
        {
            _records = data;
        }

        public IDictionary<string, string> Properties { get; set; }

        public byte[] GetBytes()
        {
            return Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, _records));
        }

        public void SetBytes(byte[] bytes)
        {
            _records = Encoding.UTF8.GetString(bytes).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}