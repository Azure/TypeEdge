using Microsoft.Azure.IoT.TypeEdge.Modules;

namespace Microsoft.Azure.IoT.TypeEdge.Hubs
{
    public class Downstream<T> : Output<T>
        where T : IEdgeMessage

    {
        public Downstream(EdgeModule module) :
         base("$downstream", module)
        {
        }
        public override string RouteName => "$downstream";
    }
}

