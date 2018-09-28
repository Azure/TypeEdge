namespace ThermostatApplication.Twins
{
    public class Waveform
    {
        public double Frequency { get; set; }
        public double Amplitude { get; set; }
        public WaveformType WaveType { get; set; }
        public double VerticalShift { get; set; }
    }
}