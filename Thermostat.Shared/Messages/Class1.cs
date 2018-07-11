using System;
using System.Collections.Generic;
using System.Text;

namespace ThermostatApplication.Messages
{
    class TemperatureVisualizationData : Temperature
    {
        public Chart chart { get; set; }
        public Update update { get; set; }
    }
}
