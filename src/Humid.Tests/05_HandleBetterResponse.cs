using Xunit;
using System.Reflection;
using System;
using System.Linq;
using NFluent;
using System.Collections.Generic;

using static Humid.Core;
using static Humid.WebActions;
using static Humid.Helpers;


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

            var testContext = Defaults.Context.With(headers : headers);
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
        // [Fact]
        // public void if_json_helper_is_called_model_should_be_serialized_to_content()
        // {
        //     var testContext = Defaults.Context.With(model : new {name="rui"});

        // }
    }
}