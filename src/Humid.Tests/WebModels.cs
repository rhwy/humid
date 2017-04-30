namespace Humid.Tests
{
    using System;
    using Xunit;
    using FunctionalHelpers;
    using static FunctionalHelpers.Core;
    using static  Humid.Core;

    public class TypesTests
    {
        // First we need to define methods for the type of the request
        [Fact] public void 
        Requests_Have_Type_and_can_be_created_by_helpers()
        {
            var get = RequestType.GET;
            Assert.IsAssignableFrom<RequestType>(get);
            
            Assert.Equal(RequestType.GET, GET);
            Assert.Equal(RequestType.POST, POST);
            Assert.Equal(RequestType.DELETE, DELETE);
            Assert.Equal(RequestType.HEAD, HEAD);
            Assert.Equal(RequestType.UNKNOWN, DEFAULT_REQUEST_TYPE);
            
        }

        //A request is composed of a route and a type:
        [Fact] public void 
        Basic_request_has_a_type_and_route_and_construct_by_helper()
        {
            var route = "***";
            var type = RequestType.UNKNOWN;
            var req = Request(type,route);

            Assert.Equal("***",req.Route);
        }


        [Fact] //a response is some content associated with a status code
        public void Response_has_content_code()
        {
            var content = "hello";
            var code = 200;
            var response = Response(content,code);
            

            Assert.Equal(200, response.StatusCode);
            Assert.Equal(content,response.Content);
        }
/* 
        [Fact] //we'll need to manipulate response along with the request, that
         // means that we'll need a context to play with both
        public void Context_is_request_restponse()
        {
            var req = new Request{ Route="route",Type = RequestType.UNKNOWN};
            var response = new Response{Content = "content",StatusCode = 0};

            var context = new Context { Request = req, Response = response };
            context.Response.StatusCode = 200;
            Assert.Equal(200, context.Response.StatusCode);
        }

        [Fact] //finally, a web request, is defined as the transformation of
        // a context! that's it ;-), let's call it a WebAction
        // public void WebAction_is_context_transform()
        // {
        //     var req = new Request{ Route="hello",Type = RequestType.UNKNOWN};
        //     var response = new Response{Content = "content",StatusCode = 0};
        //     var beforeContext = new Context { Request = req, Response = response };

        //     WebAction action = (ctx) => new Some<Context>{
        //             Value = new Context {
        //                 Request = ctx.Request,
        //                 Response = new Response{ Content = "hello", StatusCode = 200}
        //             }} ;

        //     var afterContext = action(beforeContext);

        //     var result = "";
        //     switch (afterContext)
        //     {
        //         case None<Context> none:
        //             result = "not found";
        //             break;
        //         case Some<Context> some when some.Value.Response.StatusCode == 200:
        //             result = some.Value.Response.Content;
        //             break;
        //         default:
        //             result = "error";
        //             break;
        //     }
        //     Assert.Equal("hello",result);
        //}
        public void WebAction_is_context_transform()
        {
            var req = new Request{ Route="hello",Type = RequestType.UNKNOWN};
            var response = new Response{Content = "content",StatusCode = 0};
            var beforeContext = new Context { Request = req, Response = response };
            
            WebAction action = (ctx) => Some(
                    new Context {
                        Request = ctx.Request,
                        Response = new Response{ Content = "hello", StatusCode = 200}
                    }) ;

            var afterContext = action(beforeContext);

            
            var result = "";

            Assert.Equal("hello",result);

        }
*/

    }
}
