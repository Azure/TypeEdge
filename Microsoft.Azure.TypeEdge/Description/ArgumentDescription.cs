using System;

namespace Microsoft.Azure.TypeEdge.Description
{
    public class ArgumentDescription
    {
        public ArgumentDescription(string argumentName, Type type, Func<Type, string> schemaGenerator)
        {
            Name = argumentName;
            TypeDescription = new TypeDescription(type, schemaGenerator);
        }

        public string Name { get; set; }
        public TypeDescription TypeDescription { get; set; }
    }
}