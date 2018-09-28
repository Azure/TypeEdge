using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Edge.Agent.Core;
using Microsoft.Azure.Devices.Edge.Agent.Docker;
using Microsoft.Azure.Devices.Edge.Storage;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.TypeEdge.Host
{
    public class EmulatorConfigSource : IConfigSource
    {
        public EmulatorConfigSource(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void Dispose()
        {
        }

#pragma warning disable 1998
        public async Task<DeploymentConfigInfo> GetDeploymentConfigInfoAsync()
#pragma warning restore 1998
        {
            var manifest = Configuration.GetValue<string>(Constants.ManifestEnvironmentName);
            var edgeAgentDesired =
                JsonConvert.DeserializeObject<ConfigurationContent>(manifest).ModulesContent["$edgeAgent"][
                    "properties.desired"];
            dynamic element = JObject.FromObject(edgeAgentDesired);
            //var deploymentConfigInfo  = JsonConvert.SerializeObject(element);

            element.TryGetValue("schemaVersion", out JToken schemaVersion);
            element.TryGetValue("runtime", out JToken runtime);
            element.TryGetValue("systemModules", out JToken systemModules);
            element.TryGetValue("modules", out JToken modules);

            var modulesDictionary = new Dictionary<string, IModule>();

            foreach (var module in (JObject) modules)
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
            return
                deploymentConfigInfo; //new DeploymentConfigInfo(0, Microsoft.Azure.Devices.Edge.Agent.Core.DeploymentConfig.Empty);
        }
    }
}