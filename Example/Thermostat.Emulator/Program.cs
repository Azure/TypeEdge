using System;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.TypeEdge.Host;
using Microsoft.Extensions.Configuration;
using Modules;
using ThermostatApplication.Modules;
using Microsoft.Azure.IoT.TypeEdge.DovEnv;

namespace ThermostatApplication
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddDotenvFile()
                .AddCommandLine(args)
                .Build();

            var host = new TypeEdgeHost(configuration);

            host.RegisterModule<ITemperatureSensor, TemperatureSensor>();
            host.RegisterModule<IPreprocessor, Preprocessor>();
            host.RegisterModule<IDataSampling, DataSampling>();
            host.RegisterModule<IAnomalyDetection, AnomalyDetection>();

            host.Upstream.Subscribe(host.GetProxy<IAnomalyDetection>().Anomaly);

            host.Build();

            await host.RunAsync();

            Console.WriteLine("Press <ENTER> to exit..");
            Console.ReadLine();
        }
    }
}