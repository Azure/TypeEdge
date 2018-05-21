using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.IoT.TypeEdge.Host
{
    public class TypeEdgeHostOptions
    {
        public string IotHubConnectionString { get; set; }
        public string DeviceId { get; set; }
        public LogEventLevel RuntimeLogLevel { get; set; }
    }
}
        