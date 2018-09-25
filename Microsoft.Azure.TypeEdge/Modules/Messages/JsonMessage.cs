using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.TypeEdge.Modules.Messages
{
    public class JsonMessage : EdgeMessage
    {
        public string JsonData { get; set; }
    }
}