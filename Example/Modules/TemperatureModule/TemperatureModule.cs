using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.TypeEdge.Enums;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.IoT.TypeEdge.Twins;
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

        public bool ResetSensor(int sensitivity)
        {
            Console.WriteLine($"ResetSensor called with sensitivity:{sensitivity}");
            return true;
        }

        public override async Task<ExecutionResult> RunAsync()
        {
            while (true)
            {
                await Temperature.PublishAsync(new TemperatureModuleOutput
                {
                    Scale = TemperatureScale.Celsius,
                    Temperature = new Random().NextDouble() * 100
                });
                Thread.Sleep(1000);
            }

            return await base.RunAsync();
        }
    }
}