using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.TypeEdge;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using TypeEdgeApplication.Shared;
using TypeEdgeApplication.Shared.Messages;
using TypeEdgeApplication.Shared.Twins;

namespace TypeEdgeApplication.Modules
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
                await Output.PublishAsync(new TypeEdgeModule1Output {Data = new Random().NextDouble().ToString()});
                Thread.Sleep(1000);
            }

            return await base.RunAsync();
        }
    }
}