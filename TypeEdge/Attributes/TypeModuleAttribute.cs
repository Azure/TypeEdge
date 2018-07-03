using System;

namespace TypeEdge.Attributes
{
    [AttributeUsage(AttributeTargets.All,
                   AllowMultiple = false,
                   Inherited = true)]
    public class TypeModuleAttribute : Attribute
    {
    }
}