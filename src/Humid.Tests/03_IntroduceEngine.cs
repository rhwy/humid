namespace Humid.Tests
{
    using Xunit;
    using static Humid.Core;
    using static FunctionalHelpers.Core;
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
        //we can chain multiple webactions to define the pipeline we
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
    }
}