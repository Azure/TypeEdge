using Microsoft.Azure.IoT.EdgeCompose.Modules;

namespace ThermpostatEdgeApplication
{
    public class ReadTemperatureOptions : IModuleOptions
    {
        public string DeviceConnectionString { get; set; }
    }
}