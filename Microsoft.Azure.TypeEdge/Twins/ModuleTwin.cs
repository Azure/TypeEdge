using Microsoft.Azure.TypeEdge.Modules;
using Microsoft.Azure.TypeEdge.Modules.Enums;
using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.TypeEdge.Twins
{
    public class ModuleTwin<T> : TypeProperty
        where T : TypeTwin, new()
    {
        public ModuleTwin(string name, TypeModule module)
            : base(name, module)
        {
        }

        public void Subscribe(Func<T, Task<TwinResult>> handler)
        {
            Module.SubscribeTwin(Name, handler);
        }

        public async Task ReportAsync(T twin)
        {
            await Module.ReportTwinAsync(Name, twin).ConfigureAwait(false);
        }

        public async Task<T> PublishAsync(T twin)
        {
            return await Module.PublishTwinAsync(Name, twin).ConfigureAwait(false);
        }

        public async Task<T> GetAsync()
        {
            return await Module.GetTwinAsync<T>(Name).ConfigureAwait(false);
        }

        public void SetDefault(T twin)
        {
            Module.SetTwinDefault(Name, twin);
        }
    }
}