using Microsoft.Azure.TypeEdge;
using Microsoft.Azure.TypeEdge.Attributes;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TypeEdgeModule3
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            await Startup.DockerEntryPoint(args);
        }
    }
}
