using Autofac;
using Castle.DynamicProxy;

namespace TypeEdge.Proxy
{
    public static class ProxyFactory
    {
        public static string IotHubConnectionString { get; set; }
        public static string DeviceId { get; set; }

        public static T GetModuleProxy<T>(string iotHubConnectionString, string deviceId)
            where T : class
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterInstance(
                new ProxyGenerator().CreateInterfaceProxyWithoutTarget<T>(new Proxy<T>(iotHubConnectionString,
                    deviceId)));

            var container = containerBuilder.Build();

            return container.Resolve<T>();
        }

        public static T GetModuleProxy<T>()
            where T : class
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterInstance(
                new ProxyGenerator().CreateInterfaceProxyWithoutTarget<T>(
                    new Proxy<T>(IotHubConnectionString,
                    DeviceId)));

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