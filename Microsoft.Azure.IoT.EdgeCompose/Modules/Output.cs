using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.EdgeCompose.Modules
{
    public class Output<T> : Endpoint
       where T : IEdgeMessage
    {
        public Output(string name, EdgeModule module) :
            base(name, module)
        {
        }
        public async Task<PublishResult> PublishAsync(T message)
        {
            return await Module.PublishAsync(Name, message);
        }
    }
}