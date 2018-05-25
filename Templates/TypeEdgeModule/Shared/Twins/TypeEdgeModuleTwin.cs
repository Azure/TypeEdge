using Microsoft.Azure.IoT.TypeEdge.Modules;

namespace Shared.Twins
{
    public class TypeEdgeModuleTwin : TypeModuleTwin
    {
        public int Threshold { get; set; }
    }
}