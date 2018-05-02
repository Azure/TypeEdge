using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.EdgeCompose
{
    public class Module<TInputMessage, TOutputMessage, TOptions> : IModule
        where TInputMessage : IModuleMessage
        where TOutputMessage : IModuleMessage
        where TOptions : class, new()
    {
        public ModuleEndpoint<TInputMessage> Input { get; set; }
        public ModuleEndpoint<TOutputMessage> Output { get; set; }

        public virtual string Name { get; private set; }
        protected virtual Func<TOptions, Task<ModuleInitializationResult>> InitHandler { get; private set; }
        protected virtual Func<Upstream<TOutputMessage>, Task<ExecutionResult>> ExecuteHandler { get; private set; }
        protected virtual Func<ModuleTwin, Upstream<TOutputMessage>, Task<ModuleTwinResult>> TwinUpdateHandler { get; private set; }
        protected virtual Func<TInputMessage, Upstream<TOutputMessage>, Task<InputMessageCallbackResult>> IncomingMessageCallback { get; private set; }
        protected virtual ModuleMethodCollection Methods { get; private set; }

        protected Module()
        {
        }

        public Module(string name,
            Func<TOptions, Task<ModuleInitializationResult>> moduleInitializationCallback,
            Func<Upstream<TOutputMessage>, Task<ExecutionResult>> moduleExecuteCallback,
            Func<ModuleTwin, Upstream<TOutputMessage>, Task<ModuleTwinResult>> moduleTwinUpdateCallback,
            Func<TInputMessage, Upstream<TOutputMessage>, Task<InputMessageCallbackResult>> incomingMessageCallback,
            ModuleMethodCollection methods)
        {
            Name = name;
            InitHandler = moduleInitializationCallback;
            ExecuteHandler = moduleExecuteCallback;
            TwinUpdateHandler = moduleTwinUpdateCallback;
            IncomingMessageCallback = incomingMessageCallback;
            Methods = methods;
        }

        public void Subscribe(ModuleEndpoint<TInputMessage> output)
        {
        }

        public void Subscribe<T>(ModuleEndpoint<T> output, Func<T, TInputMessage> endpointTypeConverter)
            where T : IModuleMessage
        {
        }
    }
}
