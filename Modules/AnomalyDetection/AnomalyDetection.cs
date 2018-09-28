using System;
using System.Threading.Tasks;
using AnomalyDetectionAlgorithms;
using Microsoft.Azure.TypeEdge.Modules;
using Microsoft.Azure.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.TypeEdge.Modules.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ThermostatApplication;
using ThermostatApplication.Messages;
using ThermostatApplication.Modules;

namespace Modules
{
    public class AnomalyDetection : TypeModule, IAnomalyDetection
    {
        private readonly object _syncClustering = new object();

        private KMeansScoring _kMeansScoring;

        public AnomalyDetection(IConfigurationRoot config)
        {
            var kMeansClusters = Convert.ToInt32(config["kMeansClusters"]);

            GetProxy<IOrchestrator>().Detection.Subscribe(this, async signal =>
            {
                try
                {
                    var cluster = 0;
                    if (_kMeansScoring != null)
                        lock (_syncClustering)
                        {
                            if (_kMeansScoring != null)
                                cluster = _kMeansScoring.Score(new[] {signal.Value});
                        }

                    if (cluster < 0)
                        await Anomaly.PublishAsync(new Anomaly {Temperature = signal});
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Error processing {signal.ToString()}");
                }

                return MessageResult.Ok;
            });

            GetProxy<IOrchestrator>().Model.Subscribe(this, model =>
            {
                //if the messages have been stored and forwarded, but the file has been deleted (e.g. a restart)
                //then the message can be empty (null)
                if (model == null)
                    return Task.FromResult(MessageResult.Ok);

                try
                {
                    lock (_syncClustering)

                    {
                        switch (model.Algorithm)
                        {
                            case Algorithm.kMeans:
                                _kMeansScoring = new KMeansScoring(kMeansClusters);
                                _kMeansScoring.DeserializeModel(model.DataJson);
                                break;
                            case Algorithm.LSTM:
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Error processing {model.ToString()}");
                }

                return Task.FromResult(MessageResult.Ok);
            });
        }

        public Output<Anomaly> Anomaly { get; set; }
    }
}