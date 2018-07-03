namespace TypeEdge.Modules
{
    public abstract class Endpoint : TypeProperty
    {
        protected Endpoint(string name, EdgeModule module)
            :base(name, module)
        {
        }
        public abstract string RouteName { get; }
    }
}