using Microsoft.Azure.IoT.TypeEdge.Attributes;
using Microsoft.Azure.IoT.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.IoT.TypeEdge.Twins;
using ThermostatApplication.Messages;
using ThermostatApplication.Twins;

namespace ThermostatApplication.Modules
{
    [TypeModule]
    public interface IPreprocessor
    {
        Output<Temperature> Training { get; set; }
        Output<Temperature> Detection { get; set; }

        ModuleTwin<PreprocessorTwin> Twin { get; set; }
    }
}