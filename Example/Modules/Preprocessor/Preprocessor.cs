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
    public class Preprocessor : EdgeModule, IPreprocessor
    {
        public Input<Temperature> Temperature { get; set; }
        public Output<Temperature> Training { get; set; }
        public Output<Temperature> Detection { get; set; }
        public ModuleTwin<PreprocessorTwin> Twin { get; set; }

        public Preprocessor(ITemperatureSensor proxy)
        {
            Temperature.Subscribe(proxy.Temperature, async signal =>
            {
                var twin = Twin.LastKnownTwin;
                if (twin != null)
                {
                    if (signal.Scale != twin.Scale)
                        if (twin.Scale == TemperatureScale.Celsius)
                            signal.Value = signal.Value * 9 / 5 + 32;

                    await RouteMessageAsync(signal, twin.RoutingMode);
                }
                return MessageResult.Ok;
            });

            Twin.Subscribe(async twin =>
            {
                System.Console.WriteLine($"Preprocessor: new routing : { twin.RoutingMode.ToString()}");
                await Twin.ReportAsync(twin);
                return TwinResult.Ok;
            });
        }

        private async Task RouteMessageAsync(Temperature signal, Routing mode)
        {
            switch (mode)
            {
                case Routing.None:
                    break;
                case Routing.Train:
                    await Training.PublishAsync(signal);
                    break;
                case Routing.Detect:
                    await Detection.PublishAsync(signal);
                    break;
                case Routing.Both:
                    var results = new Task<PublishResult>[] {
                        Training.PublishAsync(signal),
                        Detection.PublishAsync(signal)
                    };
                    await Task.WhenAll(results);
                    break;
                default:
                    break;
            }
        }

        public override async Task<ExecutionResult> RunAsync()
        {
            await Twin.GetAsync();
            return ExecutionResult.Ok;
        }
    }
}