using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.TypeEdge.Enums;
using Microsoft.Azure.TypeEdge.Modules;
using Microsoft.Azure.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.TypeEdge.Modules.Enums;
using Microsoft.Azure.TypeEdge.Modules.Messages;
using Microsoft.Azure.TypeEdge.Twins;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ThermostatApplication;
using ThermostatApplication.Messages;
using ThermostatApplication.Modules;
using ThermostatApplication.Twins;

namespace Modules
{
    public class Orchestrator : TypeModule, IOrchestrator
    {
        private readonly object _twinLock = new object();
        private OrchestratorTwin _lastTwin;

        //this is a temp depedency, until we get the feature extraction module
        public Orchestrator(ITemperatureSensor temperatureProxy, IModelTraining trainerProxy)
        {
            temperatureProxy.Temperature.Subscribe(this, async signal =>
            {
                OrchestratorTwin twin;

                lock (_twinLock)
                {
                    twin = _lastTwin;
                }

                if (twin == null) return MessageResult.Ok;
                if (twin.Scale == TemperatureScale.Celsius)
                    signal.Value = signal.Value * 9 / 5 + 32;

                await BroadcastMessage(signal, twin);

                return MessageResult.Ok;
            });

            trainerProxy.Model.Subscribe(this, async model =>
            {
                Logger.LogInformation(
                    $"New Trained Model for {model.Algorithm}. Updating the model in {model.Algorithm}");
                await Model.PublishAsync(model);
                return MessageResult.Ok;
            });

            Twin.Subscribe(async twin =>
            {
                lock (_twinLock)
                {
                    _lastTwin = twin;
                }

                Logger.LogInformation($"Twin update. Routing  = {twin.RoutingMode.ToString()}");
                await Twin.ReportAsync(twin);
                return TwinResult.Ok;
            });
        }

        public Output<DataAggregate> FeatureExtraction { get; set; }

        public Output<Temperature> Training { get; set; }
        public Output<Temperature> Detection { get; set; }
        public Output<GraphData> Visualization { get; set; }
        public Output<Model> Model { get; set; }

        public ModuleTwin<OrchestratorTwin> Twin { get; set; }

        private async Task BroadcastAggregate(Reference<DataAggregate> aggregate, OrchestratorTwin twin)
        {
            var messages = new List<Task>();
            foreach (Routing item in Enum.GetValues(typeof(Routing)))
                if (twin.RoutingMode.HasFlag(item))
                    switch (item)
                    {
                        case Routing.VisualizeFeature:
                            messages.Add(Visualization.PublishAsync(new GraphData
                                {
                                    CorrelationID = "Feature",
                                    Values = aggregate.Message.Values
                                }
                            ));
                            break;
                        case Routing.FeatureExtraction:
                            messages.Add(FeatureExtraction.PublishAsync(new DataAggregate
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
            Logger.LogInformation($"BroadcastMessage : {JsonConvert.SerializeObject(signal)}");

            var messages = new List<Task>();
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
                            messages.Add(Visualization.PublishAsync(new GraphData
                                {
                                    CorrelationID = "Source",
                                    Values = new[] {new double[2] {signal.TimeStamp, signal.Value}}
                                }
                            ));
                            break;
                        default:
                            continue;
                    }

            if (messages.Count > 0)
                await Task.WhenAll(messages).ConfigureAwait(false);
        }

        public override async Task<ExecutionResult> RunAsync(CancellationToken cancellationToken)
        {
            await Twin.GetAsync();
            return ExecutionResult.Ok;
        }
    }
}