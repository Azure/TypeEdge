using Microsoft.Azure.IoT.TypeEdge.Modules;

namespace ThermpostatEdgeApplication
{
    public class ReadTemperatureOptions : IModuleOptions
    {
        public string DeviceConnectionString { get; set; }
    }
}