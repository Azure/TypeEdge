using Microsoft.Azure.IoT.TypeEdge.Twins;

namespace TypeEdgeApplication.Shared.Twins
{
    public class TypeEdgeModule1Twin : TypeModuleTwin
    {
        public int Threshold { get; set; }
    }
}