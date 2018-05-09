using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.TypeEdge.Modules
{
    public class Output<T> : Endpoint
       where T : IEdgeMessage
    {
        public Output(string name, EdgeModule module) :
            base(name, module)
        {
        }
        public override string RouteName => $"/messages/modules/{this.Module.Name}/outputs/{Name}";

        public string ModuleName { get { return Module.Name; } }

        public async Task<PublishResult> PublishAsync(T message)
        {
            return await Module.PublishAsync(Name, message);
        }
    }
}