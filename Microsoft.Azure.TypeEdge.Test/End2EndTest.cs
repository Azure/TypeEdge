using System;
using System.Threading;
using Microsoft.Azure.Devices.Edge.Agent.Docker;
using Microsoft.Azure.Devices.Edge.Util;
using Microsoft.Azure.TypeEdge.Host;
using Microsoft.Azure.TypeEdge.Proxy;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Microsoft.Azure.TypeEdge.Test
{
    public class End2EndTest
    {
        [Fact]
        public void TestSingleModule()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json")
                .AddEnvironmentVariables()
                .Build();

            var host = new TypeEdgeHost(configuration);

            //register the modules
            host.RegisterModule<ITestModule, TestModule>();
            host.Upstream.Subscribe(host.GetProxy<ITestModule>().Output);

            //customize the runtime configuration
            var dockerRegistry = configuration.GetValue<string>("DOCKER_REGISTRY") ?? "";
            var manifest = host.GenerateDeviceManifest((e, settings) =>
            {
                //this is the opportunity for the host to change the hosting settings of the module e
                if (!settings.IsExternalModule)
                    settings.Config = new DockerConfig($"{dockerRegistry}{e}:1.0", settings.Config.CreateOptions);
                return settings;
            });

            //provision a new device with the new manifest
            var sasToken = host.ProvisionDevice(manifest);

            //build an emulated device in memory
            host.BuildEmulatedDevice(sasToken);

            //run the emulated device
            var task = host.RunAsync();
            Console.WriteLine("Waiting for 20 seconds...");
            Thread.Sleep(20 * 1000);

            ProxyFactory.Configure(configuration["IotHubConnectionString"],
                configuration["DeviceId"]);

            var testModule = ProxyFactory.GetModuleProxy<ITestModule>();
            var offset = new Random().Next(0, 1000);
            var lastTwin = testModule.Twin.GetAsync().Result;
            lastTwin.Offset = offset;
            var res = testModule.Twin.PublishAsync(lastTwin).Result;
            var res2 = testModule.TestDirectMethod(10);

            Assert.Equal(res.Offset, offset);
            Assert.Equal(10, res2);
        }
    }
}
