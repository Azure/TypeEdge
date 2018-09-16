using Autofac;
using Docker.DotNet;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TypeEdge.Enums;
using TypeEdge.Modules;
using TypeEdge.Modules.Enums;

namespace TypeEdge.Host.Docker
{
    internal class DockerModuleHost 
    {
        DockerClient _dockerClient;

        public DockerModuleHost(IConfigurationRoot configuration)
        {
            _dockerClient = new DockerClientConfiguration(new Uri(configuration.GetValue<string>("DockerUri")))
                .CreateClient();

        }
    }
}
