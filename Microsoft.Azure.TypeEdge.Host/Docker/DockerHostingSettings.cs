using System;
using System.Collections.Generic;
using System.Reflection;
using Docker.DotNet.Models;
using Microsoft.Azure.Devices.Edge.Agent.Core;
using Microsoft.Azure.Devices.Edge.Agent.Docker;
using Microsoft.Azure.TypeEdge.Modules;
using Newtonsoft.Json;
using RestartPolicy = Microsoft.Azure.Devices.Edge.Agent.Core.RestartPolicy;

namespace Microsoft.Azure.TypeEdge.Host.Docker
{
    public class DockerHostingSettings
    {
        public DockerHostingSettings()
        {

        }
        public DockerHostingSettings(HostingSettings hostingSettings)
        {
            IsExternalModule = hostingSettings.IsExternalModule;

            Version = hostingSettings.Version;
            Type = hostingSettings.Type;
            DesiredStatus = Enum.Parse<ModuleStatus>(hostingSettings.DesiredStatus.ToString());
            RestartPolicy = Enum.Parse<RestartPolicy>(hostingSettings.RestartPolicy.ToString());
            Config = new DockerConfig(hostingSettings.ImageName, ProcessCreateOptions(hostingSettings.Options));
        }

        [JsonIgnore] public bool IsExternalModule { get; }
        [JsonIgnore] public bool IsSystemModule { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; private set; }

        [JsonProperty(Required = Required.Always, PropertyName = "type")]
        public string Type { get; private set; }

        [JsonProperty(PropertyName = "status")]
        public ModuleStatus? DesiredStatus { get; private set; }

        [JsonProperty(PropertyName = "restartPolicy")]
        public RestartPolicy? RestartPolicy { get; private set; }

        [JsonProperty(Required = Required.Always, PropertyName = "settings")]
        public DockerConfig Config { get; set; }

        private static CreateContainerParameters ProcessCreateOptions(Dictionary<string, object> options)
        {
            var res = new CreateContainerParameters();

            if (options != null)
            {
                var properties = res.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var property in properties)
                    if (options.ContainsKey(property.Name))
                        property.SetValue(res,
                            JsonConvert.DeserializeObject(JsonConvert.SerializeObject(options[property.Name]),
                                property.PropertyType));
            }

            return res;
        }
    }
}