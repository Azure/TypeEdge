using System;
using Microsoft.Azure.TypeEdge.Modules.Messages;

namespace ThermostatApplication.Messages.Visualization
{
    public class Chart
    {
        public String Name { get; set; }
        public String X_Label { get; set; }
        public String Y_Label { get; set; }
        public String[] Headers { get; set; } 
        public Boolean Append { get; set; } 
    }
}
