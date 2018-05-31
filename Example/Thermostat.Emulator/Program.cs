using System;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.TypeEdge.Host;
using Microsoft.Extensions.Configuration;
using Modules;
using ThermostatApplication.Modules;
using Microsoft.Azure.IoT.TypeEdge.Host.DovEnv;

namespace ThermostatApplication
{ 
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings_thermostat.json")
                .AddEnvironmentVariables()
                .AddDotenvFile()
                .AddCommandLine(args)
                .Build();
             
            var host = new TypeEdgeHost(configuration);

            host.RegisterModule<ITemperatureModule, TemperatureModule>();
            host.RegisterModule<INormalizeTemperatureModule, NormalizeTemperatureModule>(); 

            host.Upstream.Subscribe(host.GetProxy<INormalizeTemperatureModule>().NormalizedTemperature);

            host.Build();

            await host.RunAsync();

            Console.WriteLine("Press <ENTER> to exit..");
            Console.ReadLine();
        }
    }
}