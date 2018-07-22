using System;
using System.Collections.Generic;
using System.Text;

namespace ThermostatApplication.ML
{
    public interface IScorer
    {
        void DeserializeModel(string data);
        int Score(double[] point);
    }
}
