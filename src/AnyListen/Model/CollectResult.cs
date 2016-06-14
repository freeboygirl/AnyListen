using System.Collections.Generic;

namespace AnyListen.Model
{
    public class CollectResult
    {
        public int ErrorCode { get; set; }
        public string ErrorMsg { get; set; }
        public string CollectId { get; set; }
        public string CollectName { get; set; }
        public string CollectLink { get; set; }
        public string CollectMaker { get; set; }
        public string CollectInfo { get; set; }
        public string CollectLogo { get; set; }
        public string Date { get; set; }
        public string Tags { get; set; }
        public int SongSize { get; set; }
        public int Page { get; set; }
        public List<SongResult> Songs { get; set; }
    }
}