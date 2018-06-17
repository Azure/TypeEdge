
using Microsoft.Azure.IoT.TypeEdge.Modules;
using Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.IoT.TypeEdge.Modules.Messages;
using ThermostatApplication.Messages;
using ThermostatApplication.Modules;

namespace Modules
{
    public class AnomalyDetection : EdgeModule, IAnomalyDetection
    {
        public Input<Temperature> Temperature { get; set; }
        public Input<Reference<Sample>> Samples { get; set; }
        public Output<Anomaly> Anomaly { get; set; }

        public AnomalyDetection(IPreprocessor preprocessor, IDataSampling trainer)
        {
            Temperature.Subscribe(preprocessor.Detection, async signal =>
            {
                return MessageResult.Ok;
            });
            Samples.Subscribe(trainer.Samples, async (sampleReference) =>
            {
                System.Console.WriteLine("New Sample");
                System.Console.WriteLine($"Length = {sampleReference.Message.Data.Length}");
                return MessageResult.Ok;
            });
        }
    }
}
