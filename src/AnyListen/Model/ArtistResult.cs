using System.Collections.Generic;

namespace AnyListen.Model
{
    public class ArtistResult
    {
        public int ErrorCode { get; set; }
        public string ErrorMsg { get; set; }
        public string TransName { get; set; }
        public string ArtistInfo { get; set; }
        public string ArtistLogo { get; set; }
        public string ArtistLink { get; set; }
        public string Genre { get; set; }
        public int SongSize { get; set; }
        public int AlbumSize { get; set; }
        public int Page { get; set; }
        public List<SongResult> Songs { get; set; }
    }
}