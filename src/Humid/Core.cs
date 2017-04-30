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
        public string Route {get;}
        public Request(RequestType type, string route)
        {
            Type = type; Route = route;
        }
    }

    public struct Response
    {
        public int StatusCode {get;}
        public string Content {get;}
        public Response(string content, int statusCode)
        {
            Content = content; StatusCode = statusCode;
        }
    }

    public struct Context
    {
        public Request Request {get;}
        public Response Response {get;}
        public Context(Request request, Response response)
        {
            Request = request; Response = response;
        }
    }

    public delegate Context WebAction(Context before);
    
    #endregion
}
