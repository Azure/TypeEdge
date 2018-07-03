using System;
using TypeEdge.Modules;
using TypeEdge.Modules.Endpoints;
using TypeEdge.Modules.Messages;
using TypeEdge.Twins;
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