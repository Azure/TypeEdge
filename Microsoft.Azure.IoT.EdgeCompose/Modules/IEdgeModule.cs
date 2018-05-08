using System;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.EdgeCompose.Hubs;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.IoT.EdgeCompose.Modules
{
    //public interface IEdgeModule
    //{
    //    Input<JsonMessage> DefaultInput { get; }
    //    Output<JsonMessage> DefaultOutput { get; }
    //    string Name { get; }

    //    CreationResult Configure(IConfigurationRoot configuration);
    //    Task<ExecutionResult> RunAsync();

    //    void Subscribe<T>(string name, Func<T, Task<MessageResult>> handler) 
    //        where T : IEdgeMessage;
    //    Task<PublishResult> PublishAsync<T>(string outputName, T message)
    //        where T : IEdgeMessage;

    //    //handlers
    //    Task<PropertiesResult> PropertiesHandler(ModuleProperties newProps);
    //    Task<TwinResult> TwinHandler(ModuleTwin newTwin);
    //}
}