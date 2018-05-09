using System;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.EdgeCompose.Modules;

namespace Microsoft.Azure.IoT.EdgeCompose.Hubs
{
    public class Upstream<T> : Input<T>
        where T : IEdgeMessage

    {
        public Upstream(EdgeModule module) :
         base("$upstream", module)
        {
        }
        public override string RouteName => "$upstream";
    }
}