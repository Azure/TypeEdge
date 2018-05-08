using Microsoft.Azure.IoT.EdgeCompose;
using Microsoft.Azure.IoT.EdgeCompose.Modules;
using System.Threading.Tasks;

namespace ThermpostatEdgeApplication
{
    public class TemperatureModuleOutput : IEdgeMessage
    {
        public TemperatureScale Scale { get; set; }
        public float Temperature { get; set; }

    }
}