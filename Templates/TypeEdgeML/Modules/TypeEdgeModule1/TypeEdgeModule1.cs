using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.TypeEdge.Enums;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.IoT.TypeEdge.Twins;
using TypeEdgeML.Shared;
using TypeEdgeML.Shared.Messages;
using TypeEdgeML.Shared.Twins;

namespace Modules
{
    public class TypeEdgeModule1 : EdgeModule, ITypeEdgeModule1
    {
        public Output<TypeEdgeModule1Output> Output { get; set; }
        public ModuleTwin<TypeEdgeModule1Twin> Twin { get; set; }

        public bool ResetModule(int sensorThreshold)
        {
            Console.WriteLine($"New sensor threshold:{sensorThreshold}");
            return true;
        }

        public override async Task<ExecutionResult> RunAsync()
        {
            while (true)
            {
                await Output.PublishAsync(new TypeEdgeModule1Output {Data = new Random().NextDouble().ToString(CultureInfo.InvariantCulture)});
                Thread.Sleep(1000);
            }
        }
    }
}