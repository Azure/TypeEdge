using Microsoft.Azure.IoT.EdgeCompose;
using Microsoft.Azure.IoT.EdgeCompose.Modules;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ThermpostatEdgeApplication.Modules
{
    public class TemperatureModule : EdgeModule
    {
        public Output<TemperatureModuleOutput> Temperature { get; set; }
    }
}
