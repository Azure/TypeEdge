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
        public void SetTwin(string name, Twin twin)
        {
            LastKnownTwin = twin;

            var resolver = new DefaultContractResolver();
            var settings = new JsonSerializerSettings { ContractResolver = resolver, Converters = new JsonConverter[] { new JsonFlatteningConverter(resolver) } };

            JsonConvert.PopulateObject(twin.Properties.Desired.ToJson(), this, settings);

            //todo: verify the name
        }
        public Twin GetTwin(string name, bool desired)
        {
            //todo: use the json serializer here
            Twin result = LastKnownTwin;
            TwinCollection properties = null;
            if (result == null)
                if (desired)
                    result = new Twin() { Properties = new TwinProperties() { Desired = new TwinCollection() } };
                else
                    result = new Twin() { Properties = new TwinProperties() { Reported = new TwinCollection() } };

            if (desired)
                properties = result.Properties.Desired;
            else
                properties = result.Properties.Reported;

            foreach (var prop in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (prop.GetValue(this) != null)
                    properties[prop.Name] = Convert.ChangeType(prop.GetValue(this), typeof(string));
            }

            properties[$"___{name}"] = true;

            return result;
        }
    }
}