using System;

namespace Microsoft.Azure.TypeEdge.Methods
{
    [AttributeUsage(AttributeTargets.All,
                  AllowMultiple = false,
                  Inherited = true)]
    public class EdgeMethodAttribute : Attribute
    {
    }
}