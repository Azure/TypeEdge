using System.Threading.Tasks;
using Microsoft.Azure.IoT.TypeEdge;

namespace Modules
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            await Startup.DockerEntryPoint(args);
        }
    }
}