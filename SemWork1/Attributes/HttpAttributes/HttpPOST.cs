namespace SemWork1.Attributes.HttpAttributes
{
    public class HttpPOST : HttpMethodAttribute
    {
        public HttpPOST(string route = "") : base(route)
        { }
    }
}
