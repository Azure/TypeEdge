using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Text;
using MathNet.Numerics.Optimization.ObjectiveFunctions;

namespace sendInfo
{
    class FFT
    {
        //how often (in number of values recieved) to compute and send FFT
        public int SendFrequency;
        public double[] curResult;
        private int cur;
        private int ArraySize;
        public ArrayList Vals;
        public Boolean isChanged;

        public FFT(int sf, int valnum)
        {
            this.SendFrequency = sf;
            this.ArraySize = valnum;

            Vals = new ArrayList();
            for (int i = 0; i < valnum; i++)
            {
                Vals.Add(0);
            }

            cur = 0;
            isChanged = false;
        }

        //give newest value generated, if appropriate we compute and send FFT
        public async void Next(double newVal)
        {
            cur = (cur + 1) % SendFrequency;

            Vals.Insert(0, newVal);
            Vals.RemoveAt(Vals.Count - 1);

            if (cur == 0)
            {
                    //send the new value
                Complex[] fftResult = new Complex[ArraySize];
                for (int i = 0; i < Vals.Count; i++)
                {
                    fftResult[i] = new Complex((double) Vals[i], 0);
                }

                MathNet.Numerics.IntegralTransforms.Fourier.Forward(fftResult);

                this.curResult = new double[ArraySize];
                for (int i = 0; i < fftResult.Length; i++)
                {
                    curResult[i] = fftResult[i].Real;
                }

                isChanged = true;
            }
         
        }

        public void SetArraySize(int newsize)
        {
            if (newsize > ArraySize)
            {
                while (newsize > ArraySize)
                {
                    Vals.Add(0);
                    ArraySize++;
                }
            }
            else if (newsize < ArraySize)
            {
                if (newsize > ArraySize)
                {
                    while (newsize < ArraySize)
                    {
                        Vals.RemoveAt(Vals.Count - 1);
                        ArraySize--;
                    }
                }
            }
        }


    }
}
