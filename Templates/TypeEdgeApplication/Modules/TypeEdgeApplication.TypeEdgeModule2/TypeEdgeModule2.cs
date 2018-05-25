using System.Threading.Tasks;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using Microsoft.Azure.IoT.TypeEdge;
using TypeEdgeApplication.Shared;
using TypeEdgeApplication.Shared.Messages;
using TypeEdgeApplication.Shared.Twins;

namespace TypeEdgeApplication.Modules
{
    public class TypeEdgeModule2 : EdgeModule, ITypeEdgeModule2
    {
        readonly ITypeEdgeModule1 proxy;

        public Output<TypeEdgeModule2Output> Output { get; set; }
        public Input<TypeEdgeModule1Output> Input { get; set; }
        public ModuleTwin<TypeEdgeModule2Twin> Twin { get; set; }

        public TypeEdgeModule2(ITypeEdgeModule1 proxy)
        {
            this.proxy = proxy;
        }

        public override void BuildSubscriptions()
        {
            Input.Subscribe(proxy.Output, async (msg) =>
            {
                await Output.PublishAsync(new TypeEdgeModule2Output()
                {
                    Data = msg.Data,
                    Metadata = System.DateTime.UtcNow.ToShortTimeString()
                });
                return MessageResult.OK;
            });
        }
    }
}
