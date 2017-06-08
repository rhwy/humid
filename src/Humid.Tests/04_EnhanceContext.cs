namespace Humid.Tests
{
    using Xunit;
    using Xunit.Sdk;
    using static Humid.Core;
    //using static FunctionalHelpers.Core;
    using static Humid.WebActions;
    using static Humid.Helpers;

    using System.Linq;

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
            string actual = TokensToRegex(inputPath);
            Assert.Equal(expectedRegex,actual);
        }

        //now we have tokens->regex, we need a matcher that will use the rex
        //and apply it to the input to provide in output a key/value list.
        [Theory]
        [InlineData("/hello/(?<name>[a-zA-Z0-9-_]*)","/hello/world","name","world")]
        public void extract_values_from_rex_to_dict(string rex, string path, string key, string value)
        {
            var kv = ExtractTokensValuesFromPath(rex,path);
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

    }
}