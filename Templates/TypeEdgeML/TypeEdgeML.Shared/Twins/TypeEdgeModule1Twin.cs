using Microsoft.Azure.IoT.TypeEdge.Twins;

namespace TypeEdgeML.Shared.Twins
{
    public class TypeEdgeModule1Twin : TypeModuleTwin
    {
        public int Threshold { get; set; }
    }
}