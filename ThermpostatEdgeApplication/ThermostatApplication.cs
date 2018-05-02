using Microsoft.Azure.IoT.EdgeCompose;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThermpostatEdgeApplication
{
    public class ThermostatApplication : IoTEdgeApplication
    {
        public override ApplicationInitializationResult InitializeApplication(Container container)
        {
            //setup modules
            var readTemperature = new Module<TemperatureModuleInput, TemperatureModuleOutput, ReadTemperatureOptions>(
               "ReadTemperatureModule",
               async (config) =>
               {
                   //initialize the user code of the module
                   return ModuleInitializationResult.OK;
               },
               async (output) =>
               {
                   //the module long running loop code
                   while (true)
                   {
                       Thread.Sleep(1000);
                       await output.PublishAsync(new TemperatureModuleOutput() { });
                   }
                   return ExecutionResult.OK;
               },
               async (update, output) =>
               {
                   //twin handler
                   return ModuleTwinResult.OK;
               },
               async (msg, output) =>
               {
                   //input handler
                   return InputMessageCallbackResult.OK;
               },
               new ModuleMethodCollection {
                    {new Method<JsonMethodArgument,  JsonMethodResponse>("Ping", (arg) => { return new JsonMethodResponse(arg, @"{""output1"": ""pong"", ""output2"": ""from ping"" }"); } ) }
               });

            var normalizeTemperatureModule = new NormalizeTemperatureModule();

            Modules.Add(normalizeTemperatureModule);
            Modules.Add(readTemperature);

            //setup routing
            readTemperature.Subscribe(Hub.Output, (msg) => { return new TemperatureModuleInput(msg); });
            normalizeTemperatureModule.Subscribe(readTemperature.Output);
            Hub.Subscribe(normalizeTemperatureModule.Output, (msg) => { return new JsonMessage(msg.ToString()); });

            return ApplicationInitializationResult.OK;
        }
    }
}
