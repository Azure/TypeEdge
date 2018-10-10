using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.TypeEdge.Modules;
using Microsoft.Azure.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.TypeEdge.Twins;

namespace Microsoft.Azure.TypeEdge.Description
{
    public static class ServiceDescriptor
    {
        public static ServiceDescription Describe(Type type)
        {
            return Describe(type, new SchemaGenerator().Generate);
        }

        public static ServiceDescription Describe<T>()
            where T : TypeModule
        {
            return Describe(typeof(T));
        }

        public static ServiceDescription Describe<T>(Func<Type, string> schemaGenerator)
            where T : TypeModule
        {
            return Describe(typeof(T), schemaGenerator);
        }

        public static ServiceDescription Describe(Type type, Func<Type, string> schemaGenerator)
        {
            return new ServiceDescription
            {
                Name = type.Name,
                InputDescriptions = GetEndpointDescription(type, typeof(Input<>), schemaGenerator),
                OutputDescriptions = GetEndpointDescription(type, typeof(Output<>), schemaGenerator),
                TwinDescriptions = GetTwinDescription(type, typeof(ModuleTwin<>), schemaGenerator),
                DirectMethodDescriptions = GetDirectMethodDescriptions(type, schemaGenerator)
            };
        }

        private static DirectMethodDescription[] GetDirectMethodDescriptions(Type type,
            Func<Type, string> schemaGenerator)
        {
            return type.GetProxyInterface().GetMethods().Where(m => !m.IsSpecialName)
                .Select(e => new DirectMethodDescription(e, schemaGenerator)).ToArray();
        }

        private static EndpointDescription[] GetEndpointDescription(Type type, Type propertyType,
            Func<Type, string> schemaGenerator)
        {
            return GetPropertyInfos(type, propertyType)
                .Select(e => new EndpointDescription(e.Name, e.PropertyType.GenericTypeArguments[0], schemaGenerator))
                .ToArray();
        }

        private static TwinDescription[] GetTwinDescription(Type type, Type propertyType,
            Func<Type, string> schemaGenerator)
        {
            return GetPropertyInfos(type, propertyType)
                .Select(e => new TwinDescription(e.Name, e.PropertyType.GenericTypeArguments[0], schemaGenerator))
                .ToArray();
        }

        private static IEnumerable<PropertyInfo> GetPropertyInfos(Type type, Type propertyType)
        {
            return type.GetProperties().Where(e =>
                e.PropertyType.IsGenericType &&
                e.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(propertyType));
        }
    }
}