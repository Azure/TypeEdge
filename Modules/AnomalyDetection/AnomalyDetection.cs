using System.Linq;
using AnomalyDetectionAlgorithms;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.IoT.TypeEdge.Modules.Messages;
using ThermostatApplication.Messages;
using ThermostatApplication.Modules;

namespace Modules
{
    public class AnomalyDetection : EdgeModule, IAnomalyDetection
    {
        object _syncSample = new object();
        object _syncClustering = new object();

        double[][] _sample = null;
        int _numClusters = 3;
        KMeansClustering _kMeansClustering;

        public Output<Anomaly> Anomaly { get; set; }


        public AnomalyDetection(IOrchestrator preprocessor, IDataSampling trainer)
        {
            preprocessor.Detection.Subscribe(this, async signal =>
            {
                int cluster = 0;
                if (_kMeansClustering != null)
                    lock (_syncClustering)
                        if (_kMeansClustering != null)
                            cluster = _kMeansClustering.Classify(new double[] { signal.Value, signal.Minimum, signal.Maximum });

                if (cluster < 0)
                {
                    System.Console.WriteLine("_____________________________Anomaly detected__________________________________");
                    await Anomaly.PublishAsync(new Anomaly() { Temperature = signal });
                }
                return MessageResult.Ok;
            });

            trainer.Samples.Subscribe(this, async (sampleReference) =>
            {
                //if the messages has been stored and forwarded, but the file has been deleted (e.g. a restart)
                //then the message can be empty (null)
                if (sampleReference == null)
                    return MessageResult.Ok;

                System.Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");

                lock (_syncSample)
                    _sample = sampleReference.Message.Data.Select(e => new double[] { e.Value, e.Minimum, e.Maximum }).ToArray();

                lock (_syncClustering)
                    _kMeansClustering = new KMeansClustering(_sample, _numClusters);

                return MessageResult.Ok;
            });
        }


    }
}
