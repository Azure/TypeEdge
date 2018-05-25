using Microsoft.Azure.IoT.TypeEdge;
using System.Threading.Tasks;

namespace TypeEdgeModule2
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            await Startup.DockerEntryPoint(args);
        }
    }
}
