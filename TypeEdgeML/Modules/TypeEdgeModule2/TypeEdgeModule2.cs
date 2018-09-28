using System;
using Microsoft.Azure.TypeEdge.Modules;
using Microsoft.Azure.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.TypeEdge.Modules.Messages;
using Microsoft.Azure.TypeEdge.Twins;
using Microsoft.Extensions.Logging;
using TypeEdgeML.Shared;
using TypeEdgeML.Shared.Messages;
using TypeEdgeML.Shared.Twins;

namespace Modules
{
    public class TypeEdgeModule2 : TypeModule, ITypeEdgeModule2
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
                Logger.LogInformation("Generated Message");

                return MessageResult.Ok;
            });
        }

        public Output<TypeEdgeModule2Output> Output { get; set; }
        public ModuleTwin<TypeEdgeModule2Twin> Twin { get; set; }
    }
}