using System;
using AnyListen.Api.Music;
using AnyListen.Interface;
using AnyListen.Model;
using Microsoft.AspNetCore.Mvc;


namespace AnyListen.Controllers.Music
{
    [Route("api/[controller]")]
    public class CollectController : Controller
    {

        [HttpGet("{type}")]
        public CollectResult Get(string type)
        {
            var id = Request.Query["id"];
            var p = Request.Query["p"];
            var s = Request.Query["s"];
            if (id.Count <= 0)
            {
                return new CollectResult
                {
                    ErrorCode = 403,
                    ErrorMsg = "请输入歌单ID"
                };
            }
            if (p.Count <= 0)
            {
                p = "1";
            }
            if (s.Count <= 0)
            {
                s = "1000";
            }

            return SearchArtist(type, id[0], p[0], s[0]);
        }

        private static CollectResult SearchArtist(string type, string id, string page, string size)
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
            return music.CollectSearch(id, Convert.ToInt32(page), Convert.ToInt32(size));
        }
    }
}
