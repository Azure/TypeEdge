using System;
using System.Collections.Generic;
using System.Text;

namespace ThermostatApplication.ML
{
    public interface ITrainer
    {
        string SerializeModel();
        void TrainModel(double[][] rawData);
    }
}
