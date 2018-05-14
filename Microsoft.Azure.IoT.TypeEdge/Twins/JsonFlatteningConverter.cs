using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Azure.IoT.TypeEdge.Twins
{
    public class JsonFlatteningConverter : JsonConverter
    {
        readonly IContractResolver resolver;

        public JsonFlatteningConverter(IContractResolver resolver)
        {
            if (resolver == null)
                throw new ArgumentNullException();
            this.resolver = resolver;
        }

        public override bool CanConvert(Type objectType)
        {
            return resolver.ResolveContract(objectType) is JsonObjectContract;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            JObject jObject = JObject.Load(reader);
            var contract = (JsonObjectContract)resolver.ResolveContract(objectType); // Throw an InvalidCastException if this object does not map to a JObject.

            existingValue = existingValue ?? contract.DefaultCreator();

            if (jObject.Count == 0)
                return existingValue;

            var groups = jObject.Properties().GroupBy(p => p.Name.Contains('.') ? p.Name.Split('.').FirstOrDefault() : null).ToArray();
            foreach (var group in groups)
            {
                if (string.IsNullOrEmpty(group.Key))
                {
                    var subObj = new JObject(group);
                    using (var subReader = subObj.CreateReader())
                        serializer.Populate(subReader, existingValue);
                }
                else
                {
                    var jsonProperty = contract.Properties[group.Key];
                    if (jsonProperty == null || !jsonProperty.Writable)
                        continue;
                    if (jsonProperty != null)
                    {
                        var subObj = new JObject(group.Select(p => new JProperty(p.Name.Substring(group.Key.Length + 1), p.Value)));
                        using (var subReader = subObj.CreateReader())
                        {
                            var propertyValue = serializer.Deserialize(subReader, jsonProperty.PropertyType);
                            jsonProperty.ValueProvider.SetValue(existingValue, propertyValue);
                        }
                    }
                }
            }
            return existingValue;
        }

        public override bool CanWrite { get { return false; } }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //todo: implement the write
            throw new NotImplementedException();
        }
    }
}
