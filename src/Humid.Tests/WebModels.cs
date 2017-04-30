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
            Assert.Equal(RequestType.UNKNOWN,req.Type);
        }


        //a response is some content associated with a status code
        [Fact] public void 
        Response_has_content_code()
        {
            var content = "hello";
            var code = 200;
            var response = Response(content,code);
            

            Assert.Equal(200, response.StatusCode);
            Assert.Equal(content,response.Content);
        }

        //we'll need to manipulate response along with the request, that
        // means that we'll need a context to play with both
        [Fact] public void 
        Context_is_request_and_response()
        {
            var req = Request(RequestType.UNKNOWN,"/route");
            var resp = Response("hello",123);

            var context = Context(req,resp);
            Assert.Equal(123, context.Response.StatusCode);
        }

 
        //finally, a web request, is defined as the transformation of
        // a context! that's it ;-), let's call it a WebAction
        [Fact] public void 
        WebAction_is_context_transform()
        {
            var req =Request(RequestType.UNKNOWN,"/hello");
            var response = Response("xxx",0);
            var beforeContext = Context(req,response);

            WebAction ok = (ctx) => Context(
                request : ctx.Request,
                response : Response(ctx.Response.Content,200)
            );

            var afterContext = ok(beforeContext);

            
            Assert.Equal(200,afterContext.Response.StatusCode);
            Assert.Equal("xxx",afterContext.Response.Content);
        }
/*
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
