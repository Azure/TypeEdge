using System;

namespace Microsoft.Azure.TypeEdge.Description
{
    public class TypeDescription
    {
        public TypeDescription(Type type, Func<Type, string> schemaGenerator)
        {
            Name = type.Name;
            Description = schemaGenerator(type);
        }

        public string Name { get; set; }
        public string Description { get; set; }
    }
}