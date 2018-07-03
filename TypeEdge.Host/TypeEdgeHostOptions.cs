using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog.Events;

namespace TypeEdge.Host
{
    public class TypeEdgeHostOptions
    {
        public string IotHubConnectionString { get; set; }
        public string DeviceId { get; set; }
        public LogEventLevel? RuntimeLogLevel { get; set; }
        public bool? PrintDeploymentJson { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public RunningEnvironment? Environment { get; set; }
    }
}