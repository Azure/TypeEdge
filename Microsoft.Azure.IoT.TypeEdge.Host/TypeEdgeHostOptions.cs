using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.IoT.TypeEdge.Host
{
    public class TypeEdgeHostOptions
    {
        public string IotHubConnectionString { get; set; }
        public string DeviceId { get; set; }
        public RunningEnvironment Environment { get; set; }
        public Serilog.Events.LogEventLevel RuntimeLogLevel { get; set; }
    }
}
