using System;

namespace Modules
{
    // The following class, message, is specific to our visualizer. This is the object pushed
    // to the JavaScript. VisMessage is an array of Messages, which is sent to the user. We 
    // have to build this object every time we want to send an update, but we don't want to 
    // make the user feel like they have to.
    public class RenderData
    {
        public String chartName;
        public String xlabel;
        public String ylabel;
        public double[][] points;
        public String[] headers;
        public Boolean append; // Append decides whether we append the data each time, or treat it as the entire graph
        public Boolean anomaly; // If this message has anomalies, set this to true.
    }
}
