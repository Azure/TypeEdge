using Autofac;
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
    public class NormalizeTemperatureModule : EdgeModule
    {

        public Input<TemperatureModuleOutput> Temperature { get; set; }
        public Output<TemperatureModuleOutput> NormalizedTemperature { get; set; }
    }
}
