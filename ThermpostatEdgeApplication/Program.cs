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
            var edgeApp = new ThermostatApplication("appsettings_thermostat.json");
            await edgeApp.RunAsync();
             
            Console.WriteLine("Press <ENTER> to exit..");
            Console.ReadLine();
        }
    }
}
