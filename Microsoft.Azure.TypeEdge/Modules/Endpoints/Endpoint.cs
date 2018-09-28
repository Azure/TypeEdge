namespace Microsoft.Azure.TypeEdge.Modules.Endpoints
{
    public abstract class Endpoint : TypeProperty
    {
        protected Endpoint(string name, TypeModule module)
            : base(name, module)
        {
        }

        public abstract string RouteName { get; }
    }
}