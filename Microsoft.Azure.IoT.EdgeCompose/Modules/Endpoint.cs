namespace Microsoft.Azure.IoT.EdgeCompose.Modules
{
    public class Endpoint
    {
        public string Name { get; set; }
        public IEdgeModule Module { get; set; }

        public Endpoint(string name, IEdgeModule module)
        {
            Name = name;
            Module = module;
        }
    }
}