using System.Linq;
using TypeEdge.Modules;
using TypeEdge.Modules.Endpoints;
using TypeEdge.Modules.Messages;
using System.Collections.Generic;
using ThermostatApplication.Messages;
using ThermostatApplication.Modules;

namespace Modules
{
    public class DataSampling : EdgeModule, IDataAggregator
    {
        const int _windowMaxSamples = 1000;
        const int _maxDelayPercentage = 10;

        Queue<Temperature> _sample;

        public Output<Reference<DataAggregate>> Aggregate { get; set; }

        public DataSampling(IOrchestrator proxy)
        {
            _sample = new Queue<Temperature>();
            proxy.Sampling.Subscribe(this, async signal =>
            {
                Reference<DataAggregate> message = null;
                lock (_sample)
                {
                    _sample.Enqueue(signal);
                    if (_sample.Count >= _windowMaxSamples)
                    {
                        message = new Reference<DataAggregate>()
                        {
                            Message = new DataAggregate()
                            {
                                Values = _sample.Select(e => e.Value).ToArray(),
                                CorrelationID = "IDataAggregator.Sampling"
                            }
                        };
                        for (int i = 0; i < _maxDelayPercentage * _windowMaxSamples / 100; i++)
                            _sample.Dequeue();
                    }
                }
                if (message != null)
                    await Aggregate.PublishAsync(message);

                return MessageResult.Ok;
            });

        }
    }
}
