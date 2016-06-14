using AnyListen.Api.Music;
using AnyListen.Interface;
using AnyListen.Model;
using Microsoft.AspNetCore.Mvc;


namespace AnyListen.Controllers.Music
{
    [Route("api/[controller]")]
    public class SongController : Controller
    {

        [HttpGet("{type}")]
        public SongResult Get(string type)
        {
            var id = Request.Query["id"];
            if (id.Count <= 0)
            {
                return null;
            }
            return SearchSong(type, id[0]);
        }

        private static SongResult SearchSong(string type, string id)
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
            return music.GetSingleSong(id);
        }

    }
}
