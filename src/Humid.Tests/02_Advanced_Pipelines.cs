namespace Humid.Tests
{
    using Xunit;
    using static Humid.Core;
    using static FunctionalHelpers.Core;

    public class AdvancedPipelines
    {
        //we can chain multiple webactions to define the pipeline we
        //want for a request.
        [Fact] public void 
        multiple_webactions_define_the_request_pipeline()
        {
            var newRequestWithContext = Defaults.Context.With(path:"/hello");
            
            WebActionFeature<string> addContent = (content) => 
                (ctx) => ctx.With(content : content);

            WebAction ok = (ctx) => Context(
                request : ctx.Request,
                response : Response(ctx.Response.Content,200)
            );

            var requestPipeline = f( (Context context) => 
                context
                | addContent("hello")
                | ok
                );

            var afterContext = requestPipeline(newRequestWithContext);
            

            Assert.Equal("hello",afterContext.Response.Content);
            Assert.Equal(200,afterContext.Response.StatusCode);        
        }

        //we can now define a route as the couple :
        //(template path, request pipeline)
        [Fact] public void 
        define_route_with_template_and_pipeline()
        {
            WebActionFeature<string> addContent = (content) => 
                (ctx) => ctx.With(content : content);

            WebAction ok = (ctx) => Context(
                request : ctx.Request,
                response : Response(ctx.Response.Content,200)
            );

            var requestPipeline = f( (Context context) => 
                context
                | addContent("hello")
                | ok
                );

            Route route = new Route(
                template : "/hello",
                pipeline : requestPipeline
            );
        }
    }
}