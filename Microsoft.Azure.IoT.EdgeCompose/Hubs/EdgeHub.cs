using Autofac;
using Microsoft.Azure.Devices.Edge.Hub.Service;
using Microsoft.Azure.IoT.EdgeCompose.Modules;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Agent = Microsoft.Azure.Devices.Edge.Agent.Core;

namespace Microsoft.Azure.IoT.EdgeCompose.Hubs
{
    public class EdgeHub : Module<JsonMessage, JsonMessage, HubOptions>
    {
        public override string Name => Agent.Constants.EdgeHubModuleIdentityName;
        public EdgeHub()
        {
            CreateHandler = async (options) =>
            {
                Console.WriteLine($"[{DateTime.UtcNow.ToString("MM/dd/yyyy hh:mm:ss.fff tt")}] Edge Hub Created()");
                return CreationResult.OK;
            };
            ExecuteHandler = async (output) =>
            {
                var res = await Program.MainAsync(this.Options.HubServiceConfiguration);
                if (res == 0)
                    return ExecutionResult.OK;
                return ExecutionResult.Error;
            };

            IncomingMessageHandler = async (arg, output) => { return InputMessageCallbackResult.OK; };
            TwinUpdateHandler = async (update, output) => { return TwinResult.OK; };
        }

        public override void PopulateOptions(IConfigurationRoot configuration)
        {
            Options.HubServiceConfiguration = new ConfigurationBuilder()
               .AddJsonFile(Constants.ConfigFileName)
               .AddEnvironmentVariables()
               .Build();
        }
    }
}