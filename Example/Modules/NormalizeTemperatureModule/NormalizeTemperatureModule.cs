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
            if (twin.Scale.HasValue)
                scale = twin.Scale.Value;
            else
                scale = TemperatureScale.Fahrenheit;

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

                //if (temp.Temperature > 30)
                //    await temperatureModuleProxy.Twin.PublishAsync(new TemperatureTwin() { MaxLimit = 30 });

                return MessageResult.OK;
            });

            Upstream.Subscribe(NormalizedTemperature);

            Twin.Subscribe(async (twin) =>
            {
                if (twin.Scale.HasValue)
                {
                    scale = twin.Scale.Value;
                    await Twin.ReportAsync(twin);
                }
                return TwinResult.OK;
            });

        }
    }
}
