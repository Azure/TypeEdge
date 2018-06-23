using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.IoT.TypeEdge.Modules.Messages
{
    public class DiagnosticsMessage : EdgeMessage
    {
        private string[] _records;

        public DiagnosticsMessage(string[] data)
        {
            _records = data;
        }
    }
}