using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Edge.Agent.Core;
using Microsoft.Azure.Devices.Edge.Agent.Docker;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Edge.Storage;

namespace TypeEdge.Host
{
    public class EmulatorConfigSource : IConfigSource
    {
        readonly IConfiguration _configuration;
        public EmulatorConfigSource(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IConfiguration Configuration => _configuration;

        public void Dispose()
        {
        }

        public async Task<DeploymentConfigInfo> GetDeploymentConfigInfoAsync()
        {
            var manifest = _configuration.GetValue<string>(Constants.ManifestEnvironmentName);
            var edgeAgentDesired = JsonConvert.DeserializeObject<ConfigurationContent>(manifest).ModulesContent["$edgeAgent"]["properties.desired"];
            dynamic element = JObject.FromObject(edgeAgentDesired);
            //var deploymentConfigInfo  = JsonConvert.SerializeObject(element);

            element.TryGetValue("schemaVersion", out JToken schemaVersion);
            element.TryGetValue("runtime", out JToken runtime);
            element.TryGetValue("systemModules", out JToken systemModules);
            element.TryGetValue("modules", out JToken modules);

            var modulesDictionary = new Dictionary<string, IModule>();

            foreach (var module in (modules as JObject) )
                modulesDictionary[module.Key] = JsonConvert.DeserializeObject<DockerModule>(module.Value.ToJson());

            var deploymentConfig = new DeploymentConfig(
                    schemaVersion.ToString(),
                    JsonConvert.DeserializeObject<DockerRuntimeInfo>(runtime.ToJson()),
                    new SystemModules(
                        JsonConvert.DeserializeObject<EdgeAgentDockerModule>(systemModules["edgeAgent"].ToJson()),
                        JsonConvert.DeserializeObject<EdgeHubDockerModule>(systemModules["edgeHub"].ToJson())),
                    modulesDictionary);

            //EdgeAgentDockerModule EdgeHubDockerModule
            var deploymentConfigInfo = new DeploymentConfigInfo(
                10,
                deploymentConfig
            );

            //var deploymentConfig = new DeploymentConfig(schemaVersion.ToString(), runtime, new SystemModules(null, null), modules);
            //return new DeploymentConfigInfo(0, JsonConvert.DeserializeObject<DeploymentConfig>(deploymentConfigInfo));
            return deploymentConfigInfo;//new DeploymentConfigInfo(0, Microsoft.Azure.Devices.Edge.Agent.Core.DeploymentConfig.Empty);
        }
    }
}
