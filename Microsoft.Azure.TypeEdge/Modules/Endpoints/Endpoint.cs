namespace Microsoft.Azure.TypeEdge.Modules
{
    public abstract class Endpoint : TypeProperty
    {
        protected Endpoint(string name, TypeModule module)
            :base(name, module)
        {
        }
        public abstract string RouteName { get; }
    }
}