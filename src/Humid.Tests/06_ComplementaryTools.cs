using Xunit;
using System;
using NFluent;
using System.Collections.Generic;

using static Humid.Core;
using static Humid.WebActions;
using System.IO;
using NFluent.Mocks;
using Newtonsoft.Json;

namespace Humid.Tests
{
    public class ComplementaryTools 
    {
        [Fact] public void we_could_setup_env_in_server_context()
        {
             var headers = new Dictionary<string,string[]>{
                ["Accept"]= new []{"text/html"}
            };
            var server = new Dictionary<string,string>{
                {"Environment","test"}
            };            

            var testContext = Defaults.Context.With(
                requestHeaders: headers,
                path:"/hello",
                type:GET,
                server:server);
            
            Check.That(testContext.Environment).IsEqualTo("test");
        }

        [Fact] public void we_sould_be_in_prod_env_by_default()
        {
             var headers = new Dictionary<string,string[]>{
                ["Accept"]= new []{"text/html"}
            };
            var server = new Dictionary<string,string>{};            

            var testContext = Defaults.Context.With(
                requestHeaders: headers,
                path:"/hello",
                type:GET,
                server:server);
            
            Check.That(testContext.Environment).IsEqualTo("production");
        }

        [Fact] 
        public void
        it_should_log_stuff_when_env_matches()
        {
            var headers = new Dictionary<string,string[]>{
                ["Accept"]= new []{"application/json"}
            };
            var server = new Dictionary<string,string>{{"env","test"}};            

            var testContext = Defaults.Context.With(
                requestHeaders: headers,
                path:"/hello",
                type:GET,
                server:server);
            string output = null;
            Action<Context> logger = (s) => output = s.Response.Content;

            Route route = Get("/hello") 
                            | Do(ctx => {
                                    return "hello";
                                })
                            | OK
                            | Log(match:"test",logger:logger); 

            Context afterContext = Defaults.Context;
            if(route.Matches(testContext))
                    afterContext = route.ApplyPipeline(testContext); 
                
            Check.That(output).IsEqualTo("hello");
        }

        [Fact] 
        public void
        it_should_NOT_log_stuff_when_env_NOT_matches()
        {
            var headers = new Dictionary<string,string[]>{
                ["Accept"]= new []{"text/html"}
            };
            var server = new Dictionary<string,string>{{"env","test"}};            

            var testContext = Defaults.Context.With(
                requestHeaders: headers,
                path:"/hello",
                type:GET,
                server:server);
            string output = null;
            Action<Context> logger = (s) => output = s.Response.Content;

            Route route = Get("/hello") 
                            | Do(ctx => {
                                    return "hello";
                                })
                            | OK
                            | Log(match:"something",logger:logger); 

            Context afterContext = Defaults.Context;
            if(route.Matches(testContext))
                    afterContext = route.ApplyPipeline(testContext); 
                
            Check.That(output).IsNull();
        }

    }
}