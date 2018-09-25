using Microsoft.Azure.TypeEdge.Modules.Messages;

namespace Microsoft.Azure.TypeEdge.Modules.Endpoints
{
    public class Upstream<T> : Output<T>
        where T : class, IEdgeMessage, new()

    {
        public Upstream(TypeModule module) :
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