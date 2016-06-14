using System.Collections.Generic;
using AnyListen.Model;

namespace AnyListen.Interface
{
    public interface IMusic
    {
        SearchResult SongSearch(string key, int page, int size);

        AlbumResult AlbumSearch(string id);

        ArtistResult ArtistSearch(string id, int page, int size);

        CollectResult CollectSearch(string id, int page, int size);

        SongResult GetSingleSong(string id, bool isDetials = false);

        string GetSongUrl(string id, string quality, string format);
    }
}