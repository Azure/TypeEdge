using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Azure.IoT.TypeEdge.Modules
{
    public class DiagnosticsMessage : IEdgeMessage
    {
        private string[] records;

        public DiagnosticsMessage(string[] data)
        {
            records = data;
        }
        public IDictionary<string, string> Properties { get; set; }

        public byte[] GetBytes()
        {
            return Encoding.UTF8.GetBytes(String.Join(Environment.NewLine, records));
        }

        public void SetBytes(byte[] bytes )
        {
            records = Encoding.UTF8.GetString(bytes).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}