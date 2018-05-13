using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.TypeEdge.Modules
{
    public class ModuleTwin<T>
        where T : IModuleTwin
    {
        private string Name{ get; set; }
        private EdgeModule Module { get; set; }

        public ModuleTwin(string name, EdgeModule module)
        {
            Module = module;
            Name = name;
        }

        public virtual void Subscribe(Func<T, Task<TwinResult>> handler)
        {
            Module.SubscribeTwin(Name, handler);
        }

        public async Task ReportAsync(T twin)
        {
            await Module.ReportTwinAsync(twin);
        }

        public Task Publish(T twin)
        {
            throw new NotImplementedException();
        }
    }
}