using System;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.TypeEdge.Modules.Messages;

namespace Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints
{
    public class Input<T> : Endpoint
        where T : IEdgeMessage
    {
        public Input(string name, EdgeModule module) :
            base(name, module)
        {
        }

        public override string RouteName => $"BrokeredEndpoint(\"/modules/{Module.Name}/inputs/{Name}\")";

        public virtual void Subscribe(Endpoint output, Func<T, Task<MessageResult>> handler)
        {
            Module.SubscribeRoute(output.Name, output.RouteName, Name, RouteName, handler);
        }
    }
}