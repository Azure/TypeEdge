using Microsoft.Azure.IoT.EdgeCompose;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThermpostatEdgeApplication
{
    public class NormalizeTemperatureModule : Module<TemperatureModuleOutput, TemperatureModuleOutput, NormalizeTemperatureOptions>
    {
        public override string Name => nameof(NormalizeTemperatureModule);

        protected override Func<NormalizeTemperatureOptions, Task<ModuleInitializationResult>> InitHandler =>
            async (config) =>
            {
                //initialize user code of module
                return ModuleInitializationResult.OK;
            };
        protected override Func<Upstream<TemperatureModuleOutput>, Task<ExecutionResult>> ExecuteHandler =>
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

        protected override Func<ModuleTwin, Upstream<TemperatureModuleOutput>, Task<ModuleTwinResult>> TwinUpdateHandler =>
            async (update, output) =>
            {
                //twin handler
                return ModuleTwinResult.OK;
            };

        protected override Func<TemperatureModuleOutput, Upstream<TemperatureModuleOutput>, Task<InputMessageCallbackResult>> IncomingMessageCallback =>
            async (msg, output) =>
            {
                //input handler
                return InputMessageCallbackResult.OK;
            };

        protected override ModuleMethodCollection Methods =>
            new ModuleMethodCollection { { new Method<JsonMethodArgument, JsonMethodResponse>("Ping", (arg) => { return new JsonMethodResponse(arg, @"{""output1"": ""pong"", ""output2"": ""from ping"" }"); }) } };


    }
}
