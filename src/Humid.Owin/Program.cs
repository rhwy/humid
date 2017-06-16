using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using static Humid.WebActions;

namespace Humid.Owin
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();
            Console.WriteLine("-------- Ready --------------");
            host.Run();
        }
    }

    public class Startup : IStartup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseHumid(routes => {
                return routes 
                + ( Get("/a") 
                     | OK 
                     | Content("I'm A"))
                + ( Get("/b") 
                     | OK 
                     | Content("I'm B"));
            });
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return services.BuildServiceProvider();
        }
        
    }
}
