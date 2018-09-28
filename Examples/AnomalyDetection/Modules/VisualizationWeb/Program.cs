using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace VisualizationWeb
{
    public class Program
    {
        // Begin 
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
        }
    }
}