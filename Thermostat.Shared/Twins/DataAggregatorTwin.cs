using TypeEdge.Twins;


namespace ThermostatApplication.Twins
{
    public class DataAggregatorTwin : TypeModuleTwin
    {
       public int TumblingWindowPercentage { get; set; }
       public int AggregationSize { get; set; }
    }

}
