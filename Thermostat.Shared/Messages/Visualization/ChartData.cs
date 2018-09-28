namespace ThermostatApplication.Messages.Visualization
{
    public class ChartData
    {
        public Chart Chart { get; set; }
        public double[][] Points { get; set; }
        public bool IsAnomaly { get; set; }
    }
}