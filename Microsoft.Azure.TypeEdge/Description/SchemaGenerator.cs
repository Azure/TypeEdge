using Newtonsoft.Json.Schema.Generation;
using System;

namespace Microsoft.Azure.TypeEdge.Description
{
    public class SchemaGenerator
    {
        private readonly JSchemaGenerator JSchemaGenerator;

        public SchemaGenerator()
        {
            JSchemaGenerator = new JSchemaGenerator();
        }

        public string Generate(Type type)
        {
            return JSchemaGenerator.Generate(type).ToString();
        }
    }
}