using Autofac;
using Microsoft.Azure.IoT.EdgeCompose;
using Microsoft.Azure.IoT.EdgeCompose.Modules;
using Microsoft.Azure.IoT.EdgeCompose.Modules.Methods;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThermpostatEdgeApplication
{
    public class NormalizeTemperatureModule : Module<TemperatureModuleOutput, TemperatureModuleOutput, NormalizeTemperatureOptions>
    {
        public override Func<NormalizeTemperatureOptions, Task<CreationResult>> CreateHandler =>
            async (config) =>
            {
                //initialize user code of module
                return CreationResult.OK;
            };
        public override Func<Upstream<TemperatureModuleOutput>, Task<ExecutionResult>> ExecuteHandler =>
            async (output) =>
            {
                //the module long running loop code
                while (true)
                {
                    Thread.Sleep(1000);
                    await output.PublishAsync(new TemperatureModuleOutput() { });
                }
                return ExecutionResult.OK;
            };

        public override Func<ModuleTwin, Upstream<TemperatureModuleOutput>, Task<TwinResult>> TwinUpdateHandler =>
            async (update, output) =>
            {
                //twin handler
                return TwinResult.OK;
            };

        public override Func<TemperatureModuleOutput, Upstream<TemperatureModuleOutput>, Task<InputMessageCallbackResult>> IncomingMessageHandler =>
            async (msg, output) =>
            {
                //input handler
                return InputMessageCallbackResult.OK;
            };

        public override ModuleMethodCollection Methods =>
            new ModuleMethodCollection { { new Method<JsonMethodArgument, JsonMethodResponse>("Ping", (arg) => { return new JsonMethodResponse(arg, @"{""output1"": ""pong"", ""output2"": ""from ping"" }"); }) } };


    }
}
