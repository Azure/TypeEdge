using Microsoft.Azure.IoT.TypeEdge;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using System;
using System.Threading;
using System.Threading.Tasks;
using ThermostatApplication;
using ThermostatApplication.Messages;
using ThermostatApplication.Modules;
using ThermostatApplication.Twins;

namespace Modules
{
    public class TemperatureModule : EdgeModule, ITemperatureModule
    {
        public Output<TemperatureModuleOutput> Temperature { get; set; }
        public ModuleTwin<TemperatureTwin> Twin { get; set; }

        public override async Task<ExecutionResult> RunAsync()
        {
            while (true)
            {
                await Temperature.PublishAsync(new TemperatureModuleOutput() { Scale = TemperatureScale.Celsius, Temperature = new Random().NextDouble() });
                Thread.Sleep(1000);
            }
            return await base.RunAsync();
        }
    }
}
