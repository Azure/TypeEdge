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
        public static T GetModuleProxy<T>(string connectionString, string deviceId)
            where T : class
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterInstance(new ProxyGenerator().
                CreateInterfaceProxyWithoutTarget<T>(new ModuleProxy<T>(connectionString, deviceId)) as T);

            var container = containerBuilder.Build();

            return container.Resolve<T>();
        }
    }
}
