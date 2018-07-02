using System;

namespace Microsoft.Azure.IoT.TypeEdge.Methods
{
    [AttributeUsage(AttributeTargets.All,
                  AllowMultiple = false,
                  Inherited = true)]
    public class EdgeMethodAttribute : Attribute
    {
    }
}