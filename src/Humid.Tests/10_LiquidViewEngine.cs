namespace Humid.Tests
{
    using Xunit;
    using System;
    using System.Linq;
    using NFluent;
    using System.Collections.Generic;

    using static Humid.Core;
    using static Humid.WebActions;
    using Humid.DotLiquid;

    using System.IO;

    public class Using_DoLiquid_as_ViewEngine
    {
        [Fact] public void 
        templates_use_an_abstraction()
        {
            var server = new Dictionary<string,string>{
                ["Site:PhysicalFullPath"]=new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName
            };            

            var headers = new Dictionary<string,string[]>{
                ["accept"]= new []{"text/html"}
            };
            var testContext = Defaults.Context.With(
                requestHeaders: headers,
                path:"/hello/world/42",
                type:GET,
                server:server);
                
            var sampleObject = new {name="world"};

            ITemplateEngine engine = new LiquidTemplateEngine();
            Check.ThatCode(() => {
                string renderTemplate = engine.RenderTemplate(testContext,"simpleLiquid");
                string renderTemplateWithModel = engine.RenderTemplate(testContext,"simpleliquid",sampleObject);
                Check.That(renderTemplateWithModel).IsEqualTo("hello world");
            }).DoesNotThrow();


        }

        [Fact] public void 
        html_action_can_return_html_with_a_template_name()
        {
            var headers = new Dictionary<string,string[]>{
                ["Accept"]= new []{"text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"}
            };

            var server = new Dictionary<string,string>{
                ["Site:PhysicalFullPath"]=new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName
            };            

            var testContext = Defaults.Context.With(
                requestHeaders: headers,
                path:"/hello/World/42",
                type:GET,
                server:server);
            
            Route route = Get("/hello/{name}/{id}") 
                            | Do(ctx => {
                                    var name = ctx.Params<string>("name","hell");
                                    var id = ctx.Params<int>("id",0);
                                    return new {name,id};
                                })
                            | Html("simpleH1")
                            | OK ;

            Context afterContext = Defaults.Context;

            if(route.Matches(testContext))
                afterContext = route.ApplyPipeline(testContext); 
            
            Check.That(afterContext.Response.Content).IsEqualTo(@"<h1 id=""42"">Hello <b>World</b></h1>");
            Check.That(afterContext.Response.StatusCode).IsEqualTo(200);

            var contentType = afterContext.Response.Headers.FirstOrDefault(x => x.Key == "Content-Type");
            Check.That(contentType).IsNotEqualTo(default(KeyValuePair<string,string[]>));
            Check.That(contentType.Value).Contains("text/html;charset=utf-8");
        }
    }
}