using System;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using Microsoft.Azure.IoT.TypeEdge.Modules.Enums;

namespace Microsoft.Azure.IoT.TypeEdge.Twins
{
    public class ModuleTwin<T>
        where T : TypeModuleTwin, new()
    {
        public ModuleTwin(string name, EdgeModule module)
        {
            Module = module;
            Name = name;
        }

        private string Name { get; }
        private EdgeModule Module { get; }

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