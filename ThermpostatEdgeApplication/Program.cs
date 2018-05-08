using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ThermpostatEdgeApplication
{
    class Program
    {
        static async Task Main(string[] args) 
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings_thermostat.json")
                .AddEnvironmentVariables()
                .Build();

            var edgeApp = new ThermostatApplication(configuration);

            await edgeApp.RunAsync();
             
            Console.WriteLine("Press <ENTER> to exit..");
            Console.ReadLine();
        }
    }
}
