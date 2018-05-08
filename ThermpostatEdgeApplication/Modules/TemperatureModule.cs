using Microsoft.Azure.IoT.EdgeCompose;
using Microsoft.Azure.IoT.EdgeCompose.Modules;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThermpostatEdgeApplication.Modules
{
    public class TemperatureModule : EdgeModule
    {
        public Output<TemperatureModuleOutput> Temperature { get; set; }

        public override async Task<ExecutionResult> RunAsync()
        {
            while (true)
            {
                await Temperature.PublishAsync(new TemperatureModuleOutput() { Scale = TemperatureScale.Celsius, Temperature = new Random().NextDouble() });
                Thread.Sleep(1000);
            }
            return await base.RunAsync();
        }
    }
}
