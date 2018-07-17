using TypeEdge.Twins;
using WaveGenerator;

namespace ThermostatApplication.Twins
{
    public class TemperatureTwin : TypeModuleTwin
    {
        public double DesiredMaximum { get; set; }
        public double DesiredMinimum { get; set; }
        public double SampleRateHz { get; set; }
        public WaveGenerator.WaveConfig WaveConfig { get; set; }
    }
}