using System;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.TypeEdge;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using ThermostatApplication;
using ThermostatApplication.Messages;
using ThermostatApplication.Modules;
using ThermostatApplication.Twins;

namespace Modules
{
    public class NormalizeTemperatureModule : EdgeModule, INormalizeTemperatureModule
    {
        //memory state
        TemperatureScale scale;
        ITemperatureModule temperatureModuleProxy;

        public Input<TemperatureModuleOutput> Temperature { get; set; }
        public Output<TemperatureModuleOutput> NormalizedTemperature { get; set; }
        public ModuleTwin<NormalizerTwin> Twin { get; set; }

        public NormalizeTemperatureModule(ITemperatureModule proxy)
        {
            temperatureModuleProxy = proxy;
        }

        public override async Task<ExecutionResult> RunAsync()
        {
            var twin = await Twin.GetAsync();
            scale = twin.Scale;

            return ExecutionResult.OK;
        }

        public override void BuildSubscriptions()
        {            
            Temperature.Subscribe(temperatureModuleProxy.Temperature, async (temp) =>
            {
                if (temp.Scale != scale)
                    if (scale == TemperatureScale.Celsius)
                        temp.Temperature = temp.Temperature * 9 / 5 + 32;
                await NormalizedTemperature.PublishAsync(temp);

                return MessageResult.OK;
            });

            Twin.Subscribe(async (twin) =>
            {
                scale = twin.Scale;
                await Twin.ReportAsync(twin);
                return TwinResult.OK;
            });

        }
    }
}
