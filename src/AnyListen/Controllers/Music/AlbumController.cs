using AnyListen.Api.Music;
using AnyListen.Interface;
using AnyListen.Model;
using Microsoft.AspNetCore.Mvc;

namespace AnyListen.Controllers.Music
{
    [Route("api/[controller]")]
    public class AlbumController : Controller
    {

        [HttpGet("{type}")]
        public AlbumResult Get(string type)
        {
            var id = Request.Query["id"];
            if (id.Count <= 0)
            {
                return new AlbumResult
                {
                    ErrorCode = 403,
                    ErrorMsg = "请输入专辑ID"
                };
            }
            return AlbumSearch(type, id[0]);
        }

        private static AlbumResult AlbumSearch(string type, string id)
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
            return music.AlbumSearch(id);
        }

    }
}
