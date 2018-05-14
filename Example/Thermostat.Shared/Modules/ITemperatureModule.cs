using System;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.TypeEdge;
using Microsoft.Azure.IoT.TypeEdge.Attributes;
using Microsoft.Azure.IoT.TypeEdge.Hubs;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using ThermostatApplication.Messages;
using ThermostatApplication.Twins;

namespace ThermostatApplication.Modules
{
    [TypeModule(Name = "Temperature")]
    public interface ITemperatureModule
    {
        Output<TemperatureModuleOutput> Temperature { get; set; }                
        ModuleTwin<TemperatureTwin> Twin { get; set; }

        bool ResetSensor(int sensitivity);
    }
}