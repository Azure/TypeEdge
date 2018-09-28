namespace ThermostatApplication.ML
{
    public interface IScorer
    {
        void DeserializeModel(string data);
        int Score(double[] point);
    }
}