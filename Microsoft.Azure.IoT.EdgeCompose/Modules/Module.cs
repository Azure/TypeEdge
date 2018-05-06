using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Agent = Microsoft.Azure.Devices.Edge.Agent.Core;

namespace Microsoft.Azure.IoT.EdgeCompose.Modules
{
    public class Module<TInputMessage, TOutputMessage, TOptions> :
        ITypedModule<TInputMessage, TOutputMessage>,
        IModule,
        IDeferred

        where TInputMessage : IModuleMessage
        where TOutputMessage : IModuleMessage
        where TOptions : IModuleOptions, new()
    {
        public Upstream<TOutputMessage> Output { get; private set; }
        public virtual string Name { get; private set; }
        public virtual Func<TOptions, Task<CreationResult>> CreateHandler { get; protected set; }
        public virtual Func<Upstream<TOutputMessage>, Task<ExecutionResult>> ExecuteHandler { get; protected set; }
        public virtual Func<ModuleTwin, Upstream<TOutputMessage>, Task<TwinResult>> TwinUpdateHandler { get; protected set; }
        public virtual Func<TInputMessage, Upstream<TOutputMessage>, Task<InputMessageCallbackResult>> IncomingMessageHandler { get; protected set; }

        public virtual ModuleMethodCollection Methods { get; private set; }
        public TOptions Options { get; protected set; }

        //for inheritance composition
        protected Module()
        {
            Name = GetType().Name;
            Output = new Upstream<TOutputMessage>();
            Methods = new ModuleMethodCollection();
        }

        //for dynamic composition
        public Module(string name,
        Func<TOptions, Task<CreationResult>> createCallback,
        Func<Upstream<TOutputMessage>, Task<ExecutionResult>> executeCallback,
        Func<ModuleTwin, Upstream<TOutputMessage>, Task<TwinResult>> twinUpdateCallback,
        Func<TInputMessage, Upstream<TOutputMessage>, Task<InputMessageCallbackResult>> incomingMessageCallback,
        ModuleMethodCollection methods)
        {
            Name = name;
            Methods = methods;

            CreateHandler = createCallback;
            ExecuteHandler = executeCallback;
            TwinUpdateHandler = twinUpdateCallback;
            IncomingMessageHandler = incomingMessageCallback;
        }

        public void RegisterOptions(ContainerBuilder builder, IConfigurationRoot configuration)
        {
            var edgeDeviceConnectionString = configuration.GetValue<string>(Constants.DeviceConnectionStringName);

            Options = new TOptions();

            Options.DeviceConnectionString = edgeDeviceConnectionString;

            PopulateOptions(configuration);

            builder.Register(c => Options);
        }

        public virtual void PopulateOptions(IConfigurationRoot configuration)
        {
            //stadardize the custom options loading here
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
        internal string ModuleConnectionString { get { return $"{Options.DeviceConnectionString};{Agent.Constants.ModuleIdKey}={Name}"; } }
    }
}
