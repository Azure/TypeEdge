using Microsoft.Azure.IoT.EdgeCompose.Modules;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.IoT.EdgeCompose.Hubs
{
    public class HubOptions : IModuleOptions
    {
        public IConfigurationRoot HubServiceConfiguration { get; set; }
        public string DeviceConnectionString { get; set; }
    }
}