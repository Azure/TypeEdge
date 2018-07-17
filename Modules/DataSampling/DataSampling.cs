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

namespace Modules
{
    public class DataSampling : EdgeModule, IDataAggregator
    {
        object _sync = new object();
        int _aggregationSize;
        int _maxDelayPercentage;

        Queue<Temperature> _sample;

        public Output<Reference<DataAggregate>> Aggregate { get; set; }
        public ModuleTwin<DataAggregatorTwin> Twin { get; set; }


        public DataSampling(IOrchestrator proxy)
        {

            Twin.Subscribe(async twin =>
            {
                
                lock (_sync)
                {
                    _aggregationSize = twin.AggregationSize;
                    _maxDelayPercentage = twin.MaxDelayPercentage;
                }
                await Twin.ReportAsync(twin);
                return TwinResult.Ok;
            });


            _sample = new Queue<Temperature>();
            proxy.Sampling.Subscribe(this, async signal =>
            {
                Reference<DataAggregate> message = null;
                lock (_sample)
                {
                    _sample.Enqueue(signal);
                    if (_sample.Count >= _aggregationSize)
                    {
                        message = new Reference<DataAggregate>()
                        {
                            Message = new DataAggregate()
                            {
                                Values = _sample.Select(e => new double[2] { e.TimeStamp, e.Value }).ToArray(),
                                CorrelationID = "IDataAggregator.Sampling"
                            }
                        };
                        for (int i = 0; i < _maxDelayPercentage * _aggregationSize / 100; i++)
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
