using System;

namespace Microsoft.Azure.IoT.TypeEdge.Attributes
{
    [AttributeUsage(AttributeTargets.All,
                   AllowMultiple = false,
                   Inherited = true)]
    public class TypeModuleAttribute : Attribute
    {
    }
}