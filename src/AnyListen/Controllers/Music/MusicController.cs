using AnyListen.Api.Music;
using AnyListen.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AnyListen.Controllers.Music
{
    [Route("api/[controller]")]
    public class MusicController : Controller
    {
        [HttpGet("{path}")]
        public void Get(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Response.StatusCode = 403;
                return;
            }
            var paths = path.Split('.');
            if (paths.Length != 2)
            {
                Response.StatusCode = 403;
                return;
            }
            var keys = paths[0].Split('_');
            if (keys.Length != 3)
            {
                Response.StatusCode = 403;
                return;
            }
            var linkInfo = GetUrl(keys[0], keys[2], keys[1], paths[1]);
            if (linkInfo == null)
            {
                Response.StatusCode = 404;
                return;
            }
            if (linkInfo.StartsWith("http://"))
            {
                Response.StatusCode = 302;
                Response.Headers.Add("Location", linkInfo);
            }
            else
            {
                Response.StatusCode = 200;
                Response.WriteAsync(linkInfo);
            }
        }

        private static string GetUrl(string type, string id, string quality, string format)
        {
            IMusic music;
            switch (type)
            {
                case "wy":
                    music = new WyMusic();
                    break;
                default:
                    return null;
            }
            return music.GetSongUrl(id, quality, format);
        }
    }
}
