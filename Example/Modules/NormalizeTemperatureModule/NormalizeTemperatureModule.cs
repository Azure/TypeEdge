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
        private TemperatureScale _scale;
        private readonly ITemperatureModule _temperatureModuleProxy;

        public NormalizeTemperatureModule(ITemperatureModule proxy)
        {
            _temperatureModuleProxy = proxy;
        }

        public Input<TemperatureModuleOutput> Temperature { get; set; }
        public Output<TemperatureModuleOutput> NormalizedTemperature { get; set; }
        public ModuleTwin<NormalizerTwin> Twin { get; set; }

        public override async Task<ExecutionResult> RunAsync()
        {
            var twin = await Twin.GetAsync();
            _scale = twin.Scale;

            return ExecutionResult.OK;
        }

        public override void BuildSubscriptions()
        {
            Temperature.Subscribe(_temperatureModuleProxy.Temperature, async temp =>
            {
                if (temp.Scale != _scale)
                    if (_scale == TemperatureScale.Celsius)
                        temp.Temperature = temp.Temperature * 9 / 5 + 32;
                await NormalizedTemperature.PublishAsync(temp);

                return MessageResult.OK;
            });

            Twin.Subscribe(async twin =>
            {
                _scale = twin.Scale;
                await Twin.ReportAsync(twin);
                return TwinResult.OK;
            });
        }
    }
}