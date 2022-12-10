using Scriban;
using SemWork1.Attributes.HttpAttributes;
using SemWork1.Models;
using SemWork1.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SemWork1.Controllers
{
    [HttpController("explorer")]
    internal class Explorer
    {
        public HttpListenerContext Context { get; set; }
        public Explorer(HttpListenerContext context)
        {
            Context = context;
        }

        [HttpGET("all")]
        public string AllSchools()
        {
            var isAuth = HttpHelper.CheckIsAuthorized(Context.Request, out _);
            var buffer = Array.Empty<byte>();
            var dao = new DAO("SemDB1");
            var schools = dao.Select<School>();
            var cities = schools.Select(s => s.City).Distinct().ToList();
            if(Context.Request.QueryString.AllKeys.Contains("City"))
            {
                var city = Context.Request.QueryString["City"].ToString();
                if(city != "Не выбрано")
                    schools = schools.Where(s => s.City == city).ToList();
            }
            var template = HttpHelper.GetTemplate("\\explorer\\index.html");
            var html = template.Render(new { schools = schools, isauth = isAuth, cities = cities });
            buffer = Encoding.UTF8.GetBytes(html);
            HttpHelper.ConfigureResponse(Context.Response, "text/html", 200, buffer);
            return "success";
        }

        [HttpGET]
        public string GetById(int id)
        {
            var dao = new DAO("SemDB1");
            var school = dao.SelectById<School>(id);
            string res = null;
            if (school is not null)
            {
                var request = Context.Request;
                var buffer = Array.Empty<byte>();
                var template = HttpHelper.GetTemplate("\\templates\\schoolprofile\\index.html");
                var events = dao.Select<SchoolEvent>().Where(e => e.SchoolId == school.Id).ToList();
                var html = template.Render(new { school = school, events = events });
                buffer = Encoding.UTF8.GetBytes(html);
                HttpHelper.ConfigureResponse(Context.Response, "text/html", 200, buffer);
                return "succes";
            }
            else
            {
                res = "there is no such user";
                var buffer = Encoding.UTF8.GetBytes(res);
                HttpHelper.ConfigureResponse(Context.Response, "text/plain", 404, buffer);
                return res;
            }
        }
    }
}
