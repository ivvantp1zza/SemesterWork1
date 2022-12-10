using System.Collections.Specialized;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using Scriban;
using SemWork1.Attributes.HttpAttributes;
using SemWork1.Controllers;
using SemWork1.Sessions;

namespace SemWork1;

public class HttpServer
{
    private ServerSettings _settings = new() { Port = 1488, Directory = @"..\..\..\site" };
    private const int MaxListeners = 10;
    private int _activeListeners;
    private static HttpListener _listener;
    private bool _isRunning = false;
    public HttpServer()
    {
        _listener = new HttpListener();
    }

    public async void Start()
    {
        if (_isRunning)
            return;
        var path = @"../../../Settings.json";
        if (File.Exists(path))
        {
            var f = File.ReadAllText(path);
            _settings = JsonConvert.DeserializeObject<ServerSettings>(f);
        }
        _listener.Prefixes.Add($"http://localhost:{_settings.Port}/");
        _listener.Start();
        _isRunning = true;
        Console.WriteLine("Server started");
        Manage();
    }

    public void Manage()
    {
        while (true)
        {
            if (_activeListeners >= MaxListeners)
            {
                Thread.Sleep(100);
                continue;
            }
            _listener.BeginGetContext(AsyncCallback, _listener);
            _activeListeners += 1;
        }
    }

    private void AsyncCallback(IAsyncResult result)
    {
        if (!_listener.IsListening) return;
        var httpContext = _listener.EndGetContext(result);
        _activeListeners -= 1;
        if (!MethodHandler(httpContext))
            StaticFileHandler(httpContext.Request, httpContext.Response);
    }

    public void Stop()
    {
        if (!_isRunning)
            return;
        _listener.Stop();
        _isRunning = false;
        Console.WriteLine("Server stopped");
    }

    
    private void StaticFileHandler(HttpListenerRequest request, HttpListenerResponse response)
    {
        var file = HttpHelper.TryGetFileBytes(request.RawUrl, out var retDefault);
        if (file is null)
        {
            HttpHelper.ConfigureResponse(response, "text/plain", 404, Encoding.UTF8.GetBytes("Resource not found"));
        }
        else
        {
            if (retDefault)
            {
                var isAuth = HttpHelper.CheckIsAuthorized(request, out _);
                var page = Encoding.UTF8.GetString(file);
                var template = Template.Parse(page);
                var html = template.Render(new { isauth = isAuth });
                var buffer = Encoding.UTF8.GetBytes(html);
                HttpHelper.ConfigureResponse(response, "text/html", 200, buffer);
            }
            else
            {
                HttpHelper.ConfigureResponse(response, retDefault ? "text/html" : HttpHelper.GetContentType(request.Url), 200, file);
            }
        }
    }

    private bool MethodHandler(HttpListenerContext _httpContext)
    {
        HttpListenerRequest request = _httpContext.Request;
        HttpListenerResponse response = _httpContext.Response;

        if (_httpContext.Request.Url.Segments.Length < 2) return false;
        string controllerName = _httpContext.Request.Url.Segments[1].Replace("/", "");
        
        var assembly = Assembly.GetExecutingAssembly();
        var controller = assembly.GetTypes()
            .Where(t => Attribute.IsDefined(t, typeof(HttpController)))
            .FirstOrDefault(c => c.GetCustomAttribute<HttpController>().Route == null ? c.Name.ToLower().Equals(controllerName.ToLower()) : c.GetCustomAttribute<HttpController>().Route.ToLower().Equals(controllerName.ToLower()));

        if (controller == null) return false;

        var methods = controller.GetMethods()
                .Where(m => m.GetCustomAttributes().Any(attr => attr.GetType().Name == $"Http{request.HttpMethod}")).ToList();
        var methodUri = request.Url.Segments.Length > 2 && !int.TryParse(request.Url.Segments.Last().Replace("/", ""), out _) ? request.Url.Segments.Last().Replace("/", "") : "";

        
        var method = methods.FirstOrDefault(m => m.GetCustomAttribute<HttpMethodAttribute>().Route == methodUri);
        
        if (method == null) return false;
        var args = GetParams(_httpContext, method);
        object? res = null;
        byte[] buffer = Array.Empty<byte>();
        var ctor = controller.GetConstructor(new Type[] { typeof(HttpListenerContext) });
        var instance = ctor.Invoke(new object[] { _httpContext });
        method.Invoke(instance, args);
        return true;
    }

    public object[] GetParams(HttpListenerContext httpContext, MethodInfo method)
    {
        string[] strParams = httpContext.Request.Url
                                .Segments
                                .Skip(2)
                                .Select(s => s.Replace("/", ""))
                                .ToArray();
        var arg = int.TryParse(strParams.LastOrDefault(), out var num);
        return arg ? method.GetParameters()
                            .Select((p, i) => Convert.ChangeType(strParams[i], p.ParameterType))
                            .ToArray() : new object[0];
    } 
}