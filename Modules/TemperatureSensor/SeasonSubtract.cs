using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;

namespace sendInfo
{
    class SeasonSubtract
    {
        // data members for deciding on the largest period present in the wave form
        private double[] fft;
        private int _trainingSetRequiredSize;
        private int _trainingSetCurrentSize;
        private int learnedMaxPeriod;
        private Boolean isTrained;

        public SeasonSubtract(int setSize)
        {
            this._trainingSetRequiredSize = setSize;
        }

    }
}
