﻿//using Newtonsoft.Json;
//using System;
//using Docker.DotNet.Models;
//using Newtonsoft.Json.Linq;
//using System.Collections.Generic;
//using System.Reflection;

//namespace Microsoft.Azure.TypeEdge.Host.Docker
//{
//    //this code has been copied from the runtime

//    [JsonConverter(typeof(DockerConfigJsonConverter))]
//    public class DockerConfig : IEquatable<DockerConfig>
//    {
//        readonly CreateContainerParameters createOptions;

//        [JsonProperty(Required = Required.Always, PropertyName = "image")]
//        public string Image { get; set; }

//        // Do a serialization roundtrip to clone the createOptions
//        // https://docs.docker.com/engine/api/v1.25/#operation/ContainerCreate
//        [JsonProperty(Required = Required.AllowNull, PropertyName = "createOptions")]
//        public CreateContainerParameters CreateOptions => JsonConvert.DeserializeObject<CreateContainerParameters>(JsonConvert.SerializeObject(this.createOptions));

//        [JsonConstructor]
//        public DockerConfig(string image, string createOptions)
//        {
//            this.Image = image?.Trim() ?? string.Empty;
//            this.createOptions = string.IsNullOrWhiteSpace(createOptions)
//                ? new CreateContainerParameters()
//                : JsonConvert.DeserializeObject<CreateContainerParameters>(createOptions);
//        }

//        public DockerConfig(string image, CreateContainerParameters createOptions)
//        {
//            this.Image = image?.Trim() ?? string.Empty;
//            this.createOptions = createOptions;
//        }

//        public override bool Equals(object obj) => this.Equals(obj as DockerConfig);

//        public override int GetHashCode()
//        {
//            unchecked
//            {
//                int hashCode = (this.Image != null ? this.Image.GetHashCode() : 0);
//                hashCode = (hashCode * 397) ^ (this.createOptions?.GetHashCode() ?? 0);
//                return hashCode;
//            }
//        }

//        public bool Equals(DockerConfig other)
//        {
//            if (ReferenceEquals(null, other))
//            {
//                return false;
//            }
//            if (ReferenceEquals(this, other))
//            {
//                return true;
//            }

//            return string.Equals(this.Image, other.Image) &&
//                CompareCreateOptions(this.CreateOptions, other.CreateOptions);
//        }

//        class DockerConfigJsonConverter : JsonConverter
//        {
//            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//            {
//                writer.WriteStartObject();

//                var dockerconfig = (DockerConfig)value;

//                writer.WritePropertyName("image");
//                serializer.Serialize(writer, dockerconfig.Image);

//                writer.WritePropertyName("createOptions");
//                serializer.Serialize(writer, JsonConvert.SerializeObject(dockerconfig.CreateOptions));

//                writer.WriteEndObject();
//            }

//            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//            {
//                JObject obj = JObject.Load(reader);

//                // Pull out JToken values from json
//                obj.TryGetValue("image", StringComparison.OrdinalIgnoreCase, out JToken jTokenImage);
//                obj.TryGetValue("createOptions", StringComparison.OrdinalIgnoreCase, out JToken jTokenCreateOptions);

//                return new DockerConfig(jTokenImage?.ToString(), (jTokenCreateOptions?.ToString() ?? string.Empty));
//            }

//            public override bool CanConvert(Type objectType) => objectType == typeof(DockerConfig);
//        }

//        static bool CompareCreateOptions(CreateContainerParameters a, CreateContainerParameters b)
//        {
//            bool result;

//            if ((a != null) && (b != null))
//            {
//                string aValue = null;
//                string bValue = null;

//                // Remove the `normalizedCreateOptions` labels from comparison consideration
//                if (a.Labels?.TryGetValue("normalizedCreateOptions", out aValue) ?? false)
//                {
//                    a.Labels?.Remove("normalizedCreateOptions");
//                }
//                if (b.Labels?.TryGetValue("normalizedCreateOptions", out bValue) ?? false)
//                {
//                    b.Labels?.Remove("normalizedCreateOptions");
//                }

//                result = JsonConvert.SerializeObject(a).Equals(JsonConvert.SerializeObject(b));

//                // Restore `normalizedCreateOptions` labels
//                if (aValue != null)
//                {
//                    a.Labels.Add("normalizedCreateOptions", aValue);
//                }
//                if (bValue != null)
//                {
//                    b.Labels.Add("normalizedCreateOptions", bValue);
//                }
//            }
//            else
//            {
//                result = (a == b);
//            }

//            return result;
//        }
//    }
//}