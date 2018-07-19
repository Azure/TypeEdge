using TypeEdge.Twins;


namespace ThermostatApplication.Twins
{
    public class ModelTrainingTwin : TypeModuleTwin
    {
       public int TumblingWindowPercentage { get; set; }
       public int AggregationSize { get; set; }
    }

}
