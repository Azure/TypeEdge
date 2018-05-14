using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.IoT.TypeEdge.Twins;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;

namespace Microsoft.Azure.IoT.TypeEdge.Modules
{
    public abstract class TypeModuleTwin : IModuleTwin
    {
        public Twin LastKnownTwin { get; set; }
        public void SetTwin(Twin twin)
        {
            LastKnownTwin = twin;

            var resolver = new DefaultContractResolver();
            var settings = new JsonSerializerSettings { ContractResolver = resolver, Converters = new JsonConverter[] { new JsonFlatteningConverter(resolver) } };

            JsonConvert.PopulateObject(twin.Properties.Desired.ToJson(), this, settings);

            //foreach (var prop in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            //{
            //    if (twin.Properties.Desired.Contains(prop.Name))
            //    {
            //        var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            //        var value = (targetType == null) ? null : Convert.ChangeType(twin.Properties.Desired[prop.Name], targetType);

            //        prop.SetValue(this, value, null);

            //    }
            //}
        }
        public Twin GetTwin(bool desired)
        {
            //todo: use the json serializer here
            Twin result = LastKnownTwin;

            if (result == null)
                if (desired)
                    result = new Twin() { Properties = new TwinProperties() { Desired = new TwinCollection() } };
                else
                    result = new Twin() { Properties = new TwinProperties() { Reported = new TwinCollection() } };

            foreach (var prop in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (prop.GetValue(this) != null)
                    if (desired)
                        result.Properties.Desired[prop.Name] = Convert.ChangeType(prop.GetValue(this), typeof(string));
                    else
                        result.Properties.Reported[prop.Name] = Convert.ChangeType(prop.GetValue(this), typeof(string));
            }
            return result;
        }
    }
}