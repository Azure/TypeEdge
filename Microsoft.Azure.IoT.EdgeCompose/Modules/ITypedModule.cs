using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.EdgeCompose.Modules
{
    public interface ITypedModule<TInputMessage, TOutputMessage>
        where TInputMessage : IModuleMessage
        where TOutputMessage : IModuleMessage
    {
        Func<Upstream<TOutputMessage>, Task<ExecutionResult>> ExecuteHandler { get; }
        Func<ModuleTwin, Upstream<TOutputMessage>, Task<TwinResult>> TwinUpdateHandler { get; }
        Func<TInputMessage, Upstream<TOutputMessage>, Task<InputMessageCallbackResult>> IncomingMessageHandler { get; }

        void Subscribe(Upstream<TInputMessage> output);

        void Subscribe<T>(Upstream<T> output, Func<T, TInputMessage> endpointTypeConverter)
            where T : IModuleMessage;

    }
}