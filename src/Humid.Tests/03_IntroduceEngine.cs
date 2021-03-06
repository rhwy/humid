namespace Humid.Tests
{
    using Xunit;
    using static Humid.Core;
    //using static FunctionalHelpers.Core;
    using static Humid.WebActions;

    using System.Linq;
    using NFluent;

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
            var engine = new Router();
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

            
            var route = new Route("/a", ok);
            Check.That(route.Filters).HasSize(1);
            Filter routeFilter = route.Filters.FirstOrDefault();
            Assert.Equal(isMatchExpected,routeFilter((newContext, true)).isMatch);
        }

        ///use Path helper to create a new route from path and pipe it 
        ///with webactions
        [Fact]
        public void Path_is_a_route_function_with_default_filter_path_and_can_continue_with_webactions()
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

        ///ensure Route.Matches(context) really filters 
        [Fact]
        public void Ensure_Matches_filters_correctly()
        {
            var route = Route.Empty;

            Filter pathFilter = ((Context context, bool ismatch) previous)
             => (previous.context,previous.context.Request.Path == "/a");

            Filter verbFilter = ((Context context, bool ismatch) previous)
             => (previous.context,previous.context.Request.Type == RequestType.GET);
 
             Filter alwaysTrueFilter = ((Context context, bool ismatch) previous) 
             => (previous.context, true);

            route.Filters.Add(alwaysTrueFilter);
            route.Filters.Add(verbFilter);
            route.Filters.Add(pathFilter);

            Assert.Equal(3, route.Filters.Count());
            
            var testContext = Defaults.Context.With(path:"/a",type:GET);

            var isRouteMatchingForContext = route.Matches(testContext);
            Check.That(isRouteMatchingForContext).IsTrue();
        }
        ///use Path helper to create a new route from path and pipe it 
        ///with another filter
        [Fact]
        public void Path_is_a_route_function_with_filter_path()
        {
            var testContext = Defaults.Context.With(path:"/a",type:GET);
            
            WebAction ok = c=>c.With(statusCode:200).With(content:"hello");

            Route route = Path("/a") 
                        | Verbs(GET,POST)
                        | ok;

            string content = null;
            int status = -1;

            if(route.Matches(testContext))
                (content,status) = route.ApplyPipeline(testContext); 
            
            
            Assert.Equal("hello",content);
            Assert.Equal(200,status);

        }

       [Fact]
       public void route_not_found_is_a_predefined_webaction()
       {
           var testContext = Defaults.Context.With(path:"/a",type:GET);
            
           var afterContext = NOT_FOUND(testContext);
          
           Assert.Equal(404,afterContext.Response.StatusCode);
       } 

       [Theory]
       [InlineData("/a",404)]
       [InlineData("/b",200)]
       [InlineData("/c",200)]
       [InlineData("/zzzz",404)]
       public void router_should_return_route_or_not_found(string path, int expectedStatusCode)
       {
           var testContext = Defaults.Context.With(path:path,type:GET);
           var router = new Router();
            WebAction ok = c=>c.With(statusCode:200);
            router.AddRoute(new Route("/b",ok));
            router.AddRoute(new Route("/c",ok));

            // Route routeNotFound = Route.Empty | NOT_FOUND;

            var route = router.FindRoute(testContext);

            string content = null;
            int status = -1;

           (content,status) = route.ApplyPipeline(testContext); 
            
            Assert.Equal(expectedStatusCode,status);
       }

      [Fact]
      public void easy_route_adding()
      {
          var testContext = Defaults.Context.With(path:"/b",type:GET);
           
          var emptyRouter = new Router();

          var router = emptyRouter
          + (new Route("/a",OK) | Content("I'm A"))
          + (new Route("/b",OK) | Content("I'm B"));
          
          Assert.Equal(2, router.Routes.Count());

          var route = router.FindRoute(testContext);

          string content = null;
            int status = -1;

           (content,status) = route.ApplyPipeline(testContext); 
            
            Assert.Equal(200,status);
            Assert.Equal("I'm B",content);

      }

      [Fact]
      public void
          routes_can_be_grouped_by_an_expression_area()
      {
          var testContext = Defaults.Context.With(path:"/c",type:GET);
           
          var emptyRouter = new Router();
          var b_and_c_area = f((Router r) 
              => r
                 + (new Route("/b", OK) | Content("I'm B"))
                 + (new Route("/c", OK) | Content("I'm C")));
          
          var router = emptyRouter
                       + (new Route("/a",OK) | Content("I'm A"))
                       + b_and_c_area;
          
          Assert.Equal(3, router.Routes.Count());

          var route = router.FindRoute(testContext);

          string content = null;
          int status = -1;

          (content,status) = route.ApplyPipeline(testContext); 
            
          Assert.Equal(200,status);
          Assert.Equal("I'm C",content);
      }
    }
}