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




        //public void TrainPeriod(double[] fftResults) {

        //    double[] compositeFreqs = FindPeaks(fftResults, 3);
        //    averageFreqs(compositeFreqs);
        //}










        //public static IList<int> FindPeaks(IList<double> values, int rangeOfPeaks)
        //{
        //    List<int> peaks = new List<int>();
        //    double current;
        //    IEnumerable<double> range;

        //    int checksOnEachSide = rangeOfPeaks / 2;
        //    for (int i = 0; i < values.Count; i++)
        //    {
        //        current = values[i];
        //        range = values;

        //        if (i > checksOnEachSide)
        //        {
        //            range = range.Skip(i - checksOnEachSide);
        //        }

        //        range = range.Take(rangeOfPeaks);
        //        if ((range.Count() > 0) && (current == range.Max()))
        //        {
        //            peaks.Add(i);
        //        }
        //    }

        //    return peaks;
        //}


    }
}
