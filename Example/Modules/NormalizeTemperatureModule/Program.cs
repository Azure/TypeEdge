using Microsoft.Azure.IoT.TypeEdge;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Modules
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            await Startup.DockerEntryPoint(args);
        }
    }
}
