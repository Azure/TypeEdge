using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Amqp.Framing;

namespace sendInfo
{
    class Message
    {
        public class NewValue
        {
            public String[] Headers;
            public double[] Inputs;
        }

        public NewValue NewVal = new NewValue();
        public double[] FFTResult;
        public double[] SubtractionGraphResult;
        public int Timestamp;
    }
}
