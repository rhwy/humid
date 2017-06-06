namespace Humid
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Core
    {
        public static RequestType GET => RequestType.GET;
        public static RequestType POST => RequestType.POST;
        public static RequestType DELETE => RequestType.DELETE;
        public static RequestType HEAD => RequestType.HEAD;
        public static RequestType PUT => RequestType.PUT;
        public static RequestType DEFAULT_REQUEST_TYPE => RequestType.UNKNOWN;

        public static class Defaults
        {
            public static Context Context => Context.Default;
            public static Request Request => Request.Default;
            public static Response Response => Response.Default;
        }

        public static Request Request(RequestType type, string route)
        => new Request(type,route);

        public static Response Response(string content, int statusCode)
        => new Response(content,statusCode);

        public static Context Context(Request request, Response response)
        => new Context(request,response);
    }

    public static class WebActions
    {
        public static Route Path(string path) 
        {
            return new Route(path);
        } 

        public static WebAction Pipeline(Func<Context,Context> action)
            => new WebAction(action);
    }



#region Web Models
    
    public enum RequestType 
    {
        GET, POST, DELETE, HEAD, PUT, UNKNOWN
    }

    public struct Request
    {
        public RequestType Type {get;}
        public string Path {get;}
        public Request(RequestType type, string path)
        {
            Type = type; Path = path;
        }
        public static Request Default 
        => new Request(RequestType.UNKNOWN, string.Empty);
    }

    public struct Response
    {
        public int StatusCode {get;}
        public string Content {get;}
        public Response(string content, int statusCode)
        {
            Content = content; StatusCode = statusCode;
        }
        public static Response Default => new Response(string.Empty, 0);
    }

    public struct Context
    {
        public Request Request {get;}
        public Response Response {get;}
        public Context(Request request, Response response)
        {
            Request = request; Response = response;
        }

        public static Context Default 
        => new Context(Request.Default, Response.Default);

        public Context With(
                RequestType? type = null, 
                string path = null,
                string content = null,
                int? statusCode = null)
        => new Context(
                new Request(
                    type ?? Request.Type,
                    path ?? Request.Path),
                new Response(
                    content ?? Response.Content,
                    statusCode ?? Response.StatusCode));
        
        public static Context operator | (Context before, WebAction next)
        => next(before);

        public void Deconstruct(out string content, out int statusCode)
        {
            content = Response.Content;
            statusCode = Response.StatusCode;
        }
    }

    public struct Route
    {
        public string Template {get;}
        public WebAction Pipeline {get;}
        public Route(string template) : this(template, null)
        {
        }

        public Route(string template, WebAction pipeline)
        {
            Template = template;
            Pipeline = pipeline;
            Filters = new List<Filter>{};
            if(!string.IsNullOrEmpty(template))
            {
                Filters.Add(DefaultFilter(template));
            }
            
        }

        public static Route Empty => new Route(string.Empty);
        
        // public Route(string template, Func<Context,Context> pipeline)
        // :this(template,new WebAction(pipeline))
        // { }

        public Context ApplyPipeline(Context before)
        => (Pipeline == null)? before : Pipeline.Invoke(before);

        public bool Matches(string path)
        {
            return Template == path;
        }

        public bool Matches(Context currentContext)
        {
            var itIsOkToContinue = true;
            Context updatedContext = Context.Default;
            foreach (var filter in Filters)
            {
                if(!itIsOkToContinue) return false;
                (updatedContext,itIsOkToContinue) = filter((currentContext, itIsOkToContinue));
            }
            return true;
        }

        public static Route operator | (Route current, WebAction pipeline)
        => new Route(
                current.Template, 
                current.Pipeline == null
                ? pipeline
                : new WebAction(
                    c=> pipeline(current.Pipeline(c))
            ));
        
        public static Route operator | (Route current, Filter filter)
        {
            current.Filters.Add(filter);
            return current;
        }

        public static Filter DefaultFilter (string template)
        {
            return ((Context context, bool isMatch) previous)
            => (previous.context, previous.isMatch && (previous.context.Request.Path == template));
        }


        public List<Filter> Filters {get;}
        
    }

    public class Router
    {
        private List<Route> routes = new List<Route>();
        public IEnumerable<Route> Routes => routes;
        public void AddRoute(Route route)
        {
            if(routes.Any(x=>x.Template == route.Template))
                return;

            routes.Add(route);
        }

    }

    public delegate Context WebAction(Context before);

    public delegate WebAction WebActionFeature<T>(T value);

    public delegate (Context context, bool isMatch) Filter((Context context, bool isMatch) previous);
    #endregion



} 
