using Microsoft.Azure.IoT.TypeEdge.Modules;

namespace TypeEdgeApplication.Shared.Twins
{
    public class TypeEdgeModule1Twin : TypeModuleTwin
    {
        public int Threshold { get; set; }
    }
}