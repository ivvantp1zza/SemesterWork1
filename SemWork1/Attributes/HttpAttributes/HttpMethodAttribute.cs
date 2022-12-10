using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemWork1.Attributes.HttpAttributes
{
    public class HttpMethodAttribute : Attribute
    {
        public string Route { get; set; }
        public HttpMethodAttribute(string route)
        {
            Route = route;
        }
    }
}
