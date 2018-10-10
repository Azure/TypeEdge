namespace Microsoft.Azure.TypeEdge.Description
{
    public class ServiceDescription
    {
        public string Name { get; set; }
        public EndpointDescription[] InputDescriptions { get; set; }
        public EndpointDescription[] OutputDescriptions { get; set; }
        public TwinDescription[] TwinDescriptions { get; set; }
        public DirectMethodDescription[] DirectMethodDescriptions { get; set; }
    }
}