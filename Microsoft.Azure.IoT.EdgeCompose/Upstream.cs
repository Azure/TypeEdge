using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.EdgeCompose
{
    public class Upstream<TOutputMessage> where TOutputMessage : IModuleMessage
    {
        public Task<PublishResult> PublishAsync(IModuleMessage message)
        {
            return Task.FromResult(PublishResult.OK);
        }
    }
}