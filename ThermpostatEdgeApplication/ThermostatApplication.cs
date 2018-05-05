using Microsoft.Azure.IoT.EdgeCompose;
using Microsoft.Azure.IoT.EdgeCompose.Hubs;
using Microsoft.Azure.IoT.EdgeCompose.Modules;
using Microsoft.Azure.IoT.EdgeCompose.Modules.Methods;
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
        public override CompositionResult Compose()
        {
            //setup modules
            var readTemperature = new Module<TemperatureModuleInput, TemperatureModuleOutput, ReadTemperatureOptions>(
               "ReadTemperatureModule",
               Container,
               async (config) =>
               {
                   //initialize the user code of the module
                   return CreationResult.OK;
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
                   return TwinResult.OK;
               },
               async (msg, output) =>
               {
                   //input handler
                   return InputMessageCallbackResult.OK;
               },
               new ModuleMethodCollection {
                    {new Method<JsonMethodArgument,  JsonMethodResponse>("Ping", (arg) => { return new JsonMethodResponse(arg, @"{""output1"": ""pong"", ""output2"": ""from ping"" }"); } ) }
               });

            var normalizeTemperatureModule = new NormalizeTemperatureModule(Container);

            Modules.Add(normalizeTemperatureModule);
            Modules.Add(readTemperature);

            //setup routing
            readTemperature.Subscribe(Hub.Output, (msg) => { return new TemperatureModuleInput(msg); });
            normalizeTemperatureModule.Subscribe(readTemperature.Output);
            Hub.Subscribe(normalizeTemperatureModule.Output, (msg) => { return new JsonMessage(msg.ToString()); });

            //setup startup depedencies
            normalizeTemperatureModule.DependsOn(readTemperature);

            return CompositionResult.OK;
        }
    }
}
