namespace Humid.Tests
{
    using System;
    using Xunit;
    //using FunctionalHelpers;
   // using static FunctionalHelpers.Core;
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

            Assert.Equal("***",req.Path);
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
            var req = Request(RequestType.UNKNOWN,"/path");
            var resp = Response("hello",123);

            var context = Context(req,resp);
            Assert.Equal("/path",context.Request.Path);
            Assert.Equal(123, context.Response.StatusCode);
        }

        //as context should be immutable we need a helper to create
        //a new context by just changing a property from a previous 
        //context
        [Fact] public void
        we_can_use_a_helper_to_create_a_new_context_from_previous()
        {
            var previous = Context(
                Request(RequestType.GET,"/hello"),
                Response("hello",200));
            
            var newContext = previous.With(content:"plop");

            Assert.Equal("plop",newContext.Response.Content);
            Assert.Equal("/hello",newContext.Request.Path);
        }

        //we need to start with an empty default context
        [Fact] public void
        we_shoudl_create_a_context_from_default()
        {
            var newContext = Defaults.Context;
            Assert.Equal(RequestType.UNKNOWN,newContext.Request.Type);
        }

        //finally, a web request, is defined as the transformation of
        // a context! that's it ;-), let's call it a WebAction
        // webaction = Func<Context,Context>
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

        //In order to keep webaction signature simple and be able
        //to chain many webbactions, we also need to have a parametrized 
        //webaction which is the composition of a new function and a webaction, 
        //it should return a webaction:
        //WebActionFeature<T> = Func<T,WebAction>
        [Fact] public void
        featured_webaction_is_a_composition_returning_webaction()
        {
            var req =Request(RequestType.UNKNOWN,"/hello");
            var response = Response("xxx",0);
            var beforeContext = Context(req,response);

            WebActionFeature<string> addContent = (content) => 
                (ctx) => ctx.With(content : content);

            WebAction ok = (ctx) => Context(
                request : ctx.Request,
                response : Response(ctx.Response.Content,200)
            );

            var afterContext1 = addContent("oh oh oh")(beforeContext);
            var afterContext = ok(afterContext1);
            Assert.Equal(200,afterContext.Response.StatusCode);
            Assert.Equal("oh oh oh",afterContext.Response.Content);
        }        
        

    }
}
