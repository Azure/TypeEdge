namespace Microsoft.Azure.IoT.TypeEdge.Modules
{
    public abstract class Endpoint
    {
        protected Endpoint(string name, EdgeModule module)
        {
            Name = name;
            Module = module;
        }

        public string Name { get; set; }
        public abstract string RouteName { get; }
        internal EdgeModule Module { get; set; }
    }
}