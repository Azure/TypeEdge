using TypeEdge.Twins;
using System;

namespace ThermostatApplication.Twins
{
    [Flags]
    public enum Routing : int
    {
        None = 1 << 0,
        Sampling = 1 << 1,
        Detect = 1 << 2,
        Visualize = 1 << 3,
        FeatureExtraction = 1 << 4
    }

    public class OrchestratorTwin : TypeModuleTwin
    {
        public TemperatureScale Scale { get; set; }

        public Routing RoutingMode { get; set; }
    }
}