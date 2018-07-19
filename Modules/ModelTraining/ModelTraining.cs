using System.Linq;
using TypeEdge.Modules;
using TypeEdge.Modules.Endpoints;
using TypeEdge.Modules.Messages;
using System.Collections.Generic;
using ThermostatApplication.Messages;
using ThermostatApplication.Modules;
using ThermostatApplication.Twins;
using TypeEdge.Twins;
using TypeEdge.Modules.Enums;
using System;
using AnomalyDetectionAlgorithms;

namespace Modules
{
    public class ModelTraining : EdgeModule, IModelTraining
    {
        object _sync = new object();

        //default values
        int _aggregationSize = 10;
        int _tumblingWindowPercentage = 50;

        Queue<Temperature> _sample;

        KMeansTraining _kMeansTraining;
        int _numClusters = 3;

        public Output<Reference<Model>> Model { get; set; }
        public ModuleTwin<ModelTrainingTwin> Twin { get; set; }


        public ModelTraining(IOrchestrator proxy)
        {
            _sample = new Queue<Temperature>();

            Twin.Subscribe(async twin =>
            {
                Console.WriteLine($"{typeof(ModelTraining).Name}::Twin update");

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
                Reference<Model> model = null;
                lock (_sample)
                {
                    _sample.Enqueue(signal);
                    if (_sample.Count >= _aggregationSize)
                    {
                        _kMeansTraining = new KMeansTraining( _numClusters);
                        _kMeansTraining.TrainModel(_sample.Select(e => new double[] { e.TimeStamp, e.Value }).ToArray());
                        model = new Reference<Model>()
                        {
                            Message = new Model() {
                                 Algorithm = ThermostatApplication.Algorithm.kMeans,
                                DataJson = _kMeansTraining.SerializeModel()
                            }
                        };
                        for (int i = 0; i < _tumblingWindowPercentage * _aggregationSize / 100; i++)
                            _sample.Dequeue();
                    }
                }
                if (model != null)
                    await Model.PublishAsync(model);

                return MessageResult.Ok;
            });
        }
    }
}
