using Microsoft.Azure.IoT.EdgeCompose.Modules;

namespace Microsoft.Azure.IoT.EdgeCompose.Hubs
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

