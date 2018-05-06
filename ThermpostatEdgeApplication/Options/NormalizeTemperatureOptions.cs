using Microsoft.Azure.IoT.EdgeCompose.Modules;

namespace ThermpostatEdgeApplication
{
    public class NormalizeTemperatureOptions : IModuleOptions
    {
        public string DeviceConnectionString { get; set; }
    }
}