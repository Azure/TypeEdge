using System;

namespace TypeEdge.Methods
{
    [AttributeUsage(AttributeTargets.All,
                  AllowMultiple = false,
                  Inherited = true)]
    public class EdgeMethodAttribute : Attribute
    {
    }
}