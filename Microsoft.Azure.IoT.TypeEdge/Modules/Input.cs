using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.TypeEdge.Modules
{
    public class Input<T> : Endpoint
        where T : IEdgeMessage
    {
        public Input(string name, EdgeModule module) :
          base(name, module)
        {
        }

        public override string RouteName => $"BrokeredEndpoint(\"/modules/{this.Module.Name}/inputs/{Name}\")";
        public virtual void Subscribe(Endpoint output, Func<T, Task<MessageResult>> handler)
        {
            Module.Subscribe(output.Name, output.RouteName, Name, RouteName, handler);
        }
        
    }
}