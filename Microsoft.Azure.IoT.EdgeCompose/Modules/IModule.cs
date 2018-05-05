using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.EdgeCompose.Modules
{
    public interface IModule
    {
        Task StartAsync();
        Task<CreationResult> CreateAsync();
    }
}