using TypeEdge.Twins;


namespace ThermostatApplication.Twins
{
    public class DataAggregatorTwin : TypeModuleTwin
    {
       public int MaxDelayPercentage { get; set; }
       public int AggregationSize { get; set; }
    }

}
