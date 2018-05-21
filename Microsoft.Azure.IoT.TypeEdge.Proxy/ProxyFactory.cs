using Autofac;
using Castle.DynamicProxy;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Azure.IoT.TypeEdge.Proxy
{
    public static class ProxyFactory
    {
        public static string IotHubConnectionString { get; set; }
        public static string DeviceId { get; set; }
        public static T GetModuleProxy<T>(string iotHubConnectionString, string deviceId)
            where T : class
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterInstance(new ProxyGenerator().
                CreateInterfaceProxyWithoutTarget<T>(new ModuleProxy<T>(iotHubConnectionString, deviceId)) as T);

            var container = containerBuilder.Build();

            return container.Resolve<T>();
        }

        public static T GetModuleProxy<T>()
            where T : class
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterInstance(new ProxyGenerator().
                CreateInterfaceProxyWithoutTarget<T>(new ModuleProxy<T>(IotHubConnectionString, DeviceId)) as T);

            var container = containerBuilder.Build();

            return container.Resolve<T>();
        }

        public static void Configure(string iotHubConnectionString, string deviceId)
        {
            IotHubConnectionString = iotHubConnectionString;
            DeviceId = deviceId;
        }
    }
}
