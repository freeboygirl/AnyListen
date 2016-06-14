using System.Collections.Generic;

namespace AnyListen.Model
{
    public class SearchResult
    {
        public int ErrorCode { get; set; }
        public string ErrorMsg { get; set; }
        public string KeyWord { get; set; }
        public int PageNum { get; set; }
        public int TotalSize { get; set; }
        public List<SongResult> Songs { get; set; }
    }
}