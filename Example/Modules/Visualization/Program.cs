using Microsoft.Azure.IoT.TypeEdge;
using System;
using System.Threading.Tasks;

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
