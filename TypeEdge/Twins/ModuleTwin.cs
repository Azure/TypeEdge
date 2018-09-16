using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;
using System.Threading.Tasks;
using TypeEdge.Modules;
using TypeEdge.Modules.Enums;

namespace TypeEdge.Twins
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
            await Module.ReportTwinAsync(Name, twin);
        }

        public async Task<T> PublishAsync(T twin)
        {
            return await Module.PublishTwinAsync(Name, twin);
        }

        public async Task<T> GetAsync()
        {
            return await Module.GetTwinAsync<T>(Name);
        }

        public void SetDefault(T twin)
        {
            Module.SetTwinDefault(Name, twin);
        }
    }
}