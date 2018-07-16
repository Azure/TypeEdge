using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TypeEdge.Enums;
using TypeEdge.Modules;
using TypeEdge.Modules.Endpoints;
using TypeEdge.Modules.Enums;
using TypeEdge.Modules.Messages;
using TypeEdge.Twins;
using ThermostatApplication;
using ThermostatApplication.Messages;
using ThermostatApplication.Modules;
using ThermostatApplication.Twins;

namespace Modules
{
    public class Orchestrator : EdgeModule, IOrchestrator
    {
        public Output<Temperature> Sampling { get; set; }
        public Output<Temperature> Detection { get; set; }
        public Output<GraphData> Visualization { get; set; }
        public Output<DataAggregate> FeatureExtraction { get; set; }

        public ModuleTwin<OrchestratorTwin> Twin { get; set; }

        public Orchestrator(ITemperatureSensor temperatureProxy, IDataAggregator aggregatorProxy)
        {
            temperatureProxy.Temperature.Subscribe(this, async signal =>
            {
                var twin = Twin.LastKnownTwin;
                if (twin != null)
                {
                    Preprocess(signal, twin);

                    List<Task> messages = new List<Task>();
                    foreach (Routing item in Enum.GetValues(typeof(Routing)))
                        if (twin.RoutingMode.HasFlag(item))
                            switch (item)
                            {
                                case Routing.Sampling:
                                    messages.Add(Sampling.PublishAsync(signal));
                                    break;
                                case Routing.Detect:
                                    messages.Add(Detection.PublishAsync(signal));
                                    break;
                                default:
                                    continue;

                            }

                    if (messages.Count > 0)
                        await Task.WhenAll(messages);
                }
                return MessageResult.Ok;
            });


            aggregatorProxy.Aggregate.Subscribe(this, async aggregate =>
            {

                if (aggregate == null)
                    return MessageResult.Ok;

                var twin = Twin.LastKnownTwin;
                if (twin != null)
                {
                    List<Task> messages = new List<Task>();
                    foreach (Routing item in Enum.GetValues(typeof(Routing)))
                        if (twin.RoutingMode.HasFlag(item))
                            switch (item)
                            {
                                case Routing.Visualize:
                                    messages.Add(Visualization.PublishAsync(
                                    new GraphData()
                                        {
                                            CorrelationID = aggregate.Message.CorrelationID,
                                            Values = aggregate.Message.Values
                                        }
                                    ));
                                    break;
                                case Routing.FeatureExtraction:
                                    messages.Add(FeatureExtraction.PublishAsync(new DataAggregate()
                                    {
                                        CorrelationID = aggregate.Message.CorrelationID,
                                        Values = aggregate.Message.Values
                                    }));
                                    break;

                                default:
                                    continue;
                            }

                    if (messages.Count > 0)
                        await Task.WhenAll(messages);
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

        private static void Preprocess(Temperature signal, OrchestratorTwin twin)
        {
            if (signal.Scale != twin.Scale)
                if (twin.Scale == TemperatureScale.Celsius)
                    signal.Value = signal.Value * 9 / 5 + 32;
        }

        public override async Task<ExecutionResult> RunAsync()
        {
            await Twin.GetAsync();
            return ExecutionResult.Ok;
        }
    }
}