using System;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.IoT.TypeEdge.Modules.Messages;
using Microsoft.Azure.IoT.TypeEdge.Twins;
using TypeEdgeML.Shared;
using TypeEdgeML.Shared.Messages;
using TypeEdgeML.Shared.Twins;

namespace Modules
{
    public class TypeEdgeModule2 : EdgeModule, ITypeEdgeModule2
    {
        public TypeEdgeModule2(ITypeEdgeModule1 proxy)
        {
            proxy.Output.Subscribe(this, async msg =>
            {
                await Output.PublishAsync(new TypeEdgeModule2Output
                {
                    Data = msg.Data,
                    Metadata = DateTime.UtcNow.ToShortTimeString()
                });
                Console.WriteLine("TypeEdgeModule2: Generated Message");

                return MessageResult.Ok;
            });
        }

        public Output<TypeEdgeModule2Output> Output { get; set; }
        public ModuleTwin<TypeEdgeModule2Twin> Twin { get; set; }
    }
}