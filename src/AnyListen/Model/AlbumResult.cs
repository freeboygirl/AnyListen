using System.Collections.Generic;

namespace AnyListen.Model
{
    public class AlbumResult
    {
        public int ErrorCode { get; set; }
        public string ErrorMsg { get; set; }
        public string AlbumInfo { get; set; }
        public string AlbumType { get; set; }
        public string AlbumGenre { get; set; }
        public string AlbumLink { get; set; }
        public List<SongResult> Songs { get; set; }
    }
}