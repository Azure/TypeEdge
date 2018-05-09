using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.EdgeCompose.Modules
{
    public abstract class Endpoint
    {
        public string Name { get; set; }
        public abstract string RouteName { get; }
        public EdgeModule Module { get; set; }

        public Endpoint(string name, EdgeModule module)
        {
            Name = name;
            Module = module;
        }
        
    }
}