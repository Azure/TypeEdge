using System;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.TypeEdge.Modules;

namespace Microsoft.Azure.IoT.TypeEdge.Hubs
{
    public class Upstream<T> : Output<T>
        where T : IEdgeMessage

    {
        public Upstream(EdgeModule module) :
         base("$upstream", module)
        {
        }
        public override string RouteName => "$upstream";

        public void Subscribe<O>(Output<O> output)
            where O : IEdgeMessage
        {
            Module.Subscribe(output.Name, output.RouteName, Name, RouteName);
        }
    }
}