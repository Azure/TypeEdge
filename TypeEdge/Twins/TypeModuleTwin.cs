using System;
using System.Reflection;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace TypeEdge.Twins
{
    public abstract class TypeModuleTwin : IModuleTwin
    {
        private object _twinLock = new object();
        Twin _lastTwin;

        public Twin LastKnownTwin { get { lock (_twinLock) return _lastTwin; } set { lock (_twinLock) _lastTwin = value; } }

        public void SetTwin(string name, Twin twin)
        {
            LastKnownTwin = twin;

            var resolver = new DefaultContractResolver();
            var settings = new JsonSerializerSettings
            {
                ContractResolver = resolver,
                Converters = new JsonConverter[] { new JsonFlatteningConverter(resolver) }
            };

            JsonConvert.PopulateObject(twin.Properties.Desired.ToJson(), this, settings);

            //todo: verify the name
        }

        public Twin GetDesiredTwin(string name)
        {
            return GetTwin(name, true);
        }

        public Twin GetReportedTwin(string name)
        {
            return GetTwin(name, false);
        }

        private Twin GetTwin(string name, bool desired)
        {
            //todo: use the json serializer here
            var result = LastKnownTwin;
            TwinCollection properties;
            if (result == null) result = desired ?
                    new Twin
                    {
                        Properties = new TwinProperties { Desired = new TwinCollection() }
                    } :
                    new Twin
                    {
                        Properties = new TwinProperties { Reported = new TwinCollection() }
                    };

            if (desired)
                properties = result.Properties.Desired;
            else
                properties = result.Properties.Reported;

            //arrays are not supported!!
            foreach (var prop in GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                if (prop.GetValue(this) != null)
                    properties[prop.Name] = Convert.ChangeType(prop.GetValue(this), typeof(string));

            properties[$"___{name}"] = true;

            return result;
        }
    }
}