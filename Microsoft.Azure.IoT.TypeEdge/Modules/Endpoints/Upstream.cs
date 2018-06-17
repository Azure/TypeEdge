using Microsoft.Azure.IoT.TypeEdge.Modules.Messages;

namespace Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints
{
    public class Upstream<T> : Output<T>
        where T : class, IEdgeMessage, new()

    {
        public Upstream(EdgeModule module) :
            base("$upstream", module)
        {
        }

        public override string RouteName => "$upstream";

        public void Subscribe<TO>(Output<TO> output)
            where TO : class, IEdgeMessage, new()
        {
            Module.SubscribeRoute(output.Name, output.RouteName, Name, RouteName);
        }
    }
}