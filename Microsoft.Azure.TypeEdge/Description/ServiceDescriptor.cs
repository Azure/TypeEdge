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
        public static ServiceDescription Describe<T>(Func<Type, string> schemaGenerator)
        where T : TypeModule
        {
            return new ServiceDescription
            {
                InputDescriptions = GetEndpointDescription<T>(typeof(Input<>), schemaGenerator),
                OutputDescriptions = GetEndpointDescription<T>(typeof(Output<>), schemaGenerator),
                TwinDescription = GetTwinDescription<T>(typeof(ModuleTwin<>), schemaGenerator),
                DirectMethodDescriptions = GetDirectMethodDescriptions<T>(schemaGenerator)
            };
        }

        private static DirectMethodDescription[] GetDirectMethodDescriptions<T>(Func<Type, string> schemaGenerator)
        {
            return typeof(T).GetProxyInterface().GetMethods().Where(m => !m.IsSpecialName).Select(e => new DirectMethodDescription(e, schemaGenerator)).ToArray();
        }

        private static EndpointDescription[] GetEndpointDescription<T>(Type propertyType, Func<Type, string> schemaGenerator)
        {
            return GetPropertyInfos<T>(propertyType)
                .Select(e => new EndpointDescription(e.Name, e.PropertyType.GenericTypeArguments[0], schemaGenerator)).ToArray();
        }

        private static TwinDescription GetTwinDescription<T>(Type propertyType, Func<Type, string> schemaGenerator)
        {
            return new TwinDescription(GetPropertyInfos<T>(propertyType).Select(e => e.PropertyType.GenericTypeArguments[0]), schemaGenerator);
        }

        private static IEnumerable<PropertyInfo> GetPropertyInfos<T>(Type propertyType)
        {
            return typeof(T).GetProperties().Where(e =>
                e.PropertyType.IsGenericType &&
                e.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(propertyType));
        }
    }
}
