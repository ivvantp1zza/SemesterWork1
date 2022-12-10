using Scriban;
using SemWork1.Attributes.HttpAttributes;
using SemWork1.Models;
using SemWork1.ORM;
using SemWork1.Sessions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SemWork1.Controllers
{
    [HttpController("news")]
    internal class News
    {
        public HttpListenerContext Context { get; set; }
        public News(HttpListenerContext context)
        {
            Context = context;
        }
        [HttpGET]
        public string AllNews()
        {
            var isAuth = HttpHelper.CheckIsAuthorized(Context.Request, out var sessionId);
            var buffer = Array.Empty<byte>();
            var dao = new DAO("SemDB1");
            var news = dao.Select<GeneralPost>();
            var template = HttpHelper.GetTemplate("\\news\\index.html");
            var user = new User(-100, "lox", "lox", "lox", "lox", "no");
            if (isAuth)
            {
                var cookieVals = Context.Request.Cookies["SessionId"].Value.Split('@');
                user.Id = SessionManager.GetSessionInfo(sessionId).AccountId;
            }
            var html = template.Render(new { posts = news, isauth = isAuth, user = user });
            buffer = Encoding.UTF8.GetBytes(html);
            HttpHelper.ConfigureResponse(Context.Response, "text/html", 200, buffer);
            return "success";
        }

        [HttpPOST]
        public string AddPost()
        {
            string res = null;
            var request = Context.Request;
            var response = Context.Response;
            if(request.Cookies.Any(c => c.Name == "SessionId"))
            {
                var cookie = request.Cookies["SessionId"];
                var session = SessionManager.GetSessionInfo(new Guid(cookie.Value.Split('@')[0]));
                if(session != null)
                {
                    byte[] buffer = Array.Empty<byte>();
                    NameValueCollection? parsed = HttpHelper.ParseBody(request);
                    var authorId = session.AccountId;
                    var topic = parsed["Topic"];
                    var content = parsed["Content"];
                    var created = parsed["Created"];
                    var dao = new DAO("SemDB1");
                    dao.Insert<GeneralPost>(new GeneralPost(authorId, topic, content, DateTime.Now));
                    HttpHelper.ConfigureResponse(response, "text/plain", (int)HttpStatusCode.Redirect, buffer);
                    res = "post added";
                    return res;
                }
                if(request.Cookies["SessionId"].Value.Split('@').Length == 3)
                {
                    var dao = new DAO("SemDB1");
                    var user = dao.SelectById<User>(int.Parse(cookie.Value.Split('@')[1]));
                    byte[] buffer = Array.Empty<byte>();
                    NameValueCollection? parsed = HttpHelper.ParseBody(request);
                    var authorId = user.Id;
                    var topic = parsed["Topic"];
                    var content = parsed["Content"];
                    var created = parsed["Created"];
                    dao.Insert<GeneralPost>(new GeneralPost(authorId, topic, content, DateTime.Now));
                    HttpHelper.ConfigureResponse(response, "text/plain", (int)HttpStatusCode.Redirect, buffer);
                    res = "post added";
                    return res;
                }
            }
            res = "only authorized users can post news";
            HttpHelper.ConfigureResponse(response, "text/plain", (int)HttpStatusCode.Redirect, Array.Empty<byte>(), "http://localhost:1488/users/auth");
            return res;
        }

        [HttpGET("delete")]
        public string DeletePost()
        {
            var dao = new DAO("SemDB1");
            var id = Context.Request.QueryString["Id"]; 
            dao.DeleteById<GeneralPost>(int.Parse(id));
            HttpHelper.ConfigureResponse(Context.Response, "text/plain", (int)HttpStatusCode.Redirect, Array.Empty<byte>());
            return "success";
        }
    }
}
