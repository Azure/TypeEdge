using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Castle.DynamicProxy;
using Microsoft.Azure.IoT.TypeEdge.Attributes;
using Microsoft.Azure.IoT.TypeEdge.Modules;
using Microsoft.Azure.IoT.TypeEdge.Proxy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Azure.IoT.TypeEdge
{
    public static class Startup
    {
        public static EdgeModule Module { get; set; }

        public static async Task DockerEntryPoint(string[] args)
        {
            var services = new ServiceCollection().AddLogging();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Populate(services);
            containerBuilder.RegisterBuildCallback(c => { });

            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();


            var moduleName = configuration.GetValue<string>(Constants.ModuleNameConfigName);

            //todo: throw or warn here?
            if (string.IsNullOrEmpty(moduleName))
            {
                Console.WriteLine($"WARN:No {Constants.ModuleNameConfigName} in configuration. ");
                Console.WriteLine("Exiting...");
                return;
                //throw new ArgumentException($"No moduleName in arguments");
            }

            var (moduleType, _) = GetModuleTypes(moduleName);

            if (moduleType == null)
                throw new ArgumentException($"No module callled {moduleName} in calling assembly");

            containerBuilder.RegisterType(moduleType);

            var moduleDepedencies = moduleType.GetConstructors().First().GetParameters();
            var moduleDepedencyTypes = moduleDepedencies.Where(i => i.ParameterType.IsInterface &&
                                                                    i.ParameterType.GetCustomAttribute(
                                                                        typeof(TypeModuleAttribute),
                                                                        true) != null).Select(e => e.ParameterType);

            var proxyGenerator = new ProxyGenerator();
            foreach (var depedency in moduleDepedencyTypes)
            {
                containerBuilder.RegisterInstance(
                    proxyGenerator.CreateInterfaceProxyWithoutTarget(depedency, new ModuleProxyBase(depedency)))
                    .As(depedency);
            }

            var container = containerBuilder.Build();

            Module = container.Resolve(moduleType) as EdgeModule;
            if (Module != null)
            {

                Module.BuildSubscriptions();

                Module.InternalConfigure(configuration);
                await Module.InternalRunAsync();
            }


            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            await WhenCancelled(cts.Token);
        }

        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        private static (Type moduleType, Type moduleInterfaceType)
            GetModuleTypes(string moduleName)
        {

            if (GetModule(moduleName, Assembly.GetEntryAssembly(), out var moduleTypes))
                return moduleTypes;

            //try all assemblies now
            return AppDomain.CurrentDomain.GetAssemblies().Any(assembly => GetModule(moduleName, assembly, out moduleTypes)) ? moduleTypes : (null, null);
        }

        private static bool GetModule(string moduleName, Assembly assembly,
            out (Type moduleType, Type moduleInterfaceType) moduleTypes)
        {
            var moduleType = assembly.GetTypes().SingleOrDefault(t =>
                t.GetInterfaces().SingleOrDefault(i =>
                    i.GetCustomAttribute(typeof(TypeModuleAttribute), true) != null &&
                    string.Equals((i.GetCustomAttribute(typeof(TypeModuleAttribute), true) as TypeModuleAttribute)?.Name, moduleName, StringComparison.CurrentCultureIgnoreCase)) != null);

            if (moduleType == null)
            {
                moduleTypes = (null, null);
                return false;
            }

            var moduleInterfaceType = moduleType.GetProxyInterface();
            moduleTypes = (moduleType, moduleInterfaceType);
            return true;
        }

        public static Type GetProxyInterface(this Type type)
        {
            return type.GetInterfaces()
                .SingleOrDefault(i => i.GetCustomAttribute(typeof(TypeModuleAttribute), true) != null);
        }
    }
}