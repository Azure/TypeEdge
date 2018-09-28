using System.Collections.Generic;
using System.Linq;
using AnomalyDetectionAlgorithms;
using Microsoft.Azure.TypeEdge.Modules;
using Microsoft.Azure.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.TypeEdge.Modules.Enums;
using Microsoft.Azure.TypeEdge.Modules.Messages;
using Microsoft.Azure.TypeEdge.Twins;
using Microsoft.Extensions.Logging;
using ThermostatApplication;
using ThermostatApplication.Messages;
using ThermostatApplication.Modules;
using ThermostatApplication.Twins;

namespace Modules
{
    public class ModelTraining : TypeModule, IModelTraining
    {
        private readonly int _numClusters = 3;

        private readonly object _sync = new object();

        //default values
        private int _aggregationSize = 400;

        private int _tumblingWindowPercentage = 10;


        public ModelTraining(IOrchestrator proxy)
        {
            var sample = new Queue<Temperature>();

            Twin.Subscribe(async twin =>
            {
                Logger.LogInformation("Twin update");

                lock (_sync)
                {
                    _aggregationSize = twin.AggregationSize;
                    _tumblingWindowPercentage = twin.TumblingWindowPercentage;
                }

                await Twin.ReportAsync(twin);
                return TwinResult.Ok;
            });

            proxy.Training.Subscribe(this, async signal =>
            {
                Model model = null;
                lock (sample)
                {
                    sample.Enqueue(signal);
                    if (sample.Count >= _aggregationSize)
                    {
                        var kMeansTraining = new KMeansTraining(_numClusters);
                        kMeansTraining.TrainModel(sample.Select(e => new[] {e.TimeStamp, e.Value}).ToArray());
                        model = new Model
                        {
                            Algorithm = Algorithm.kMeans,
                            DataJson = kMeansTraining.SerializeModel()
                        };
                        for (var i = 0; i < _tumblingWindowPercentage * _aggregationSize / 100; i++)
                            sample.Dequeue();
                    }
                }

                if (model != null)
                    await Model.PublishAsync(model).ConfigureAwait(false);

                return MessageResult.Ok;
            });
        }

        public Output<Model> Model { get; set; }
        public ModuleTwin<ModelTrainingTwin> Twin { get; set; }
    }
}