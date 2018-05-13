using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.TypeEdge.Modules
{
    public class ModuleTwin<T>
        where T : IModuleTwin
    {
        public string Name{ get; set; }
        public EdgeModule Module { get; set; }

        public ModuleTwin(string name, EdgeModule module)
        {
            Module = module;
            Name = name;
        }

        public virtual void Subscribe(Func<T, Task<TwinResult>> handler)
        {
            Module.SubscribeTwin(Name, handler);
        }

        public void Report(T twin)
        {
            throw new NotImplementedException();
        }
    }
}