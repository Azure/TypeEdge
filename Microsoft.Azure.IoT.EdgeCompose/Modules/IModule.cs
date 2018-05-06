using Autofac;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.EdgeCompose.Modules
{
    public interface IModule
    {
        Task StartAsync();
        Task<CreationResult> CreateAsync();
        void PopulateOptions(IConfigurationRoot configuration);
        void RegisterOptions(ContainerBuilder builder, IConfigurationRoot configuration);

    }
}