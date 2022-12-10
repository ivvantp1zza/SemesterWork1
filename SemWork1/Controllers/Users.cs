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
    [HttpController("users")]
    internal class Users
    {
        public HttpListenerContext Context { get; set; }
        public Users(HttpListenerContext context)
        {
            Context = context;
        }

        [HttpGET]
        public string GetById(int id)
        {
            var dao = new DAO("SemDB1");
            var user = dao.SelectById<User>(id);
            string res = null;
            if (user is not null)
            {
                var request = Context.Request;
                var buffer = Array.Empty<byte>();
                var template = HttpHelper.GetTemplate("\\templates\\profile\\profile.html");
                var html = template.Render(new { user = user });
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

        [HttpGET("profile")]
        public string GetAccountInfo()
        {
            var request = Context.Request;
            string res = null;
            var buffer = Array.Empty<byte>();
            string? html = null;
            User? user = null;
            if (request.Cookies.Any(c => c.Name == "SessionId"))
            {
                var dao = new DAO("SemDB1");
                var cookie = request.Cookies["SessionId"];
                if (SessionManager.CheckSession(new Guid(cookie.Value.Split('@')[0])))
                {
                    var session = SessionManager.GetSessionInfo(new Guid(cookie.Value.Split('@')[0]));
                    user = dao.SelectById<User>(session.AccountId);
                }
                if(request.Cookies["SessionId"].Value.Split('@').Length == 3)
                { 
                    user = dao.SelectById<User>(int.Parse(cookie.Value.Split('@')[1]));
                }
                if(user is not null)
                {
                    var template = HttpHelper.GetTemplate("\\templates\\profile\\profile.html");
                    html = template.Render(new { user = user });
                    buffer = Encoding.UTF8.GetBytes(html);
                    HttpHelper.ConfigureResponse(Context.Response, "text/html", 200, buffer);
                    return "succes";
                }
            }
            res = "please authorize";
            //buffer = Encoding.ASCII.GetBytes(res.ToString());
            HttpHelper.ConfigureResponse(Context.Response, "text/plain", (int)HttpStatusCode.Redirect, buffer, @"http://localhost:1488/users/auth");
            return res;
        }

        [HttpPOST("profile")]
        public string UpdateAccount()
        {
            var request = Context.Request;
            var response = Context.Response;
            NameValueCollection? parsed = HttpHelper.ParseBody(request);
            var login = parsed["Login"];
            var city = parsed["City"];
            var email = parsed["Email"];
            var password = parsed["Password"];
            var dao = new DAO("SemDB1");
            var id = int.Parse(request.Cookies["SessionId"].Value.Split('@')[1]);
            dao.Update<User>(id, "Login", login);
            dao.Update<User>(id, "City", city);
            dao.Update<User>(id, "Email", email);
            dao.Update<User>(id, "Password", password);
            HttpHelper.ConfigureResponse(response, "text/plain", (int)HttpStatusCode.Redirect, Array.Empty<byte>(), "http://localhost:1488/users/profile");
            return null;
        }

        [HttpGET("edit")]
        public string EditProfile()
        {
            var request = Context.Request;
            string res = null;
            var buffer = Array.Empty<byte>();
            string? html = null;
            User? user = null;
            if (request.Cookies.Any(c => c.Name == "SessionId"))
            {
                var dao = new DAO("SemDB1");
                var cookie = request.Cookies["SessionId"];
                if (SessionManager.CheckSession(new Guid(cookie.Value.Split('@')[0])))
                {
                    var session = SessionManager.GetSessionInfo(new Guid(cookie.Value.Split('@')[0]));
                    user = dao.SelectById<User>(session.AccountId);
                }
                if (request.Cookies["SessionId"].Value.Split('@').Length == 3)
                {
                    user = dao.SelectById<User>(int.Parse(cookie.Value.Split('@')[1]));
                }
                if (user is not null)
                {
                    var template = HttpHelper.GetTemplate("\\templates\\editprofile\\index.html");
                    html = template.Render(new { user = user });
                    buffer = Encoding.UTF8.GetBytes(html);
                    HttpHelper.ConfigureResponse(Context.Response, "text/html", 200, buffer);
                    return "succes";
                }
            }
            res = "please authorize";
            //buffer = Encoding.ASCII.GetBytes(res.ToString());
            HttpHelper.ConfigureResponse(Context.Response, "text/plain", (int)HttpStatusCode.Redirect, buffer, @"http://localhost:1488/users/auth");
            return res;
        }

        [HttpPOST("auth")]
        public string Login()
        {
            var request = Context.Request;
            var response = Context.Response;
            if (HttpHelper.CheckIsAuthorized(request, out _))
            {
                HttpHelper.ConfigureResponse(Context.Response, "text/plain", (int)HttpStatusCode.Redirect,  Array.Empty<byte>());
                return "success";
            }
            string res = null;
            byte[] buffer = Array.Empty<byte>();
            NameValueCollection? parsed = HttpHelper.ParseBody(request);
            var login = parsed["Login"];
            var password = parsed["Password"];
            var rememberme = parsed["checkbox"];
            var dao = new DAO("SemDB1");
            var users = dao.Select<User>();
            var acc = users.FirstOrDefault(x => x.Login == login && x.Password == password);
            var sessionId = acc is not null ? SessionManager.CreateSession(acc.Id, login, DateTime.Now) : Guid.Empty;
            //res = method.Invoke(Activator.CreateInstance(controller), new object[] { login, password });
            if (sessionId != Guid.Empty)
            {
                if (rememberme == "on")
                    response.Cookies.Add(new Cookie("SessionId", $"{sessionId}@{acc.Id}@rm")
                    {
                        Expires = DateTime.Now.AddYears(1),
                        Path = "/",
                    }) ;
                else
                {
                    response.Cookies.Add(new Cookie("SessionId", $"{sessionId}@{acc.Id}")
                    {
                        Expires = DateTime.Now.AddDays(1),
                        Path = "/"
                    });
                }
                
                res = $"welcome {login}!";
                buffer = Encoding.ASCII.GetBytes(res.ToString());
                HttpHelper.ConfigureResponse(response, "text/plain", (int)HttpStatusCode.Redirect, Array.Empty<byte>());
            }
            else
            {
                res = $"No such user like {login} registered in system.";
                //buffer = Encoding.ASCII.GetBytes(res.ToString());
                HttpHelper.ConfigureResponse(response, "text/plain", (int)HttpStatusCode.Redirect, buffer, "http://localhost:1488/users/auth");
            }
            return res;
        }

        [HttpGET("auth")]
        public string LoginPage()
        {
            var request = Context.Request;
            string res = null;
            var buffer = Array.Empty<byte>();
            //var bytes = HttpHelper.TryGetFileBytes(request.RawUrl, out _);
            //var page = Encoding.UTF8.GetString(bytes);
            //var template = Template.Parse(page);
            //var html = template.Render(new { user = user });
            buffer = HttpHelper.TryGetFileBytes(request.RawUrl.Replace("users/", ""), out _);
            HttpHelper.ConfigureResponse(Context.Response, "text/html", 200, buffer);
            return "success";
        }

        [HttpGET("logout")]
        public string Logout()
        {
            var request = Context.Request;
            var response = Context.Response;
            var cookie = request.Cookies["SessionId"];
            var spl = cookie.Value.Split('@');
            SessionManager.DeleteSession(new Guid(spl[0]));
            response.Cookies.Add(new Cookie("SessionId", $"{spl[0]}@{spl[1]}")
            {
                Path = "/"
            });
            HttpHelper.ConfigureResponse(response, "text/plain", (int)HttpStatusCode.Redirect, Array.Empty<byte>(), "http://localhost:1488/news");
            return "logged out";
        }

        [HttpPOST("register")]
        public string Register()
        {
            var request = Context.Request;
            var response = Context.Response;
            string res = null;
            var dao = new DAO("SemDB1");
            byte[] buffer = Array.Empty<byte>();
            NameValueCollection? parsed = HttpHelper.ParseBody(request);
            var login = parsed["Login"];
            var password = parsed["Password"];
            var email = parsed["Email"];
            if(dao.Select<User>().Where(u => u.Email == email).Any())
            {
                res = "User with such email is already exists";
                HttpHelper.ConfigureResponse(response, "text/plain", 200, Encoding.UTF8.GetBytes(res));
                return res;
            }
            var city = parsed["City"];
            var sex = parsed["Sex"];
            
            dao.Insert<User>(new User(login, email, password, city, sex));
            res = $"User {login} added";
            //buffer = Encoding.ASCII.GetBytes(res.ToString());
            HttpHelper.ConfigureResponse(response, "text/plain", (int)HttpStatusCode.Redirect, buffer, "http://localhost:1488/users/auth");
            return $"User {login} added";
        }

        [HttpGET("register")]
        public string RegisterPage()
        {
            var request = Context.Request;
            string res = null;
            var buffer = Array.Empty<byte>();
            buffer = HttpHelper.TryGetFileBytes(request.RawUrl.Replace("users/", ""), out _);
            HttpHelper.ConfigureResponse(Context.Response, "text/html", 200, buffer);
            return "success";
        }
    }
}
