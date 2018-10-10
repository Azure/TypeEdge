using System;

namespace Microsoft.Azure.TypeEdge.Description
{
    public class EndpointDescription
    {
        public EndpointDescription(string name, Type type, Func<Type, string> schemaGenerator)
        {
            Name = name;
            TypeDescription = new TypeDescription(type, schemaGenerator);
        }

        public string Name { get; }

        public TypeDescription TypeDescription { get; }
    }
}