using TypeEdge.Twins;

namespace ThermostatApplication.Twins
{
    public class TemperatureTwin : TypeModuleTwin
    {
        public double SamplingHz { get; set; }
        public double Frequency { get; set; }
        public double Amplitude { get; set; }
        public WaveformType WaveType { get; set; }
        public double VerticalShift { get; set; }

        //TODO: convert this to array-ish 
        //public Waveform Waveform { get; set; }
    }
}