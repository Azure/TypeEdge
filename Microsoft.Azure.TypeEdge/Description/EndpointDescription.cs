using System;

namespace Microsoft.Azure.TypeEdge.Description
{
    public class EndpointDescription
    {
        public string Name { get; }

        public string TypeDescription { get; }

        public EndpointDescription(string name, Type type, Func<Type, string> schemaGenerator)
        {
            Name = name;
            TypeDescription = schemaGenerator(type);
        }
    }
}