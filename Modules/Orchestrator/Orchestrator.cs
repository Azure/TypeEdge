using System;
using System.Collections.Generic;
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
using Newtonsoft.Json;

namespace Modules
{
    public class Orchestrator : EdgeModule, IOrchestrator
    {
        public Output<Temperature> Training { get; set; }
        public Output<Temperature> Detection { get; set; }
        public Output<GraphData> Visualization { get; set; }
        public Output<Model> Model { get; set; }
        public Output<DataAggregate> FeatureExtraction { get; set; }

        public ModuleTwin<OrchestratorTwin> Twin { get; set; }

        //this is a temp depedency, until we get the feature extraction module
        public Orchestrator(ITemperatureSensor temperatureProxy)
        {
            temperatureProxy.Temperature.Subscribe(this, async signal =>
            {
                var twin = Twin.LastKnownTwin;
                if (twin != null)
                {
                    if (twin.Scale == TemperatureScale.Celsius)
                        signal.Value = signal.Value * 9 / 5 + 32;

                    await BroadcastMessage(signal, twin);
                }
                return MessageResult.Ok;
            });

            Twin.Subscribe(async twin =>
            {
                Console.WriteLine($"{typeof(OrchestratorTwin).Name}::Twin update. Routing  = { twin.RoutingMode.ToString()}");
                await Twin.ReportAsync(twin);
                return TwinResult.Ok;
            });


        }

        private async Task BroadcastAggregate(Reference<DataAggregate> aggregate, OrchestratorTwin twin)
        {
            List<Task> messages = new List<Task>();
            foreach (Routing item in Enum.GetValues(typeof(Routing)))
                if (twin.RoutingMode.HasFlag(item))
                    switch (item)
                    {
                        case Routing.VisualizeFeature:
                            messages.Add(Visualization.PublishAsync(new GraphData()
                            {
                                CorrelationID = "Feature",
                                Values = aggregate.Message.Values
                            }
                            ));
                            break;
                        case Routing.FeatureExtraction:
                            messages.Add(FeatureExtraction.PublishAsync(new DataAggregate()
                            {
                                CorrelationID = "Feature",
                                Values = aggregate.Message.Values
                            }));
                            break;

                        default:
                            continue;
                    }

            if (messages.Count > 0)
                await Task.WhenAll(messages);
        }

        private async Task BroadcastMessage(Temperature signal, OrchestratorTwin twin)
        {
            Console.WriteLine($"Orchestrator.BroadcastMessage : {JsonConvert.SerializeObject(signal)}");

            List<Task> messages = new List<Task>();
            foreach (Routing item in Enum.GetValues(typeof(Routing)))
                if (twin.RoutingMode.HasFlag(item))
                    switch (item)
                    {
                        case Routing.Train:
                            messages.Add(Training.PublishAsync(signal));
                            break;
                        case Routing.Detect:
                            messages.Add(Detection.PublishAsync(signal));
                            break;
                        case Routing.VisualizeSource:
                            messages.Add(Visualization.PublishAsync(new GraphData()
                            {
                                CorrelationID = "Source",
                                Values = new double[1][] { new double[2] { signal.TimeStamp, signal.Value } }
                            }
                            ));
                            break;
                        default:
                            continue;
                    }

            if (messages.Count > 0)
                await Task.WhenAll(messages).ConfigureAwait(false);
        }

        public override async Task<ExecutionResult> RunAsync()
        {
            await Twin.GetAsync();
            return ExecutionResult.Ok;
        }
    }
}