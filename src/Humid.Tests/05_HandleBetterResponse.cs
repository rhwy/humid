namespace Humid.Tests
{
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


    public class HandleBetterResponse
    {
        [Fact] public void 
        context_should_carry_very_simple_stringheaders_with_comma_separated_values()
        {
            var headers = new[]{ "accept:application/json","accept-language:en-EN, fr"};
            var testContext = Defaults.Context.With(stringHeaders : headers);

            Check.That(testContext.Request.Headers).HasSize(2);
            Check.That(testContext.Request.Headers["accept"]).ContainsExactly("application/json");
            Check.That(testContext.Request.Headers["accept-language"]).ContainsExactly("en-EN","fr");
        }

        [Fact] public void 
        context_should_carry_regular_owin_headers()
        {
            var headers = new Dictionary<string,string[]>{
                ["accept"]= new []{"application/json","text/html"},
                ["accept-language"] = new []{"en-EN"}
            };

            var testContext = Defaults.Context.With(requestHeaders: headers);
            Check.That(testContext.Request.Headers).HasSize(2);
            Check.That(testContext.Request.Headers["accept"])
                .ContainsExactly("application/json","text/html");
            Check.That(testContext.Request.Headers["accept-language"])
                .ContainsExactly("en-EN");
        }

        [Fact] public void 
        response_should_have_a_model_object_response_if_needed()
        {
            var testContext = Defaults.Context.With(model : new {name="rui"});
            Check.That(testContext.Response.Model.name as string).IsEqualTo("rui");
        }

        [Fact] public void 
        ensure_do_helper_can_accept_any_return_value_as_model()
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

            Check.That((string)afterContext.Response.Model.name).IsEqualTo("world");
            Check.That((int)afterContext.Response.Model.id).IsEqualTo(42);
            Check.That((int)afterContext.Response.StatusCode).IsEqualTo(200);
        }

        [Fact] public void 
        json_action_with_no_param_serialize_model_to_content_if_json_headers()
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
            
            Check.That(afterContext.Response.Content).IsEqualTo("{\"name\":\"world\",\"id\":42}");
            Check.That(afterContext.Response.StatusCode).IsEqualTo(200);
            Check.That(afterContext.Response.Headers["content-type"][0]).IsEqualTo("application/json");
        }

        [Fact] public void 
        json_action_can_also_adapt_model_before_serialization()
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
            
            Check.That(afterContext.Response.Content).IsEqualTo("{\"name\":\"WORLD\",\"id\":42,\"foo\":\"bar\"}");
            Check.That(afterContext.Response.StatusCode).IsEqualTo(200);
        }

        [Fact] public void 
        json_action_has_no_effect_if_accept_not_right_type()
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
            
            Check.That(afterContext.Response.Content).IsEqualTo("{ name = world, id = 42 }");
            Check.That(afterContext.Response.StatusCode).IsEqualTo(200);
        }

        
        [Fact] public void 
        context_should_be_filled_with_host_information()
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

            Check.That(testContext.Server["Site:PhysicalFullPath"]).IsEqualTo(currentAppPath);
        }

        
    }
}