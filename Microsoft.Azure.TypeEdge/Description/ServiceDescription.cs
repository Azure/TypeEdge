using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.TypeEdge.Description
{
    public class ServiceDescription
    {
        public EndpointDescription[] InputDescriptions { get; set; }
        public EndpointDescription[] OutputDescriptions { get; set; }
        public TwinDescription TwinDescription { get; set; }
        public DirectMethodDescription[] DirectMethodDescriptions { get; set; }
    }
}
