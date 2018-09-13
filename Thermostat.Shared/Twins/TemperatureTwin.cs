using TypeEdge.Twins;

namespace ThermostatApplication.Twins
{
    public class TemperatureTwin : TypeTwin
    {
        public double SamplingHz { get; set; }
        public double Frequency { get; set; }
        public double Amplitude { get; set; }
        public WaveformType WaveType { get; set; }
        public double Offset { get; set; }

        //TODO: convert this to array-ish 
        //public Waveform Waveform { get; set; }
    }
}