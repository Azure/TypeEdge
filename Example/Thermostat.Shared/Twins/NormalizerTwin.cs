using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using System;

namespace ThermostatApplication.Twins
{
    public class NormalizerTwin : TypeModuleTwin
    {
        public TemperatureScale? Scale { get; set; }
    }
}