﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using static Humid.Core;

namespace Humid.Middleware
{
    public class HumidWorkflow
    {
        private readonly RequestDelegate _next;
        private Func<Router,Router> _defineRoutes;
        private IHostingEnvironment _environment;
        public HumidWorkflow(RequestDelegate next,Func<Router,Router> defineRoutes, IHostingEnvironment environment)
        {
            _next = next;
            _defineRoutes = defineRoutes;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var router = _defineRoutes(new Router());
            var requestPath = context.Request.Path;
            var method = context.Request.Method;
            var content = string.Empty;
            var requestHeaders = context.Request.Headers.ToDictionary(k => k.Key, v => v.Value.ToArray());
            var physicalFullPath = Environment.CurrentDirectory;
            var queryString = context.Request.QueryString.HasValue?context.Request.QueryString.Value:"";
            var beforeContext = Defaults.Context.With(
                path:requestPath, 
                type:(RequestType)Enum.Parse(typeof(RequestType),method),
                requestHeaders:requestHeaders,
                server: new Dictionary<string, string>{
                    {"Site:PhysicalFullPath",physicalFullPath},
                    {"Environment",_environment.EnvironmentName}
                },
                query:queryString);
            var route = router.FindRoute(beforeContext);
            if( !Route.Empty.Equals(route) && !WebActions.NOT_FOUND.Equals(route))
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
            this IApplicationBuilder builder, IHostingEnvironment environment, Func<Router,Router> defineRoutes)
        {
            var te = builder.ApplicationServices.GetService(typeof(ITemplateEngine)) as ITemplateEngine;
            Console.WriteLine($"found templates : {te.GetType().Name}");
            WebTemplateEngine.Register("html",te);
            return builder.UseMiddleware<HumidWorkflow>(defineRoutes,environment);
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