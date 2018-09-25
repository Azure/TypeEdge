using Microsoft.Azure.TypeEdge.Attributes;
using Microsoft.Azure.TypeEdge.Modules.Endpoints;
using Microsoft.Azure.TypeEdge.Modules.Messages;
using ThermostatApplication.Messages;
using Microsoft.Azure.TypeEdge.Twins;
using ThermostatApplication.Twins;

namespace ThermostatApplication.Modules
{
    [TypeModule]
    public interface IModelTraining 
    {
        Output<Model> Model { get; set; }
        ModuleTwin<ModelTrainingTwin> Twin { get; set; }

    }
}