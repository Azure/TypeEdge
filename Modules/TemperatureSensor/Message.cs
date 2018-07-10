using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Amqp.Framing;

namespace Modules
{
    public class Message
    {
        public String chartName;
        public String xlabel;
        public String ylabel;
        public double[][] points;
        public String[] headers;
        public Boolean append;
        public Boolean anomaly;
    }
    public class VisMessage
    {
        public Message[] messages;
    }
}
