using System.Threading.Tasks;
using Microsoft.Azure.IoT.TypeEdge;

namespace TypeEdgeModule2
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            await Startup.DockerEntryPoint(args);
        }
    }
}