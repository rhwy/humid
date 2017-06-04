namespace Humid.Tests
{
    using Xunit;
    using static Humid.Core;
    using static Humid.WebActions;

    using static FunctionalHelpers.Core;

    public class Pipelines
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

            var requestPipeline = Pipeline( 
                (context) => 
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

            var requestPipeline = Pipeline( 
                (context) => 
                    context
                    | addContent("hello")
                    | ok
            );

            Route route = new Route(
                template : "/hello",
                pipeline : requestPipeline
            );
        }

        //if a route matches, we'll want to apply the pipeline to 
        //get the updated context
        [Fact] public void 
        apply_pipeline_for_a_matched_route()
       {
           WebActionFeature<string> addContent = (some) => 
                (ctx) => ctx.With(content : some);

            WebAction ok = (ctx) => Context(
                request : ctx.Request,
                response : Response(ctx.Response.Content,200)
            );

            var requestPipeline = Pipeline( 
                (context) => 
                    context
                    | addContent("hello")
                    | ok
                );

            Route route = new Route(
                template : "/hello",
                pipeline : requestPipeline
            );

            string content = string.Empty;
            int status=0;
            var beforeContext = Defaults.Context.With(path : "/hello");

            if(route.Matches(path: "/hello"))
                (content,status) = route.ApplyPipeline(beforeContext); 
            
            
            Assert.Equal("hello",content);
            Assert.Equal(200,status);
       } 
    }
}