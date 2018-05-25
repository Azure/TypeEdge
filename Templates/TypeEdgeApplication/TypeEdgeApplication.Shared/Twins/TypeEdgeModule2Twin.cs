using Microsoft.Azure.IoT.TypeEdge.Modules;

namespace TypeEdgeApplication.Shared.Twins
{
    public class TypeEdgeModule2Twin : TypeModuleTwin
    {
        public int Threshold { get; set; }
    }
}