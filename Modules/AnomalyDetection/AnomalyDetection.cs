using System.Linq;
using AnomalyDetectionAlgorithms;
using TypeEdge.Modules;
using TypeEdge.Modules.Endpoints;
using TypeEdge.Modules.Messages;
using ThermostatApplication.Messages;
using ThermostatApplication.Modules;
using System;

namespace Modules
{
    public class AnomalyDetection : EdgeModule, IAnomalyDetection
    {
        object _syncSample = new object();
        object _syncClustering = new object();
        
        int _numClusters = 3;
        KMeansScoring _kMeansScoring;

        public Output<Anomaly> Anomaly { get; set; }

        public AnomalyDetection(IOrchestrator orcherstratorProxy, IModelTraining modelTrainingProxy)
        {
            orcherstratorProxy.Detection.Subscribe(this, async signal =>
            {
                try
                {
                    int cluster = 0;
                    if (_kMeansScoring != null)
                        lock (_syncClustering)
                            if (_kMeansScoring != null)
                                cluster = _kMeansScoring.Score(new double[] { signal.Value });

                    if (cluster < 0)
                        await Anomaly.PublishAsync(new Anomaly() { Temperature = signal });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                return MessageResult.Ok;
            });

            modelTrainingProxy.Model.Subscribe(this, async (model) =>
            {
                //if the messages has been stored and forwarded, but the file has been deleted (e.g. a restart)
                //then the message can be empty (null)
                if (model == null)
                    return MessageResult.Ok;

                try
                {
                    lock (_syncClustering)

                        switch (model.Algorithm)
                        {
                            case ThermostatApplication.Algorithm.kMeans:
                                _kMeansScoring = new KMeansScoring(_numClusters);
                                _kMeansScoring.DeserializeModel(model.DataJson);
                                break;
                            case ThermostatApplication.Algorithm.LSTM:
                                break;
                            default:
                                break;
                        }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                return MessageResult.Ok;
            });
        }
    }
}
