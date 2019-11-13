using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Humid.Middleware;
using static Humid.Core;
using static Humid.WebActions;
using System.IO;
using Fluid;
using Humid.DotLiquid;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace Humid.SampleForMinimalAspnetcore
{
    class Program
    {
        public static void Main(string[] args)
        {
            //new DataSource().Open();

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext,config) =>
                {
                    var host = hostingContext.HostingEnvironment;
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", false, true);
                    config.AddJsonFile($"appsettings.{host.EnvironmentName}.json", true, true);
                    config.AddCommandLine(args);
                    config.AddEnvironmentVariables();
                })
                .UseContentRoot("templates").UseWebRoot("wwwroot")
                .UseStartup<Startup>();
    }

    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddSingleton<ITemplateEngine>(new FluidTemplateEngine());
            services.AddSingleton<ITemplateEngine>(new LiquidTemplateEngine());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();
 
            app.UseHumid(
                env,routes => {
                var get_hello_A = 
                    Get("/a") 
                     | Content("Hello A")
                     | OK ;
                     
                var any_hello_B = 
                    new Route("/b",OK)
                     | Content("Hello B");
                
                var delete_only_hello_C = 
                    Path("/c")
                    | Verbs(DELETE)
                    | Content("Hello C") 
                    | OK;

                var get_hello_name = 
                    Get("/hello/{name}")
                     | Do(ctx => ctx.With(content: $"Hello {ctx.Params<string>("name", "world").ToUpper()}"))
                     | OK;
                
                var get_hello_object = 
                    Get("/hello/world")
                     | Do(ctx => {
                            var md = new HeyRed.MarkdownSharp.Markdown();
                            var html = md.Transform("<h2>Hello,</h2><p>**uncatched route**</p>");

                            return new{ message = html,value=42}; })
                     | OK;

                var get_hello_view = 
                    Get("/niceHello/{name}") 
                            | Do(ctx => { 
                                    int id = ctx.Query<int>("id",-1);
                                    var name = ctx.Params<string>("name","world");
                                    return new {name,id};
                                })
                            | Html("simpleHello")
                            | OK
                            | Log("production");

                var get_hello_liquid = 
                    Get("/liquid/hello")
                        | Do(c => new { name = new { first = c.Query<string>("name","world")}})
                        | Html("liquid/simple")
                        | OK
                        | Log(
                            match:"Production",
                            logger:c=>Console.WriteLine($"match : {c.Request.TypeName} {c.Request.Path}"));
                
                //how to group routes by functionality, ex:
//                var groupedRoutesWithViews = 
//                    f<Router>(r => r
//                                    - get_hello_view
//                                    - get_hello_liquid);
//                
                return routes 
                       - get_hello_A
                       - any_hello_B
                       - delete_only_hello_C
                       - get_hello_object
                       - get_hello_name
                       //- groupedRoutesWithViews
                ;
                     
           
            });

            app.Run(async (context) =>
            {
                if(context.Response.StatusCode == 400)
                {
                    await context.Response.WriteAsync("your query doesn't match any route");
                }
                
            });
            
        }

        
    }

    public class FluidTemplateEngine : ITemplateEngine
    {
        public string RenderTemplate(Context context, string name, object model)
        {
            var source = RenderTemplate(context,name);

            if (FluidTemplate.TryParse(source, out var template))
            {   
                var templateContext = new TemplateContext();
                templateContext.MemberAccessStrategy.Register(model.GetType()); // Allows any public property of the model to be used
                templateContext.SetValue("p", model);

                return template.Render(templateContext);
            }
            return null;
        }

        public string RenderTemplate(Context context, string name)
        {
            var rootPath = context.Server["Site:PhysicalFullPath"];
            var templateRelativePath = System.IO.Path.Combine("templates",name + ".html");
            var templatePath = System.IO.Path.Combine(rootPath,templateRelativePath);
            if(File.Exists(templatePath))
                return File.ReadAllText(templatePath);
            
            return null;
        }
    }

    public class DataSource
    {
        public void Open()
        {
            var connection = new SqliteConnection("Data Source=HumidSample.db");
            connection.Open();
            var recreateCommand = connection.CreateCommand();
            recreateCommand.CommandText = @"
        DROP TABLE Properties;
        DROP TABLE Contact;
";
            recreateCommand.ExecuteNonQuery();
            //var init = "CREATE TABLE IF NOT EXISTS some_table (id INTEGER PRIMARY KEY AUTOINCREMENT, content TEXT);";
            var createCommand = connection.CreateCommand();
            createCommand.CommandText =
            @"
                CREATE TABLE IF NOT EXISTS Contact (
                    id INTEGER,
                    label TEXT
                );
                CREATE TABLE IF NOT EXISTS Properties (
                    id INTEGER,
                    label TEXT,
                    content TEXT,
                    contact_id INTEGER
                );
                INSERT OR IGNORE INTO Contact
                VALUES (1, 'Rui') 
                ;
                INSERT OR IGNORE INTO Properties
                VALUES (1, 'Email', 'play@rui.fr',1),
                       (2, 'LastName', 'Carvalho',1);
            ";
            //,FOREIGN KEY (contact_id) REFERENCES Contact
            createCommand.ExecuteNonQuery();
            var selectCommand  = connection.CreateCommand();
            selectCommand.CommandText = 
                "SELECT * FROM Properties";
            using (var reader = selectCommand.ExecuteReader())
            {
                while(reader.Read())
                {
                    
                    var ds = reader.GetSchemaTable();
                    ds.Load(reader);
                    //var x = ds.GetXml();
                    Console.WriteLine(ds.DataSet == null);
                    //Console.WriteLine($"{values[0]}. {values[3]} : {values[4]}");
                    //Console.WriteLine($"{reader.GetValue(0)}. {reader.GetValue(3)} : {reader.GetValue(4)}");
                }
            }
            connection.Close();
        }
    }
}
