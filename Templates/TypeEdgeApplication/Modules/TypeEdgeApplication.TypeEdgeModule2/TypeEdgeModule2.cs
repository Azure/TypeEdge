using System;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using TypeEdgeApplication.Shared;
using TypeEdgeApplication.Shared.Messages;
using TypeEdgeApplication.Shared.Twins;

namespace TypeEdgeApplication.Modules
{
    public class TypeEdgeModule2 : EdgeModule, ITypeEdgeModule2
    {
        private readonly ITypeEdgeModule1 _proxy;

        public TypeEdgeModule2(ITypeEdgeModule1 proxy)
        {
            _proxy = proxy;
        }

        public Output<TypeEdgeModule2Output> Output { get; set; }
        public Input<TypeEdgeModule1Output> Input { get; set; }
        public ModuleTwin<TypeEdgeModule2Twin> Twin { get; set; }

        public override void BuildSubscriptions()
        {
            Input.Subscribe(_proxy.Output, async msg =>
            {
                await Output.PublishAsync(new TypeEdgeModule2Output
                {
                    Data = msg.Data,
                    Metadata = DateTime.UtcNow.ToShortTimeString()
                });
                return MessageResult.OK;
            });
        }
    }
}