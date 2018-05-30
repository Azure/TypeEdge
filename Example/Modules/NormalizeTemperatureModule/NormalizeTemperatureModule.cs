using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.TypeEdge.Enums;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.IoT.TypeEdge.Modules.Enums;
using Microsoft.Azure.IoT.TypeEdge.Modules.Messages;
using Microsoft.Azure.IoT.TypeEdge.Twins;
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
            while (true)
            {
                Console.WriteLine("Temporary loop");
                Thread.Sleep(1000);
            }

            var twin = await Twin.GetAsync();
            _scale = twin.Scale;

            return ExecutionResult.Ok;
        }

        public override void BuildSubscriptions()
        {
            Temperature.Subscribe(_temperatureModuleProxy.Temperature, async temp =>
            {
                Console.WriteLine("New Message in NormalizeTemperatureModule.");
                if (temp.Scale != _scale)
                    if (_scale == TemperatureScale.Celsius)
                        temp.Temperature = temp.Temperature * 9 / 5 + 32;
                await NormalizedTemperature.PublishAsync(temp);

                return MessageResult.Ok;
            });

            Twin.Subscribe(async twin =>
            {
                _scale = twin.Scale;
                await Twin.ReportAsync(twin);
                return TwinResult.Ok;
            });
        }
    }
}