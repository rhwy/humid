using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using static Humid.Core;

namespace Humid.Owin
{
    public class HumidOwinLinker
    {
        
    }

    public static class HumidExtensions
    {
        public static IApplicationBuilder UseHumid(this IApplicationBuilder app, Func<Router,Router> defineRoutes)
        {
            app.UseOwin(pipeline =>
            {
                pipeline(next => SetRouter(defineRoutes));
            });
            return app;
        }

        private static Func<IDictionary<string, object>, Task> SetRouter(Func<Router,Router> defineRoutes)
        {
            var router = defineRoutes(new Router());
            Func<IDictionary<string, object>, Task> owinPlug = (env) => {

                var requestPath = (string)env["owin.RequestPathBase"] + (string)env["owin.RequestPath"];
                var content = string.Empty;

                var beforeContext = Defaults.Context.With(path:requestPath);
                var route = router.FindRoute(beforeContext);

                var afterContext = route.ApplyPipeline(beforeContext);

                var responseBytes = Encoding.UTF8.GetBytes(afterContext.Response.Content);

                var responseStream = (Stream)env["owin.ResponseBody"];
                return responseStream.WriteAsync(responseBytes, 0, responseBytes.Length);
            };
            return owinPlug;
        }
        // public static Task OwinHello(IDictionary<string, object> environment)
        // {
        //     string responseText = "Hello World via OWIN";
        //     byte[] responseBytes = Encoding.UTF8.GetBytes(responseText);

        //     // OWIN Environment Keys: http://owin.org/spec/spec/owin-1.0.0.html
        //     var responseStream = (Stream)environment["owin.ResponseBody"];
        //     var responseHeaders = (IDictionary<string, string[]>)environment["owin.ResponseHeaders"];

        //     responseHeaders["Content-Length"] = new string[] { responseBytes.Length.ToString(CultureInfo.InvariantCulture) };
        //     responseHeaders["Content-Type"] = new string[] { "text/plain" };

        //     return responseStream.WriteAsync(responseBytes, 0, responseBytes.Length);
        // }
    }
}