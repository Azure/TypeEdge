namespace Microsoft.Azure.IoT.EdgeCompose.Modules
{
    public class Endpoint
    {
        public string Name { get; set; }
        public EdgeModule Module { get; set; }

        public Endpoint(string name, EdgeModule module)
        {
            Name = name;
            Module = module;
        }
    }
}