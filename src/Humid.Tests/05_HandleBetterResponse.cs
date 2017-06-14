using Xunit;
using System.Reflection;
using System;
using System.Linq;
using static Humid.Core;

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
        public void context_should_carry_very_simple_headers()
        {
            var headers = new[]{ "accept:application/json","accept-language:en-EN"};
            var testContext = Defaults.Context.With(headers : headers);

            Assert.Equal(2,testContext.Request.Headers.Count());
            Assert.Equal("application/json",testContext.Request.Headers["accept"]);
            Assert.Equal("en-EN",testContext.Request.Headers["accept-language"]);
        }

    }
}