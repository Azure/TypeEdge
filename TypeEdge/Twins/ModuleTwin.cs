using System;
using System.Threading.Tasks;
using TypeEdge.Modules;
using TypeEdge.Modules.Enums;

namespace TypeEdge.Twins
{
    public class ModuleTwin<T> : TypeProperty
        where T : TypeModuleTwin, new()
    {
        private object _twinLock = new object();
        T _lastTwin;

        public T LastKnownTwin { get { lock (_twinLock) return _lastTwin; } set { lock (_twinLock) _lastTwin = value; } }

        public ModuleTwin(string name, EdgeModule module)
           : base(name, module)
        {
        }

        public virtual void Subscribe(Func<T, Task<TwinResult>> handler)
        {
            Module.SubscribeTwin(Name, handler);
        }

        public async Task ReportAsync(T twin)
        {
            await Module.ReportTwinAsync(Name, twin);
            LastKnownTwin = twin;
        }

        public async Task<T> PublishAsync(T twin)
        {
            var t = await Module.PublishTwinAsync(Name, twin);
            LastKnownTwin = t;
            return t;
        }

        public async Task<T> GetAsync()
        {
            var t = await Module.GetTwinAsync<T>(Name);
            LastKnownTwin = t;
            return t;
        }
    }
}