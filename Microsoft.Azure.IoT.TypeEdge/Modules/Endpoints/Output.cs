using System.Threading.Tasks;
using Microsoft.Azure.IoT.TypeEdge.Enums;
using Microsoft.Azure.IoT.TypeEdge.Modules.Messages;

namespace Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints
{
    public class Output<T> : Endpoint
        where T : IEdgeMessage
    {
        public Output(string name, EdgeModule module) :
            base(name, module)
        {
        }

        public override string RouteName => $"/messages/modules/{Module.Name}/outputs/{Name}";


        public async Task<PublishResult> PublishAsync(T message)
        {
            return await Module.PublishMessageAsync(Name, message);
        }
    }
}