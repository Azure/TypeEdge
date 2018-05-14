using Microsoft.Azure.Devices.Shared;
using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.TypeEdge.Modules
{
    public class ModuleTwin<T>
        where T : TypeModuleTwin, new()
    {
        private string Name { get; set; }
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
            await Module.ReportTwinAsync(Name, twin);
        }

        public Task<T> PublishAsync(T twin)
        {
            return Module.PublishTwinAsync(Name, twin);
        }

        public async Task<T> GetAsync()
        {
            return await Module.GetTwinAsync<T>(Name);
        }
    }
}