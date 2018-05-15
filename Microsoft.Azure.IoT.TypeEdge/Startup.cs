using Autofac;
using Autofac.Extensions.DependencyInjection;
using Castle.DynamicProxy;
using Microsoft.Azure.IoT.TypeEdge.Attributes;
using Microsoft.Azure.IoT.TypeEdge.Host;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.TypeEdge
{
    public static class Startup
    {
        public static async Task Main(string[] args)
        {
            var services = new ServiceCollection().AddLogging();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Populate(services);
            containerBuilder.RegisterBuildCallback(c => { });

            var configuration = new ConfigurationBuilder()
               .AddEnvironmentVariables()
               .AddCommandLine(args)
               .Build();


            var moduleName = configuration.GetValue<string>("moduleName");

            if (string.IsNullOrEmpty(moduleName))
                throw new ArgumentException($"No moduleName in arguments");

            var (moduleType, moduleInterface) = GetModuleTypes(moduleName);

            if (moduleType == null)
                throw new ArgumentException($"No module callled {moduleName} in calling assembly");

            containerBuilder.RegisterType(moduleType);

            var moduleDepedencies = moduleType.GetConstructors().First().GetParameters();
            var moduleDepedencyTypes = moduleDepedencies.Where(i => i.ParameterType.IsInterface &&
                i.GetCustomAttribute(typeof(TypeModuleAttribute), true) != null).Select(e => e.ParameterType);

            var proxyGenerator = new ProxyGenerator();
            moduleDepedencyTypes.Select(e =>
           containerBuilder.RegisterInstance(
               proxyGenerator.CreateInterfaceProxyWithoutTarget(e, new ModuleProxyBase(e))));

            var container = containerBuilder.Build();

            var module = container.Resolve(moduleType) as EdgeModule;
            module.InternalConfigure(configuration);
            await module.InternalRunAsync();

        }

        private static (Type moduleType, Type moduleInterfaceType) GetModuleTypes(string moduleName)
        {
            var moduleType = Assembly.GetCallingAssembly().GetTypes().SingleOrDefault(t => t.GetInterfaces().SingleOrDefault(i => i.GetCustomAttribute(typeof(TypeModuleAttribute), true) != null && (i.GetCustomAttribute(typeof(TypeModuleAttribute), true) as TypeModuleAttribute).Name == moduleName) != null);

            if (moduleType != null)
            {
                var moduleInterfaceType = moduleType.GetProxyInterface();
                return (moduleType, moduleInterfaceType);
            }
            return (null, null);
        }

        public static Type GetProxyInterface(this Type type)
        {
            return type.GetInterfaces().SingleOrDefault(i => i.GetCustomAttribute(typeof(TypeModuleAttribute), true) != null);
        }

    }
}
