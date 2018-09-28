using System;
using Microsoft.Azure.TypeEdge.Twins;

namespace ThermostatApplication.Twins
{
    [Flags]
    public enum Routing
    {
        None = 1 << 0,
        Train = 1 << 1,
        Detect = 1 << 2,
        VisualizeSource = 1 << 3,
        FeatureExtraction = 1 << 4,
        VisualizeFeature = 1 << 5
    }

    public class OrchestratorTwin : TypeTwin
    {
        public TemperatureScale Scale { get; set; }

        public Routing RoutingMode { get; set; }
    }
}