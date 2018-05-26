using Microsoft.Azure.IoT.TypeEdge.Twins;

namespace Shared.Twins
{
    public class TypeEdgeModuleTwin : TypeModuleTwin
    {
        public int Threshold { get; set; }
    }
}