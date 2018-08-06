using System.Linq;
using AnomalyDetectionAlgorithms;
using TypeEdge.Modules;
using TypeEdge.Modules.Endpoints;
using TypeEdge.Modules.Messages;
using ThermostatApplication.Messages;
using ThermostatApplication.Modules;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Modules
{
    public class AnomalyDetection : EdgeModule, IAnomalyDetection
    {
        readonly object _syncSample = new object();
        object _syncClustering = new object();
        
        int kMeansClusters = 4;
        KMeansScoring _kMeansScoring;

        public Output<Anomaly> Anomaly { get; set; }

        public AnomalyDetection(IConfigurationRoot config)
        {
            kMeansClusters = Convert.ToInt32(config["kMeansClusters"]);

            GetProxy<IOrchestrator>().Detection.Subscribe(this, async signal =>
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

            GetProxy<IModelTraining>().Model.Subscribe(this, (model) =>
            {
                //if the messages has been stored and forwarded, but the file has been deleted (e.g. a restart)
                //then the message can be empty (null)
                if (model == null)
                    return Task.FromResult(MessageResult.Ok);

                try
                {
                    lock (_syncClustering)

                        switch (model.Algorithm)
                        {
                            case ThermostatApplication.Algorithm.kMeans:
                                _kMeansScoring = new KMeansScoring(kMeansClusters);
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

                return Task.FromResult(MessageResult.Ok);
            });
        }
    }
}
