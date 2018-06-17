
using Microsoft.Azure.IoT.TypeEdge.Modules;
using Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.IoT.TypeEdge.Modules.Messages;
using System.Collections.Generic;
using ThermostatApplication.Messages;
using ThermostatApplication.Modules;

namespace Modules
{
    public class DataSampling : EdgeModule, IDataSampling
    {
        List<Temperature> _sample;

        public Input<Temperature> Temperature { get; set; }
        public Output<Reference<Sample>> Samples { get; set; }

        public DataSampling(IPreprocessor proxy)
        {
            _sample = new List<Temperature>();
            Temperature.Subscribe(proxy.Training, async signal =>
            {
                _sample.Add(signal);
                if (_sample.Count > 999)
                {
                    await Samples.PublishAsync(new Reference<Sample>()
                    {
                        Message = new Sample() { Data = _sample.ToArray() }
                    });
                    _sample.Clear();
                }
                return MessageResult.Ok;
            });

        }
    }
}
