namespace Humid.Tests
{
    using Xunit;
    using static Humid.Core;
    using static FunctionalHelpers.Core;
    using static Humid.WebActions;

    using System.Linq;

    ///<summary>
    ///Now we have all the parts to start builder the Humid web engine!
    ///The engine is the central piece of our infrastructure, it:
    ///- stores the routes and pipelines
    ///- applies path to matching routes and exectutes pipelines
    ///- links to owin context
    ///</summary>
    public class IntroduceEngine
    {
        //we can register routes in the engine
        //want for a request.
        [Theory]
        [InlineData("/a","/b",2)]
        [InlineData("/a","/a",1)]
         public void
        engine_stores_project_routes(string path1, string path2,int countExpected)
        {
            var engine = new Engine();
            WebAction ok = c=>c.With(statusCode:200);
            engine.AddRoute(new Route(path1,ok));
            engine.AddRoute(new Route(path2,ok));
            
            Assert.Equal(countExpected, engine.Routes.Count());

            
        }

        //in order to define more specifically routes, we can add filters
        //to routes. filters will help to select the right route for the
        //given request context. If no filter is defined then a default one
        //will be used with only the path template.
        [Theory] 
        [InlineData("/a",true)]
        [InlineData("/b",false)]
        public void
        routes_default_filter_is_only_path_template(string requestPath, bool isMatchExpected)
        {
            var newContext = Defaults.Context.With(path:requestPath);
            
            WebAction ok = c=>c.With(statusCode:200);

            Filter defaultFilter = ((Context context, bool ismatch) previous)
             => (previous.context,previous.context.Request.Path == "/a");
 
            var route = new Route("/a", ok);
            Assert.Equal(1,route.Filters.Count());
            Filter routeFilter = route.Filters.FirstOrDefault();
            Assert.Equal(defaultFilter((newContext, true)),routeFilter((newContext, true)));
            Assert.Equal(isMatchExpected,routeFilter((newContext, true)).isMatch);
        }


        [Fact]
        public void Path_is_a_route_function_with_filter_path()
        {
            var newContext = Defaults.Context.With(path:"/a");
            
            WebAction ok = c=>c.With(statusCode:200).With(content:"hello");

            Route route = Path("/a") | ok;

            string content = null;
            int status = -1;
            if(route.Matches(path: "/a"))
                (content,status) = route.ApplyPipeline(newContext); 
            
            
            Assert.Equal("hello",content);
            Assert.Equal(200,status);

        }

        
    }
}