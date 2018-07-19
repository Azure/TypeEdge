using System;

namespace ThermostatApplication.Messages.Visualization
{
    public class ChartData
    {
        public Chart Chart { get; set; }
        public double[][] Points { get; set; }
        public Boolean IsAnomaly { get; set; }
    }
}
