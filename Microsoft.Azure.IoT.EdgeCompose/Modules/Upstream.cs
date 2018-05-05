using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.EdgeCompose.Modules
{
    public class Upstream<TOutputMessage> : ModuleEndpoint<TOutputMessage>
        where TOutputMessage : IModuleMessage
    {
        public Task<PublishResult> PublishAsync(IModuleMessage message)
        {
            return Task.FromResult(PublishResult.OK);
        }
    }
}