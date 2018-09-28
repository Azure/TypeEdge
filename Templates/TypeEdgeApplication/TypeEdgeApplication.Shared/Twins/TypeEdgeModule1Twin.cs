using Microsoft.Azure.TypeEdge.Twins;

namespace TypeEdgeApplication.Shared.Twins
{
    public class TypeEdgeModule1Twin : TypeTwin
    {
        public int Threshold { get; set; }
    }
}