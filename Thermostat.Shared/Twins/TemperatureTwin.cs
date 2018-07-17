using TypeEdge.Twins;
using WaveGenerator;

namespace ThermostatApplication.Twins
{
    public class TemperatureTwin : TypeModuleTwin
    {
        public double SampleRateHz { get; set; }
        public WaveGenerator.WaveConfig WaveConfig { get; set; }
    }
}