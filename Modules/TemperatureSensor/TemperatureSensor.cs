using System;
using System.Threading;
using System.Threading.Tasks;
using TypeEdge.Enums;
using TypeEdge.Modules;
using TypeEdge.Modules.Endpoints;
using TypeEdge.Modules.Enums;
using TypeEdge.Twins;
using ThermostatApplication;
using ThermostatApplication.Messages;
using ThermostatApplication.Modules;
using ThermostatApplication.Twins;
using WaveGenerator;

namespace Modules
{
    public class TemperatureSensor : EdgeModule, ITemperatureSensor
    {
        object _sync = new object();
        double _anomalyOffset = 0.0;
        double _minimum = 60.0;
        double _maximum = 80.0;


        public Output<Temperature> Temperature { get; set; }
        public ModuleTwin<TemperatureTwin> Twin { get; set; }

        public TemperatureSensor()
        {
            Twin.Subscribe(async twin =>
            {
                lock (_sync)
                {
                    _minimum = twin.DesiredMaximum;
                    _maximum = twin.DesiredMaximum;
                }
                await Twin.ReportAsync(twin);
                return TwinResult.Ok;
            });
        }
        
        public void GenerateAnomaly(int value)
        {
            Console.WriteLine($"GenerateAnomaly called with value:{value}");
            lock (_sync)
                _anomalyOffset = value;
        }
        public override async Task<ExecutionResult> RunAsync()
        {
            

            //begin simple generator
            var comps = new WaveConfig[3];
            comps[0] = new WaveConfig(WaveType.Sine, 0.0001, 70);
            comps[1] = new WaveConfig(WaveType.Sine, 0.001, 30);
            comps[2] = new WaveConfig(WaveType.Flat, 1, 1)
            {
                VerticalShift = 130
            };
            var dataGenerator = new WaveGenerator.WaveGenerator(comps);

            var valueCounter = 0;

            //somehow instantiate chart?
            
            var newValue = dataGenerator.Read();
            //Somehow send data

            return newValue;
        }
    }
}