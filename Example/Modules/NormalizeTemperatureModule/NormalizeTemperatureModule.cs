using Microsoft.Azure.IoT.TypeEdge.Modules;
using ThermostatApplication;
using ThermostatApplication.Messages;
using ThermostatApplication.Modules;
using ThermostatApplication.Twins;

namespace Modules
{
    public class NormalizeTemperatureModule : EdgeModule, INormalizeTemperatureModule
    {
        ITemperatureModule temperatureModuleProxy;
        TemperatureScale scale;

        public Input<TemperatureModuleOutput> Temperature { get; set; }
        public Output<TemperatureModuleOutput> NormalizedTemperature { get; set; }
        public ModuleTwin<NormalizerTwin> Twin { get; set; }

        public NormalizeTemperatureModule(ITemperatureModule proxy)
        {
            temperatureModuleProxy = proxy;
        }

        public override void BuildSubscriptions()
        {

            Temperature.Subscribe(temperatureModuleProxy.Temperature, async (temp) =>
            {
                if (temp.Scale != scale)
                    if (scale == TemperatureScale.Celsius)
                        temp.Temperature = temp.Temperature * 9 / 5 + 32;
                await NormalizedTemperature.PublishAsync(temp);

                if (temp.Temperature > 30)
                    await temperatureModuleProxy.Twin.PublishAsync(new TemperatureTwin() { MaxLimit = 30 });

                return MessageResult.OK;
            });

            Upstream.Subscribe(NormalizedTemperature);

            Twin.Subscribe(async (twin) =>
            {
                if (twin.Scale.HasValue)
                    await Twin.ReportAsync(new NormalizerTwin() { Scale = twin.Scale });

                return TwinResult.OK;
            });

        }
    }
}
