using Xunit;
using System.Reflection;
using System;
using System.Linq;
using NFluent;
using System.Collections.Generic;

using static Humid.Core;
using static Humid.WebActions;
using static Humid.Helpers;
using System.IO;
using NFluent.Mocks;

namespace Humid.Tests
{
    public class HandleBetterResponse
    {
        //actually we have a body response that is directly a string.
        //what we want is to handle an object response and then serialize
        //it as a string in the correct format depending on the accept type
        //provided by the client.
        //that means that we have to change:
        //- add Headers in request
        //- add headers in response
        //- add an object response
        //- change signature of Do webaction to Func<Context,object> and do the 
        //   transformation to string while applying action


        [Fact]
        public void context_should_carry_very_simple_stringheaders_with_comma_separated_values()
        {
            var headers = new[]{ "accept:application/json","accept-language:en-EN, fr"};
            var testContext = Defaults.Context.With(stringHeaders : headers);

            Assert.Equal(2,testContext.Request.Headers.Count());
            Check.That(testContext.Request.Headers).HasSize(2);
            Check.That(testContext.Request.Headers["accept"]).ContainsExactly("application/json");
            Check.That(testContext.Request.Headers["accept-language"]).ContainsExactly("en-EN","fr");
        }

        [Fact]
        public void context_should_carry_regular_owin_headers()
        {
            var headers = new Dictionary<string,string[]>{
                ["accept"]= new []{"application/json","text/html"},
                ["accept-language"] = new []{"en-EN"}
            };

            var testContext = Defaults.Context.With(requestHeaders: headers);
            Assert.Equal(2,testContext.Request.Headers.Count());
            Check.That(testContext.Request.Headers).HasSize(2);
            Check.That(testContext.Request.Headers["accept"])
                .ContainsExactly("application/json","text/html");
            Check.That(testContext.Request.Headers["accept-language"])
                .ContainsExactly("en-EN");
        }

        [Fact]
        public void response_should_have_a_model_object_response_if_needed()
        {
            var testContext = Defaults.Context.With(model : new {name="rui"});

            Check.That(testContext.Response.Model.name as string).IsEqualTo("rui");
        }

        [Fact]
        public void ensure_do_helper_can_accept_any_return_value_as_model()
        {
            var testContext = Defaults.Context.With(
                path:"/hello/world/42",
                type:GET);
            
            Route route = Get("/hello/{name}/{id}") 
                            | Do(ctx => {
                                    var name = ctx.Params<string>("name","hell");
                                    var id = ctx.Params<int>("id",0);
                                    
                                    return new {name,id};
                                })
                            | OK;

            Context afterContext = Defaults.Context;

            if(route.Matches(testContext))
                afterContext = route.ApplyPipeline(testContext); 
            
            Assert.Equal("world",afterContext.Response.Model.name);
            Assert.Equal(42,afterContext.Response.Model.id);
            Assert.Equal(200,afterContext.Response.StatusCode);
        }
        [Fact]
        //public void by_default_if_accept_is_json_and_model_exists_we_serialize_it_to_content()
        public void json_action_with_no_param_serialize_model_to_content_if_json_headers()
        {
            var headers = new Dictionary<string,string[]>{
                ["accept"]= new []{"application/json"}
            };

            var testContext = Defaults.Context.With(
                requestHeaders: headers,
                path:"/hello/world/42",
                type:GET);
            
            Route route = Get("/hello/{name}/{id}") 
                            | Do(ctx => {
                                    var name = ctx.Params<string>("name","hell");
                                    var id = ctx.Params<int>("id",0);
                                    return new {name,id};
                                })
                            | JSON
                            | OK;

            Context afterContext = Defaults.Context;

            if(route.Matches(testContext))
                afterContext = route.ApplyPipeline(testContext); 
            
            Assert.Equal("{\"name\":\"world\",\"id\":42}",afterContext.Response.Content);
            Assert.Equal(200,afterContext.Response.StatusCode);
            Assert.Equal("application/json",afterContext.Response.Headers["content-type"][0]);
        }

        [Fact]
        public void json_action_can_also_adapt_model_before_serialization()
        {
            var headers = new Dictionary<string,string[]>{
                ["accept"]= new []{"application/json"}
            };

            var testContext = Defaults.Context.With(
                requestHeaders: headers,
                path:"/hello/world/42",
                type:GET);
        
            Route route = Get("/hello/{name}/{id}") 
                            | Do(ctx => {
                                    var name = ctx.Params<string>("name","hell");
                                    var id = ctx.Params<int>("id",0);
                                    return new {name,id};
                                })
                            | Json(model=> new {
                                    name=model.name.ToUpper(),
                                    model.id,
                                    foo="bar"})
                            | OK;

            Context afterContext = Defaults.Context;

            if(route.Matches(testContext))
                afterContext = route.ApplyPipeline(testContext); 
            
            Assert.Equal("{\"name\":\"WORLD\",\"id\":42,\"foo\":\"bar\"}",afterContext.Response.Content);
            Assert.Equal(200,afterContext.Response.StatusCode);
            
        }

        [Fact]
        public void json_action_has_no_effect_if_accept_not_right_type()
        {
            var headers = new Dictionary<string,string[]>{
                ["accept"]= new []{"text/html"}
            };

            
            var testContext = Defaults.Context.With(
                requestHeaders: headers,
                path:"/hello/world/42",
                type:GET);
            
            Route route = Get("/hello/{name}/{id}") 
                            | Do(ctx => {
                                    var name = ctx.Params<string>("name","hell");
                                    var id = ctx.Params<int>("id",0);
                                    return new {name,id};
                                })
                            | Json(model=> new {
                                    name=model.name.ToUpper(),
                                    model.id,
                                    foo="bar"})
                            | OK;

            Context afterContext = Defaults.Context;

            if(route.Matches(testContext))
                afterContext = route.ApplyPipeline(testContext); 
            
            Assert.Equal("{ name = world, id = 42 }",afterContext.Response.Content);
            Assert.Equal(200,afterContext.Response.StatusCode);

        }

        //use new HeadersDictionary instead of IDictionary<string,string> for headers
        //in request and response

        //introduce html view with 1/micro templating 2/addon with dotliquid (in ext proj)
        //introduce async?
        //introduce railway?
        //remove aspnet dependencies in humid core, create a real humid.owin lib and real
        //demo project.
        
        [Fact]
        public void context_should_be_filled_with_host_information()
        {
            var currentAppPath = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            var server = new Dictionary<string,string>{
                ["Site:PhysicalFullPath"]=currentAppPath
            };  
            var headers = new Dictionary<string,string[]>{
                ["accept"]= new []{"text/html"}
            };
            var testContext = Defaults.Context.With(
                requestHeaders: headers,
                path:"/hello/world/42",
                type:GET,
                server:server);

            Assert.Equal(currentAppPath,testContext.Server["Site:PhysicalFullPath"]);
        }

        [Fact]
        public void templates_use_an_abstraction()
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
            var sampleObject = new {id=42};
            ITemplateEngine engine = new SimpleTemplateEngine();
            string renderTemplate = engine.RenderTemplate(testContext,"templateName");
            string renderTemplateWithModel = engine.RenderTemplate(testContext,"templateName",sampleObject);
        }

        [Fact]
        public void html_action_can_return_html_with_a_template_name()
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
                            | OK | Log();

            Context afterContext = Defaults.Context;

            if(route.Matches(testContext))
                afterContext = route.ApplyPipeline(testContext); 
            
            Check.That(afterContext.Response.Content).IsEqualTo(@"<h1 id=""42"">Hello <b>World</b></h1>");
            Check.That(afterContext.Response.StatusCode).IsEqualTo(200);
            var contentType = afterContext.Response.Headers.FirstOrDefault(x => x.Key == "content-type");
            Check.That(contentType).IsNotEqualTo(default(KeyValuePair<string,string[]>));
            Check.That(contentType.Value).Contains("text/html");
        }

        [Fact] public void
        it_should_print_context_in_console_when_needed()
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
                            | OK
                            | Log(production:false);

            Context afterContext = Defaults.Context;

            using (var console = new CaptureConsole())
            {
                if(route.Matches(testContext))
                    afterContext = route.ApplyPipeline(testContext); 
                
                Check.That(console.Output).IsEqualTo(@"{
  ""Request"": {
    ""Type"": 0,
    ""Path"": ""/hello/World/42"",
    ""QueryString"": """",
    ""Query"": {},
    ""RouteParams"": {
      ""name"": ""World"",
      ""id"": ""42""
    },
    ""Headers"": {
      ""Accept"": [
        ""text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8""
      ]
    }
  },
  ""Response"": {
    ""StatusCode"": 200,
    ""Content"": ""<h1 id=\""42\"">Hello <b>World</b></h1>"",
    ""Model"": {
      ""name"": ""World"",
      ""id"": 42
    },
    ""Headers"": {
      ""content-type"": [
        ""text/html""
      ]
    }
  },
  ""Server"": {
    ""Site:PhysicalFullPath"": ""/Volumes/Work/code/Rui/humid/src/Humid.Tests""
  }
}
");

            }
            
            

        }
    }
}