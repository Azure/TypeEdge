using Autofac;
using Microsoft.Azure.IoT.EdgeCompose;
using Microsoft.Azure.IoT.EdgeCompose.Hubs;
using Microsoft.Azure.IoT.EdgeCompose.Modules;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ThermpostatEdgeApplication.Modules;

namespace ThermpostatEdgeApplication
{
    public class ThermostatApplication : IoTEdgeApplication
    {
        public ThermostatApplication(IConfigurationRoot configuration)
            : base(configuration)
        {
        }

        public override CompositionResult Compose()
        {
            //setup modules
            var temperatureModule = new TemperatureModule();
            var normalizeTemperatureModule = new NormalizeTemperatureModule();

            Modules.Add(temperatureModule);
            Modules.Add(normalizeTemperatureModule);

            //setup pub/sub
            temperatureModule.DefaultInput.Subscribe(Hub.Downstream, async (msg) => { return MessageResult.OK; });

            normalizeTemperatureModule.Temperature.Subscribe(temperatureModule.Temperature, async (temp) =>
            {
                if (temp.Scale == TemperatureScale.Celsius)
                    temp.Temperature = temp.Temperature * 9 / 5 + 32;

                return MessageResult.OK;
            });

            Hub.Upstream.Subscribe(normalizeTemperatureModule.NormalizedTemperature, async (temp) =>
            {
                return new JsonMessage(temp.ToString());
            });

            //setup startup depedencies
            normalizeTemperatureModule.DependsOn(temperatureModule);

            return CompositionResult.OK;
        }
    }
}
