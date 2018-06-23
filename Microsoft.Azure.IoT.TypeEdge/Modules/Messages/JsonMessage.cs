using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.IoT.TypeEdge.Modules.Messages
{
    public class JsonMessage : EdgeMessage
    {
        public string JsonData { get; set; }
    }
}