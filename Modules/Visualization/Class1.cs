using System;

namespace Modules
{
    // This object, Chart, is what users will set up. We need to parse it and turn it into messages.
    // These variables shouldn't ever have to change, so the end user will just see this as a 
    // startup cost.
    public class Chart
    {
        public String chartName;
        public String xlabel;
        public String ylabel;
        public String[] headers; // The headers for the data. It's up to the user to find the correct size
        public Boolean append; // Append decides whether we append the data each time, or treat it as the entire graph
    }
}
