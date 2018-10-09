using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.Azure.TypeEdge.Description
{
    public class DirectMethodDescription
    {
        public string Name { get; set; }
        public string[] ArgumentsTypeDescription { get; set; }
        public string ReturnTypeDescription { get; set; }

        public DirectMethodDescription(MethodInfo mi, Func<Type, string> schemaGenerator)
        {
            
            Name = mi.Name;
            if (mi.ReturnType != typeof(void))
                ReturnTypeDescription = schemaGenerator(mi.ReturnType);
            ArgumentsTypeDescription = mi.GetParameters().Select(p => schemaGenerator(p.ParameterType)).ToArray();
        }
    }
}