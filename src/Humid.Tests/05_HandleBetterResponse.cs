using Xunit;
using System.Reflection;
using System;
using System.Linq;
using NFluent;

using static Humid.Core;
using System.Collections.Generic;

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
    }
}