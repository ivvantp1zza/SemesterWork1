using Scriban;
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

namespace SemWork1
{
    internal static class HttpHelper
    {
        internal static readonly Dictionary<string, string> ContentTypeDictionary = new()
        {
            { "bmp", "image/bmp" },
            { "gif", "image/gif" },
            { "jpeg", "image/jpeg" },
            { "jpg", "image/jpeg" },
            { "png", "image/png" },
            { "svg", "image/svg+xml" },
            { "webp", "image/webp" },

            { "avi", "video/x-msvideo" },
            { "mkv", "video/x-matroska" },
            { "mp4", "video/mp4" },
            { "mpeg", "video/mpeg" },
            { "mpv", "video/mpv" },
            { "webm", "video/webm" },

            { "aac", "audio/mp4" },
            { "mp3", "audio/mpeg" },
            { "ogg", "audio/ogg" },

            { "css", "text/css" },
            { "csv", "text/csv" },
            { "html", "text/html" },
            { "php", "text/php" },
            { "txt", "text/plain" },

            { "doc", "application/msword" },
            { "docx", "application/msword" },
            { "js", "application/javascript" },
            { "json", "application/json" },
            { "pdf", "application/pdf" },
            { "tex", "application/x-tex" },
            { "xml", "application/xml" },
        };

        public static string GetContentType(Uri url)
        {
            var lastSegment = url.Segments.Last().Split(".");
            var fileType = lastSegment.Length == 2 ? lastSegment[1] : "";
            var contentType = "text/plain";
            ContentTypeDictionary.TryGetValue(fileType, out contentType);
            return contentType is null ? "text/plain" : contentType;
        }

        public static void ConfigureResponse(HttpListenerResponse response, string contentType, int statusCode, byte[] buffer, string redirectUri = @"http://localhost:1488/news")
        
        {
            response.Headers.Set("Content-Type", contentType);
            response.StatusCode = statusCode;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            if (statusCode == (int)HttpStatusCode.Redirect)
                response.Headers.Add(HttpResponseHeader.Location, redirectUri);
            response.Close();
        }

        public static byte[] TryGetFileBytes(string url, out bool returnedDefault)
        {
            returnedDefault = true;
            byte[] buffer = null;
            var directoryPath = "..\\..\\..\\site";
            var fullPath = directoryPath + url.Replace("/", "\\");
            if (Directory.Exists(fullPath))
            {
                fullPath += "\\index.html";
                if (File.Exists(fullPath))
                {
                    using (var sourceStream = File.Open(fullPath, FileMode.Open))
                    {
                        buffer = new byte[sourceStream.Length];
                        sourceStream.Read(buffer, 0, (int)sourceStream.Length);
                    }
                }
            }
            else if (File.Exists(fullPath))
            {
                returnedDefault = false;
                using (var sourceStream = File.Open(fullPath, FileMode.Open))
                {
                    buffer = new byte[sourceStream.Length];
                    sourceStream.Read(buffer, 0, (int)sourceStream.Length);
                }
            }
            return buffer;
        }

        public static NameValueCollection? ParseBody(HttpListenerRequest request)
        {
            if (request.HasEntityBody)
            {
                Stream body = request.InputStream;
                System.Text.Encoding encoding = request.ContentEncoding;
                StreamReader reader = new StreamReader(body, encoding);
                var bodyRet = reader.ReadToEnd();
                return System.Web.HttpUtility.ParseQueryString(bodyRet);
            }
            return null;
        }

        public static Template? GetTemplate(string path)
        {
            var fullPath = "..\\..\\..\\site" + path;
            var page = File.ReadAllText(fullPath);
            return Template.Parse(page);
        }
        
        public static bool CheckIsAuthorized(HttpListenerRequest request, out Guid newSessionId)
        {
            if(request.Cookies.Any(c => c.Name == "SessionId"))
            {
                var cookie = request.Cookies["SessionId"];
                var vals = cookie.Value.Split('@');
                if(SessionManager.CheckSession(new Guid(vals[0])))
                {
                    newSessionId = SessionManager.GetSessionInfo(new Guid(vals[0])).Id;
                    return true;
                }
                else if(vals.Length == 3)
                {
                    var dao = new DAO("SemDB1");
                    var login = dao.SelectById<User>(int.Parse(vals[1])).Login;
                    newSessionId = SessionManager.CreateSession(int.Parse(vals[1]), login, DateTime.Now);
                    return true;
                }
            }
            newSessionId = Guid.Empty;
            return false;
        }

        public static Session? GetSession(HttpListenerRequest request)
        {
            if (request.Cookies.Any(c => c.Name == "SessionId"))
            {
                var cookie = request.Cookies["SessionId"];
                return SessionManager.GetSessionInfo(new Guid(cookie.Value));
            }
            return null;
        }
    }
}
