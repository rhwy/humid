namespace Humid
{
    using System;
    public static class Core
    {
        public static RequestType GET => RequestType.GET;
        public static RequestType POST => RequestType.POST;
        public static RequestType DELETE => RequestType.DELETE;
        public static RequestType HEAD => RequestType.HEAD;
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





    #region Web Models
    
    public enum RequestType 
    {
        GET, POST, DELETE, HEAD, UNKNOWN
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
    }

    public struct Route
    {
        public string Template {get;}
        public WebAction Pipeline {get;}
        public Route(string template, WebAction pipeline)
        {
            Template = template;
            Pipeline = pipeline;
        }
        public Route(string template, Func<Context,Context> pipeline)
        {
            Template = template;
            Pipeline = new WebAction(pipeline);
        }
    }

    public delegate Context WebAction(Context before);

    public delegate WebAction WebActionFeature<T>(T value);
    #endregion
}
