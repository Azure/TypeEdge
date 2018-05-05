using Microsoft.Azure.IoT.EdgeCompose.Modules;
using StructureMap;
using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.EdgeCompose.Hubs
{
    public class MemoryHub : Module<JsonMessage, JsonMessage, HubOptions>
    {
        public override Func<Upstream<JsonMessage>, Task<ExecutionResult>> ExecuteHandler =>
            async (output) => { return ExecutionResult.OK; };

        public override Func<HubOptions, Task<CreationResult>> CreateHandler =>
            async (options) => { return CreationResult.OK; };

        public override Func<JsonMessage, Upstream<JsonMessage>, Task<InputMessageCallbackResult>> IncomingMessageHandler =>
            async (arg, output) => { return InputMessageCallbackResult.OK; };

        public override Func<ModuleTwin, Upstream<JsonMessage>, Task<TwinResult>> TwinUpdateHandler =>
            async (update, output) => { return TwinResult.OK; };

        public MemoryHub(Container container)
            :base(container)
        {
        }
    }
}