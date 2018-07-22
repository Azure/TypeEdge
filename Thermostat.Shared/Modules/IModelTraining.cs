using TypeEdge.Attributes;
using TypeEdge.Modules.Endpoints;
using TypeEdge.Modules.Messages;
using ThermostatApplication.Messages;
using TypeEdge.Twins;
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