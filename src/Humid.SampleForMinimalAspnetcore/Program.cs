using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Humid.Middleware;
using static Humid.Core;
using static Humid.WebActions;

namespace Humid.SampleForMinimalAspnetcore
{
    class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }

    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
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
                
                var get_hello_object = 
                    Get("/hello/world")
                     | Do(ctx => new{ message = "Hello world",value=42} )
                     | OK;
                var get_hello_view = 
                    Get("/niceHello/{name}") 
                            | Do(ctx => {
                                    var name = ctx.Params<string>("name","world");
                                    return new {name};
                                })
                            | Html("simpleHello")
                            | OK
                            | Log(true);


                return routes 
                        + get_hello_A
                        + any_hello_B
                        + get_del_hello_C
                        + get_hello_object
                        + get_hello_name
                        + get_hello_view
                ;
                     
           
            });

            app.Run(async (context) =>
            {
                if(context.Response.StatusCode == 400)
                {
                    await context.Response.WriteAsync("Hello from uncatched route!");
                }
                
            });
            
        }

        
    }
}
