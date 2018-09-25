using System;
using System.Reflection;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.TypeEdge.Twins
{
    public abstract class TypeTwin
    {
        Twin _twin;
        string _name;

        internal TwinCollection GetReportedProperties(string twinName = null)
        {
            if (_twin == null)
                _twin = new Twin(new TwinProperties() { Desired = new TwinCollection(), Reported = new TwinCollection() });
            UpdateProperties(_twin.Properties.Reported, twinName != null ? twinName : _name);
            return _twin.Properties.Reported;
        }

        internal Twin GetTwin()
        {
            //only the proxy is calling this
            if (_twin == null)
                _twin = new Twin(new TwinProperties() { Desired = new TwinCollection(), Reported = new TwinCollection() });
            UpdateProperties(_twin.Properties.Desired, _name);
            return _twin;
        }

        public static T CreateTwin<T>(string name, Twin twin)
            where T : TypeTwin
        {
            var instance = Activator.CreateInstance<T>();
            return SetupInstance(name, twin, instance);
        }
        public static TypeTwin CreateTwin(Type type, string name, TwinCollection desiredProperties)
        {
            var instance = (TypeTwin)Activator.CreateInstance(type);
            return SetupInstance(name,
                new Twin(new TwinProperties()
                {
                    Desired = desiredProperties,
                    Reported = new TwinCollection()
                }),
                instance);
        }
        private static T SetupInstance<T>(string name, Twin twin, T instance) where T : TypeTwin
        {
            instance._twin = twin;
            instance._name = name;
            instance.PopulateProperties();
            return instance;
        }
        private void PopulateProperties()
        {
            var props = _twin.Properties.Desired;

            if (!IsValid(props))
                return;

            var resolver = new DefaultContractResolver();
            var settings = new JsonSerializerSettings
            {
                ContractResolver = resolver,
                Converters = new JsonConverter[] { new JsonFlatteningConverter(resolver) }
            };

            JsonConvert.PopulateObject(props.ToJson(), this, settings);
        }
        private void UpdateProperties(TwinCollection properties, string name)
        {
            //arrays are not supported!!
            foreach (var prop in GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                if (prop.GetValue(this) != null)
                    properties[prop.Name] = Convert.ChangeType(prop.GetValue(this), typeof(string));

            properties[$"___{name}"] = true;
        }
        private bool IsValid(TwinCollection props)
        {
            var nameToken = $"___{_name}";

            if (!props.Contains(nameToken) ||
                !(props[nameToken] is JValue nameValue) ||
                nameValue.Type != JTokenType.Boolean ||
                !((bool)nameValue.Value))
                return false;
            return true;
        }

    }
}