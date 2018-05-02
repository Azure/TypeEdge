using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.EdgeCompose
{
    public class Hub : Module<JsonMessage, JsonMessage, HubOptions>
    {
        public override string Name { get => nameof(Hub); }

        protected override Func<Upstream<JsonMessage>, Task<ExecutionResult>> ExecuteHandler =>
            async (output) => { return ExecutionResult.OK; };

        protected override Func<HubOptions, Task<ModuleInitializationResult>> InitHandler =>
            async (options) => { return ModuleInitializationResult.OK; };

        protected override Func<JsonMessage, Upstream<JsonMessage>, Task<InputMessageCallbackResult>> IncomingMessageCallback =>
            async (arg, output) => { return InputMessageCallbackResult.OK; };

        protected override Func<ModuleTwin, Upstream<JsonMessage>, Task<ModuleTwinResult>> TwinUpdateHandler =>
            async (update, output) => { return ModuleTwinResult.OK; };

        public Hub()
        {
        }
    }
}