namespace SemWork1.Attributes.HttpAttributes
{
    public class HttpController : Attribute
    {
        public string Route { get; }

        public HttpController(string route)
        {
            Route = route;
        }
    }
}

