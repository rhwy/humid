namespace Humid.Tests
{
    using Xunit;
    using Xunit.Sdk;
    using static Humid.Core;
    //using static FunctionalHelpers.Core;
    using static Humid.WebActions;
    using static Humid.Helpers;

    using System.Linq;
    using System.Collections.Generic;

    ///<summary>
    ///Now we have the basics parts of our web lib but to make it really usable, we need:
    ///- tokenized paths : get the params from the path and pass it into context as a (string,string) dictionary
    ///- capture querystrings : capture querystring and pass it into context as a (string,string) dictionary
    ///- get the string body of what's sent (in post) to the server as a Body property of the context
    ///</summary>     
    public class EnhanceContext
    {
        
        //for simpler use, we will use tokenised strings in our routes
        //but behind the scenes we want to use regex. the idea is to create
        //a regex named group with the name of the token that captures any 
        // (char,number,-,_) at it's place, ex:
        //"/hello/{name}" -> "/hello/(?<name>[a-zA-Z0-9-_]*)"
        [Theory]
        [InlineData("/simple/path","/simple/path")]
        [InlineData("/hello/{name}","/hello/(?<name>[a-zA-Z0-9-_]*)")]
        [InlineData("/hello/{id}","/hello/(?<id>[a-zA-Z0-9-_]*)")]
        [InlineData("/","/")]
        [InlineData("/hello/{id}/bla","/hello/(?<id>[a-zA-Z0-9-_]*)/bla")]
        public void tokenizer_helper_can_transform_stringWithTokens_in_regex_string(string inputPath, string expectedRegex)
        {
            string actual = TransformRouteExpression(inputPath);
            Assert.Equal(expectedRegex,actual);
        }

        //now we have tokens->regex, we need a matcher that will use the rex
        //and apply it to the input to provide in output a key/value list.
        [Theory]
        [InlineData("/hello/(?<name>[a-zA-Z0-9-_]*)","/hello/world","name","world")]
        public void extract_values_from_rex_to_dict(string rex, string path, string key, string value)
        {
            var kv = ExtractTokensValuesFromExpression(rex,path);
            Assert.Equal(kv.Keys.FirstOrDefault(), "name");
            Assert.Equal(kv.Values.FirstOrDefault(), "world");
        }

        //we want to have more that just a string path to define routes,
        //we want them to be tokenised
        [Theory]
        [InlineData("/t/{name}","/t/rui",true)]
        [InlineData("/t/{name}","/t/Jimmy",true)]
        [InlineData("/t/{id}","/t/xx/toto",false)]
        public void
        path_can_have_tokens(string template, string path,bool expectedMatch)
        {
            var route = Path(template) ;
            var testContext = Defaults.Context.With(path:path);
            //implement route template with other tests
            var isMatch = route.Matches(testContext);
            Assert.Equal(expectedMatch,isMatch);
            //
        }

        //now that we have a tokenized matcher, we need to add this extracted data
        //to the context request as RouteParams property. The structure should
        //internally store that data as a Dictionary<string,string> but as users, 
        //we'll prefer to have an helper method on context to extract values safely
        //and possibelly with a cast:
        [Fact]
        public void can_extract_route_params_values_safely_with_cast()
        {
            var routeParams = new Dictionary<string,string>{
                ["name"] = "paul",
                ["id"] = "42"
            };
            var testContext = Defaults.Context.With(routeParams : routeParams );
            var name = testContext.Params<string>("name");
            var id = testContext.Params<int>("id");
            var missingValue = testContext.Params<string>("missing");
            var missingValueWithFallback = testContext.Params<int>("value",123);

            Assert.Equal("paul",name);
            Assert.Equal(42,id);
            Assert.Equal(null,missingValue);
            Assert.Equal(123,missingValueWithFallback);
        }

        //it should be interesting now to put all the things together and use these
        //route parameters for doing a real thing within a route
        [Fact]
        public void can_consume_routeparams_within_a_webaction()
        {
            var testContext = Defaults.Context.With(
                path:"/hello/world/42",
                type:GET);
            
            Route route = Get("/hello/{name}/{id}") 
                            | Do(ctx => {
                                    var name = ctx.Params<string>("name","hell");
                                    return ctx.With(content: $"hello {name}");
                                })
                            | OK;

            string content = null;
            int status = -1;

            if(route.Matches(testContext))
                (content,status) = route.ApplyPipeline(testContext); 
            
            Assert.Equal("hello world",content);
            Assert.Equal(200,status);

        }

        //just after route params, handling querystrings seems to be a next logical path
        [Fact]
        public void can_parse_queryStrings()
        {
            string queryString = "id=42&name=marvin";
            var result = ParseQueryString(queryString);
            Assert.Equal(2, result.Count());
            Assert.Equal("marvin",result["name"]);
            Assert.Equal("42",result["id"]);
        }

        [Fact]
        public void can_consume_querystring_within_a_webaction()
        {
            var testContext = Defaults.Context.With(
                path:"/hello",
                query:"id=42&name=marvin",
                type:GET);

            Assert.Equal("id=42&name=marvin", testContext.Request.QueryString);

            Route route = Get("/hello") 
                            | Do(ctx => {
                                    var name = ctx.Query<string>("name","hell");
                                    var value = ctx.Query<int>("id",0);
                                    return ctx.With(content: $"hello {name}, the answer is {value}");
                                })
                            | OK;

            string content = null;
            int status = -1;

            if(route.Matches(testContext))
                (content,status) = route.ApplyPipeline(testContext); 
            
            Assert.Equal("hello marvin, the answer is 42",content);
            Assert.Equal(200,status);
        }
    }
}