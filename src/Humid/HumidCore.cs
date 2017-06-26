namespace Humid
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;

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

        public static Request Request(RequestType type, string route, Dictionary<string,string> routeParams = null)
        => new Request(type,route, routeParams);

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

        public static Route Get(string path)
        {
            return Path(path) | Verbs(Core.GET);
        }
        public static WebAction Pipeline(Func<Context,Context> action)
            => new WebAction(action);

        public static Filter Verbs (params RequestType[] requestTypes)
        {
            return ((Context context, bool ismatch) previous)
                => (
                    previous.context,
                    requestTypes.Any(
                        x=> x == previous.context.Request.Type));
        }

        public static WebAction Do(Func<Context,Context> action)
        => new WebAction(action);

        public static WebAction Do(Func<Context,dynamic> action)
        {
            return new WebAction(c => c.With(model:action(c)));
        }

        public static WebAction JSON
        => new WebAction(c => {
            if(c.Response.Model != null && c.Request.Headers.ContainsKey("accept"))
            {
                var accept = c.Request.Headers["accept"];
                if(accept.Any(x=>x.Contains("json")))
                {
                    var serialized = JsonConvert.SerializeObject(c.Response.Model);
                    return c.With(content:serialized);
                }
            }
            return c;
        });
        public static WebAction OK
        => new WebAction(c =>c.With(statusCode:200));
            
        public static WebAction NOT_FOUND
        => new WebAction(c => c.With(statusCode:404,content:"Page Not Found"));

        public static WebAction Content(string content)
        => new WebAction(c => c.With(content : content));
    }

    public delegate string TransformRouteExpression(string tokensExpressions);
    public delegate Dictionary<string,string> ExtractTokensValuesFromExpression(string regEx, string path);
    public delegate Dictionary<string,string> ExtractTokensFromPath(string route, string path);
    public delegate Dictionary<string,string> ParseQueryString(string queryString);
    public delegate IDictionary<string,string[]> KeyValueStringArrayToDictionary(IEnumerable<string> list);

    public static class Helpers
    {
        public static TransformRouteExpression TransformRouteExpression {get;set;} = transformRouteExpression;
        private static string transformRouteExpression(string tokensExpressions)
        {
            string current = "";
            bool take = false;
            string result = "";
            foreach(char a in tokensExpressions)
            {
                if(a == '{') take = true;
                if(a == '}') {
                    take = false; 
                    result += $"(?<{current}>[a-zA-Z0-9-_]*)";
                    current = ""; 
                }
                if(!take &&  a!='}') { result += a;}
                else {
                    if( a!='{' &&  a!='}') current += a;
                }
            } 
            return result;
        }
        public static ExtractTokensValuesFromExpression ExtractTokensValuesFromExpression {get;set;} = extractTokensValuesFromExpression;
        private static Dictionary<string,string> extractTokensValuesFromExpression(string regEx, string path)
        {
            string regExSalt = "/_";
            path = path + regExSalt;
            var routeExpression = new Regex(regEx+regExSalt);
            var routeParams = new Dictionary<string,string>();
            if(!routeExpression.IsMatch(path)) return routeParams;
            
            var names = routeExpression.GetGroupNames();
            var values = routeExpression.Split(path);
            foreach(var n in routeExpression.GetGroupNumbers())
            {
                routeParams.Add(names[n],values[n]);
            }
            routeParams.Remove("0");
            return routeParams;
        }

        public static ExtractTokensFromPath ExtractTokensFromPath {get;set;} = extractTokensFromPath;
        private static Dictionary<string,string> extractTokensFromPath(string route, string path)
        {
            return extractTokensValuesFromExpression(transformRouteExpression(route),path);
        }

        public static Func<string,string,bool> RouteIsMatch {get;set;} = routeIsMatch;
        private static bool routeIsMatch(string route, string path)
        {
            string regExSalt = "/_";
            path = path + regExSalt;
            var routeExpression = new Regex(transformRouteExpression(route+regExSalt));
            return routeExpression.IsMatch(path);
        }

        public static ParseQueryString ParseQueryString {get;set;} = parseQueryString;
        private static Dictionary<string,string> parseQueryString(string queryString)
        {
            var result = new Dictionary<string,string>();
            if(queryString.StartsWith("?"))
                queryString = queryString.Substring(1);
            var keyValues = queryString.Split("&".ToCharArray());
            foreach (var keyValue in keyValues)
            {
                var temp = keyValue.Split("=".ToCharArray());
                string key = null, value = null;

                if(temp.Length>0)
                    key = temp[0];
                if(temp.Length==2)
                    value = temp[1];
                if(!string.IsNullOrEmpty(key))
                    result.Add(key,value);
            }
            return result;
        }

        public static KeyValueStringArrayToDictionary ExtractHeadersToDictionary {get;set;} = extractHeadersToDictionary;
        private static Dictionary<string,string[]> extractHeadersToDictionary(IEnumerable<string> list)
        {
            string[] parseValues(string value)
            {
                if(string.IsNullOrEmpty(value))
                    return new string[]{};
                return value
                    .Split(",".ToCharArray())
                    .Select(x=>x.Trim())
                    .ToArray();
            }
            return list.Select(x=> {
                if(string.IsNullOrEmpty(x)) return (key:"",value:"");
                var items = x.Split(":".ToCharArray());
                if(items.Length == 1)
                    return (key:items[0],value:"");
                if(items.Length == 2)
                    return (key:items[0],value:items[1]);
                return (key:"",value:"");
            })
            .Where(x=>!string.IsNullOrEmpty(x.key))
            .ToDictionary(x=>x.key, x=>parseValues(x.value)) ?? new Dictionary<string,string[]>();
        }
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
        public string QueryString {get;}
        public Dictionary<string,string> Query {get;}
        public Dictionary<string,string> RouteParams {get;} 
        public IDictionary<string,string[]> Headers {get;}
        public Request(RequestType type, string path,
            Dictionary<string,string> routeParams, 
            string query = null,
            IDictionary<string,string[]> headers=null)
        {
            Type = type; 
            Path = path;
            RouteParams = routeParams ?? new Dictionary<string,string>();
            QueryString = query ?? string.Empty;
            Query = Helpers.ParseQueryString(QueryString);
            Headers = headers;
        }
        
        public static Request Default 
        => new Request( 
            type:RequestType.UNKNOWN,
            path:string.Empty,
            routeParams: new Dictionary<string,string>(),
            query: string.Empty,
            headers : new Dictionary<string,string[]>());
    }

    public struct Response
    {
        public int StatusCode {get;}
        public string Content {get;}
        public dynamic Model {get;}
        public Response(string content, int statusCode, dynamic model = null)
        {
            Content = content; StatusCode = statusCode;
            Model = model;
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
        public T Params<T>(string key, T defaultValue = default(T))
        {
            object outputValue = default(T);
            if(Request.RouteParams.ContainsKey(key))
            {
                outputValue = Request.RouteParams[key];
                try {
                    if(defaultValue is string)
                        return (T)outputValue;
                    return (T)Convert.ChangeType(outputValue,typeof(T));
                }
                catch{
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        public T Query<T>(string key, T defaultValue = default(T))
        {
            object outputValue = default(T);
            if(Request.Query.ContainsKey(key))
            {
                outputValue = Request.Query[key];
                try {
                    if(defaultValue is string)
                        return (T)outputValue;
                    return (T)Convert.ChangeType(outputValue,typeof(T));
                }
                catch{
                    return defaultValue;
                }
            }
            return defaultValue;
        }




        public static Context Default 
        => new Context(Request.Default, Response.Default);

        public Context With(
                RequestType? type = null, 
                string path = null,
                string content = null,
                int? statusCode = null,
                Dictionary<string,string> routeParams = null,
                string query = null,
                IEnumerable<string> stringHeaders = null,
                IDictionary<string,string[]> headers = null,
                dynamic model = null)
        => new Context(
                new Request(
                    type ?? Request.Type,
                    path ?? Request.Path,
                    routeParams ?? Request.RouteParams,
                    query ?? Request.QueryString,
                    setHeaders(headers,Request.Headers,stringHeaders)),
                new Response(
                    content ?? Response.Content,
                    statusCode ?? Response.StatusCode,
                    model ?? Response.Model));
        
        public static Context operator | (Context before, WebAction next)
        => next(before);

        private IDictionary<string,string[]> setHeaders(IDictionary<string,string[]> headers, IDictionary<string,string[]> actual, IEnumerable<string> stringHeaders)
        {
            if(headers != null) return headers;
            if(stringHeaders != null)
            {
                if(actual != null && actual.Any()) return actual;
                return Helpers.ExtractHeadersToDictionary(stringHeaders);
            }
            return actual;
        }
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
                Filters.Add(PathFilter(template));
            }
            
        }

        public static Route Empty => new Route(string.Empty);
        
        // public Route(string template, Func<Context,Context> pipeline)
        // :this(template,new WebAction(pipeline))
        // { }

        public Context ApplyPipeline(Context before)
        {
            var routeParams = Helpers.ExtractTokensFromPath(
                Template,before.Request.Path);
            var contextWithParams = before.With(routeParams : routeParams);        
            var afterContext = (Pipeline == null)? contextWithParams : Pipeline.Invoke(contextWithParams);

            return afterContext;
        }

        public bool Matches(string path)
        {
            return Template == path;
        }

        public bool Matches(Context currentContext)
        {
            var itIsOkToContinue = true;
            Context updatedContext = currentContext;
            foreach (var filter in Filters)
            {
                (updatedContext,itIsOkToContinue) = filter((updatedContext, itIsOkToContinue));
                if(!itIsOkToContinue) return false;
                
            }
            return true;
        }

        public static Route operator | (Route current, WebAction pipeline)
        {
            var route = new Route(
                current.Template, 
                current.Pipeline == null
                ? pipeline
                : new WebAction(
                    c=> pipeline(current.Pipeline(c))
            ));
            route.Filters = current.Filters;
            return route;
        }
        public static Route operator | (Route current, Filter filter)
        {
            current.Filters.Add(filter);
            return current;
        }

        public static Filter PathFilter (string template)
        {
            return ((Context context, bool isMatch) previous)
            => {
                var routeMatch = Helpers.RouteIsMatch(template,previous.context.Request.Path);
                if(routeMatch)
                {
                    var routeParams = Helpers.ExtractTokensFromPath(template,previous.context.Request.Path);
                    return (
                        previous.context.With(routeParams: routeParams), 
                        previous.isMatch);
                }
                return (
                    previous.context,false
                );
            };
        }


        public List<Filter> Filters {get;set;}
        
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
        private static Route routeNotFound = (Route.Empty | WebActions.NOT_FOUND);
        public Route FindRoute(Context context)
        {
            var routeFound =  Routes.Any(x=>x.Matches(context));

            return routeFound ? Routes.First(x=>x.Matches(context)) : routeNotFound;
        }

        public static Router operator + (Router current, Route newRoute)
        {
            current.AddRoute(newRoute);
            return current;
        }
    }

    public delegate Context WebAction(Context before);

    public delegate WebAction WebActionFeature<T>(T value);

    public delegate (Context context, bool isMatch) Filter((Context context, bool isMatch) previous);
    #endregion



} 
