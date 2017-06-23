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
using static Humid.Core;

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
                var get_hello_A = 
                    Get("/a") 
                     | Content("Hello A")
                     | OK ;
                     
                var any_hello_B = 
                    new Route("/b",OK)
                     | Content("Hello B");
                
                var get_del_hello_C = 
                    Path("/c")
                    | Verbs(GET,DELETE)
                    | Content("Hello C")
                    | OK;

                var get_hello_name = 
                    Get("/hello/{name}")
                     | Do(ctx => ctx.With(content: $"Hello {ctx.Params<string>("name", "world").ToUpper()}"))
                     | OK;

                return routes 
                + get_hello_A
                + any_hello_B
                + get_del_hello_C
                + get_hello_name;
                     
           
            });
        }

        private static int counter = 0;
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return services.BuildServiceProvider();
        }
        
    }
}
