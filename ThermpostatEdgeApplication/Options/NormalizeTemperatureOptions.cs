using Microsoft.Azure.IoT.TypeEdge.Modules;

namespace ThermpostatEdgeApplication
{
    public class NormalizeTemperatureOptions : IModuleOptions
    {
        public string DeviceConnectionString { get; set; }
    }
}