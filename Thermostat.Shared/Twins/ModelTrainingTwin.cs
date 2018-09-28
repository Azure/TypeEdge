using Microsoft.Azure.TypeEdge.Twins;

namespace ThermostatApplication.Twins
{
    public class ModelTrainingTwin : TypeTwin
    {
        public int TumblingWindowPercentage { get; set; }
        public int AggregationSize { get; set; }
    }
}