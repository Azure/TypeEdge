using Microsoft.Extensions.Options;
using StructureMap;
using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.EdgeCompose.Modules
{
    public class Module<TInputMessage, TOutputMessage, TOptions> : 
        ITypedModule<TInputMessage, TOutputMessage>, 
        IModule, 
        IDeferred
        
        where TInputMessage : IModuleMessage
        where TOutputMessage : IModuleMessage
        where TOptions : class, new()
    {
        public Upstream<TOutputMessage> Output { get; private set; }
        protected Container Container { get; private set; }

        public string Name { get; private set; }

        public virtual Func<TOptions, Task<CreationResult>> CreateHandler { get; private set; }
        public virtual Func<Upstream<TOutputMessage>, Task<ExecutionResult>> ExecuteHandler { get; private set; }
        public virtual Func<ModuleTwin, Upstream<TOutputMessage>, Task<TwinResult>> TwinUpdateHandler { get; private set; }
        public virtual Func<TInputMessage, Upstream<TOutputMessage>, Task<InputMessageCallbackResult>> IncomingMessageHandler { get; private set; }

        public virtual ModuleMethodCollection Methods { get; private set; }
        public TOptions Options { get; private set; }

        //for inheritance composition
        protected Module(Container container)
        {
            Name = GetType().Name;
            Container = container;
            Options = Container.GetInstance<TOptions>();
            Output = new Upstream<TOutputMessage>();
            Methods = new ModuleMethodCollection();
        }

        //for dynamic composition
        public Module(string name,
        Container container,
        Func<TOptions, Task<CreationResult>> createCallback,
        Func<Upstream<TOutputMessage>, Task<ExecutionResult>> executeCallback,
        Func<ModuleTwin, Upstream<TOutputMessage>, Task<TwinResult>> twinUpdateCallback,
        Func<TInputMessage, Upstream<TOutputMessage>, Task<InputMessageCallbackResult>> incomingMessageCallback,
        ModuleMethodCollection methods) : this(container)
        {
            Name = name;
            Methods = methods;

            CreateHandler = createCallback;
            ExecuteHandler = executeCallback;
            TwinUpdateHandler = twinUpdateCallback;
            IncomingMessageHandler = incomingMessageCallback;
        }

        public void Subscribe(Upstream<TInputMessage> output)
        {
        }

        public void Subscribe<T>(Upstream<T> output, Func<T, TInputMessage> endpointTypeConverter)
            where T : IModuleMessage
        {
        }

        public void DependsOn(IModule module)
        {
        }

        public async Task<CreationResult> CreateAsync()
        {
            return await CreateHandler(Options);
        }
        public Task StartAsync()
        {
            return Task.Factory.StartNew(() => ExecuteHandler(Output), TaskCreationOptions.LongRunning);
        }
    }
}
