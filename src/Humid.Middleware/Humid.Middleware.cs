using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

using static Humid.Core;

namespace Humid.Middleware
{
    public class HumidWorkflow
    {
        private readonly RequestDelegate _next;
        private Func<Router,Router> _defineRoutes;
        public HumidWorkflow(RequestDelegate next,Func<Router,Router> defineRoutes)
        {
            _next = next;
            _defineRoutes = defineRoutes;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var router = _defineRoutes(new Router());
            var requestPath = context.Request.Path;
            var method = context.Request.Method;
            var content = string.Empty;

            var beforeContext = Defaults.Context.With(path:requestPath, type:(RequestType)Enum.Parse(typeof(RequestType),method));
            var route = router.FindRoute(beforeContext);
            if( !(Route.Empty.Equals(route) || WebActions.NOT_FOUND.Equals(route)))
            {
                var afterContext = route.ApplyPipeline(beforeContext);
                await context.Response.WriteAsync(afterContext.Response.Content);
            }
            await _next(context);
            
        }
    }
    public static class Linker
    {
        public static IApplicationBuilder UseHumid(
            this IApplicationBuilder builder,Func<Router,Router> defineRoutes)
        {
            return builder.UseMiddleware<HumidWorkflow>(defineRoutes);
        }
    }

    
    // ---------------------------------------------------------------------------
    // we actually deprecated the Owin solution, but if needed it's simply as that:
    // ---------------------------------------------------------------------------
    // public static class LinkerOwin
    // {
    //     public static IApplicationBuilder UseHumidOnOwin(this IApplicationBuilder app, Func<Router,Router> defineRoutes)
    //     {
    //         app.UseOwin(pipeline =>
    //         {
    //             pipeline(next => SetRouter(defineRoutes));
    //         });
    //         return app;
    //     }

    //     private static Func<IDictionary<string, object>, Task> SetRouter(Func<Router,Router> defineRoutes)
    //     {
    //         var router = defineRoutes(new Router());
    //         Func<IDictionary<string, object>, Task> owinPlug = (env) => {

    //             var requestPath = (string)env["owin.RequestPathBase"] + (string)env["owin.RequestPath"];
    //             var method = (string)env["owin.RequestMethod"];
    //             var content = string.Empty;

    //             var beforeContext = Defaults.Context.With(path:requestPath, type:(RequestType)Enum.Parse(typeof(RequestType),method));
    //             var route = router.FindRoute(beforeContext);

    //             var afterContext = route.ApplyPipeline(beforeContext);

    //             var responseBytes = Encoding.UTF8.GetBytes(afterContext.Response.Content);

    //             var responseStream = (Stream)env["owin.ResponseBody"];
    //             return responseStream.WriteAsync(responseBytes, 0, responseBytes.Length);
    //         };
    //         return owinPlug;
    //     }
       
    // }
}