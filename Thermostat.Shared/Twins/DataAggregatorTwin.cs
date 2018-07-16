using TypeEdge.Twins;


namespace Thermostat.Shared.Twins
{
    public class DataAggregatorTwin : TypeModuleTwin
    {
       public int MaxDelayPercentage { get; set; }
       public int AggregationSize { get; set; }
    }

}
