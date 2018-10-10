using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.Azure.TypeEdge.Description
{
    public class DirectMethodDescription
    {
        public DirectMethodDescription(MethodInfo mi, Func<Type, string> schemaGenerator)
        {
            Name = mi.Name;
            if (mi.ReturnType != typeof(void))
                ReturnTypeDescription = new TypeDescription(mi.ReturnType, schemaGenerator);
            ArgumentsTypeDescription = mi.GetParameters()
                .Select(p => new ArgumentDescription(p.Name, p.ParameterType, schemaGenerator)).ToArray();
        }

        public string Name { get; set; }
        public ArgumentDescription[] ArgumentsTypeDescription { get; set; }
        public TypeDescription ReturnTypeDescription { get; set; }
    }
}