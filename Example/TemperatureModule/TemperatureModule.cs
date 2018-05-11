using Microsoft.Azure.IoT.TypeEdge;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using System;
using System.Threading;
using System.Threading.Tasks;
using ThermostatApplication;
using ThermostatApplication.Messages;
using ThermostatApplication.Modules;

namespace Modules
{
    public class TemperatureModule : EdgeModule, ITemperatureModule
    {
        public Output<TemperatureModuleOutput> Temperature { get; set; }

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
