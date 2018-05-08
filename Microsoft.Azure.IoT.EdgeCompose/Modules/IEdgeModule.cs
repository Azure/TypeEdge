using System.Threading.Tasks;
using Microsoft.Azure.IoT.EdgeCompose.Hubs;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.IoT.EdgeCompose.Modules
{
    public interface IEdgeModule
    {
        Input<JsonMessage> DefaultInput { get; }
        Output<JsonMessage> DefaultOutput { get; }
        string Name { get; }

        CreationResult Create(IConfigurationRoot configuration);
        Task<InitializationResult> InitAsync();
        Task<PropertiesResult> PropertiesHandler(ModuleProperties newProps);
        Task<ExecutionResult> RunAsync();
        Task<TwinResult> TwinHandler(ModuleTwin newTwin);
        Task<PublishResult> PublishMessageAsync<T>(string outputName, T message) where T : IEdgeMessage;
    }
}