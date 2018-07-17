using TypeEdge.Twins;
using System;
using System.Collections.Generic;

namespace ThermostatApplication.Twins
{
    public class VisualizationTwin : TypeModuleTwin
    {
        public string ChartName {get; set;}
        public string XAxisLabel {get; set;}
        public string YAxisLabel {get; set;}
        public Dictionary<int, string> Headers { get; set; }
        public Boolean Append;
    }
}