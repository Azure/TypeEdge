using Microsoft.Azure.TypeEdge.Modules;
using Microsoft.Azure.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.TypeEdge.Twins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
            return new ServiceDescription(type.Name,
                GetEndpointDescription(type, typeof(Input<>), schemaGenerator),
                GetEndpointDescription(type, typeof(Output<>), schemaGenerator),
                GetTwinDescription(type, typeof(ModuleTwin<>), schemaGenerator),
                GetDirectMethodDescriptions(type, schemaGenerator));
        }

        private static List<DirectMethodDescription> GetDirectMethodDescriptions(Type type,
            Func<Type, string> schemaGenerator)
        {
            return type.GetProxyInterface().GetMethods().Where(m => !m.IsSpecialName)
                .Select(e => new DirectMethodDescription(e, schemaGenerator)).ToList();
        }

        private static List<EndpointDescription> GetEndpointDescription(Type type, Type propertyType,
            Func<Type, string> schemaGenerator)
        {
            return GetPropertyInfos(type, propertyType)
                .Select(e => new EndpointDescription(e.Name, e.PropertyType.GenericTypeArguments[0], schemaGenerator))
                .ToList();
        }

        private static List<TwinDescription> GetTwinDescription(Type type, Type propertyType,
            Func<Type, string> schemaGenerator)
        {
            return GetPropertyInfos(type, propertyType)
                .Select(e => new TwinDescription(e.Name, e.PropertyType.GenericTypeArguments[0], schemaGenerator))
                .ToList();
        }

        private static IEnumerable<PropertyInfo> GetPropertyInfos(Type type, Type propertyType)
        {
            return type.GetProperties().Where(e =>
                e.PropertyType.IsGenericType &&
                e.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(propertyType));
        }
    }
}