using System.Linq;
using AnomalyDetectionAlgorithms;
using TypeEdge.Modules;
using TypeEdge.Modules.Endpoints;
using TypeEdge.Modules.Messages;
using ThermostatApplication.Messages;
using ThermostatApplication.Modules;

namespace Modules
{
    public class AnomalyDetection : EdgeModule, IAnomalyDetection
    {
        object _syncSample = new object();
        object _syncClustering = new object();
        
        int _numClusters = 3;
        KMeansScoring _kMeansScoring;

        public Output<Anomaly> Anomaly { get; set; }

        public AnomalyDetection(IOrchestrator orcherstratorProxy)
        {
            orcherstratorProxy.Detection.Subscribe(this, async signal =>
            {
                int cluster = 0;
                if (_kMeansScoring != null)
                    lock (_syncClustering)
                        if (_kMeansScoring != null)
                            cluster = _kMeansScoring.Score(new double[] { signal.Value });

                if (cluster < 0)
                {
                    System.Console.WriteLine("_____________________________Anomaly detected__________________________________");
                    await Anomaly.PublishAsync(new Anomaly() { Temperature = signal });
                }
                return MessageResult.Ok;
            });

            orcherstratorProxy.Model.Subscribe(this, async (model) =>
            {
                //if the messages has been stored and forwarded, but the file has been deleted (e.g. a restart)
                //then the message can be empty (null)
                if (model == null)
                    return MessageResult.Ok;

                System.Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");

                lock (_syncClustering)

                    switch (model.Message.Algorithm)
                    {
                        case ThermostatApplication.Algorithm.kMeans:
                            _kMeansScoring = new KMeansScoring(_numClusters);
                            _kMeansScoring.DeserializeModel(model.Message.DataJson);
                            break;
                        case ThermostatApplication.Algorithm.LSTM:
                            break;
                        default:
                            break;
                    }
                

                return MessageResult.Ok;
            });
        }
    }
}
