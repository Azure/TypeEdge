namespace ThermostatApplication.ML
{
    public interface ITrainer
    {
        string SerializeModel();
        void TrainModel(double[][] rawData);
    }
}