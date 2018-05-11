using Microsoft.Azure.IoT.TypeEdge.Modules;
using ThermostatApplication;
using ThermostatApplication.Messages;
using ThermostatApplication.Modules;

namespace Modules
{
    public class NormalizeTemperatureModule : EdgeModule, INormalizeTemperatureModule
    {
        ITemperatureModule temperatureModuleProxy;

        public Input<TemperatureModuleOutput> Temperature { get; set; }
        public Output<TemperatureModuleOutput> NormalizedTemperature { get; set; }

        public NormalizeTemperatureModule(ITemperatureModule proxy)
        {
            temperatureModuleProxy = proxy;
        }

        public override void ConfigureSubscriptions() {

            Temperature.Subscribe(temperatureModuleProxy.Temperature, async (temp) =>
            {
                if (temp.Scale == TemperatureScale.Celsius)
                    temp.Temperature = temp.Temperature * 9 / 5 + 32;

                await NormalizedTemperature.PublishAsync(temp);

                return MessageResult.OK;
            });

            Upstream.Subscribe(NormalizedTemperature);
        }
    }
}
