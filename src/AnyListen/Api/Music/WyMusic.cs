using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using AnyListen.Helper;
using AnyListen.Interface;
using AnyListen.Model;
using Newtonsoft.Json.Linq;

namespace AnyListen.Api.Music
{
    public class WyMusic : IMusic
    {
        #region 加密发送包

        //此处为网易客户端用户Cookie，会员Cookie可以获取无损地址
        public static string WyNewCookie = "__remember_me=true; MUSIC_U=5f9d910d66cb2440037d1c68e6972ebb9f15308b56bfeaa4545d34fbabf71e0f36b9357ab7f474595690d369e01fbb9741049cea1c6bb9b6; __csrf=8ea789fbbf78b50e6b64b5ebbb786176; os=uwp; osver=10.0.10586.318; appver=1.2.1; deviceId=0e4f13d2d2ccbbf31806327bd4724043";

        private static string GetEncHtml(string url, string text)
        {
            //加密参考 https://github.com/darknessomi/musicbox
            //该处使用固定密钥，简化操作，效果与随机密钥一致
            const string secKey = "a44e542eaac91dce";
            var pad = 16 - text.Length % 16;
            for (var i = 0; i < pad; i++)
            {
                text = text + Convert.ToChar(pad);
            }
            var encText = AesEncrypt(AesEncrypt(text, "0CoJUm6Qyw8W8jud"), secKey);
            const string encSecKey = "411571dca16717d9af5ef1ac97a8d21cb740329890560688b1b624de43f49fdd7702493835141b06ae45f1326e264c98c24ce87199c1a776315e5f25c11056b02dd92791fcc012bff8dd4fc86e37888d5ccc060f7837b836607dbb28bddc703308a0ba67c24c6420dd08eec2b8111067486c907b6e53c027ae1e56c188bc568e";
            var data = new Dictionary<string, string>
            {
                {"params", encText},
                {"encSecKey", encSecKey},
            };
            var html = CommonHelper.PostData(url, data, 0, 0, new Dictionary<string, string>
            {
                {"Cookie", WyNewCookie}
            });
            return html;
        }

        private static string AesEncrypt(string toEncrypt, string key, string iv = "0102030405060708")
        {
            var keyArray = Encoding.UTF8.GetBytes(key);
            var ivArr = Encoding.UTF8.GetBytes(iv);
            var toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);
            using (var aesDel = Aes.Create())
            {
                aesDel.IV = ivArr;
                aesDel.Key = keyArray;
                aesDel.Mode = CipherMode.CBC;
                aesDel.Padding = PaddingMode.PKCS7;
                var cTransform = aesDel.CreateEncryptor();
                var resultArr = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                return Convert.ToBase64String(resultArr, 0, resultArr.Length);
            }
        }

        #endregion

        private static SearchResult Search(string key, int page, int size)
        {
            var text = "{\"s\":\"" + key + "\",\"type\":1,\"offset\":" + (page - 1) * size + ",\"limit\":" + size + ",\"total\":true}";
            var html = GetEncHtml("http://music.163.com/weapi/cloudsearch/get/web?csrf_token=", text);
            var result = new SearchResult
            {
                ErrorCode = 200,
                ErrorMsg = "OK",
                KeyWord = key,
                PageNum = page,
                TotalSize = 0,
                Songs = new List<SongResult>()
            };
            if (string.IsNullOrEmpty(html) || html == "null")
            {
                result.ErrorCode = 300;
                result.ErrorMsg = "获取源代码失败";
                return result;
            }
            try
            {
                var json = JObject.Parse(html);
                var datas = json["result"]["songs"];
                var list = GetListByJson(datas);
                if (list == null || list.Count <= 0)
                {
                    result.ErrorCode = 404;
                    result.ErrorMsg = "没有找到符合要求的歌曲";
                    return result;
                }
                result.TotalSize = json["result"]["songCount"].Value<int>();
                result.Songs = list;
                return result;
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex);
                result.ErrorCode = 404;
                result.ErrorMsg = "没有找到符合要求的歌曲";
                return result;
            }
        }

        private static List<SongResult> GetListByJson(JToken jsons)
        {
            var list = new List<SongResult>();
            foreach (var j in jsons)
            {
                try
                {
                    var ar = j["ar"].Aggregate("", (current, jToken) => current + (jToken["name"].ToString() + ";"));
                    var song = new SongResult
                    {
                        SongId = j["id"].ToString(),
                        SongName = WebUtility.HtmlDecode(j["name"].ToString()),
                        SongSubName = WebUtility.HtmlDecode(j["alia"].First?.ToString()),
                        SongLink = "http://music.163.com/#/song?id="+ j["id"],

                        ArtistId = j["ar"].First["id"].ToString(),
                        ArtistName = WebUtility.HtmlDecode(ar).TrimEnd(';'),
                        ArtistSubName = "",

                        AlbumId = j["al"]["id"].ToString(),
                        AlbumName = WebUtility.HtmlDecode(j["al"]["name"].ToString()),
                        AlbumSubName = WebUtility.HtmlDecode(j["al"]["alia"]?.First?.ToString()),
                        AlbumArtist = WebUtility.HtmlDecode(j["ar"].First["name"].ToString()),

                        Length =CommonHelper.NumToTime((Convert.ToInt32(j["dt"].ToString()) / 1000).ToString()),
                        Size = "",
                        BitRate = "128K",

                        FlacUrl = "",
                        ApeUrl = "",
                        WavUrl = "",
                        SqUrl = "",
                        HqUrl = "",
                        LqUrl = "",
                        CopyUrl = "",

                        PicUrl = CommonHelper.GetSongUrl("wy", "320", j["id"].ToString(), "jpg"),
                        LrcUrl = CommonHelper.GetSongUrl("wy", "320", j["id"].ToString(), "lrc"),
                        TrcUrl = CommonHelper.GetSongUrl("wy", "320", j["id"].ToString(), "trc"),
                        KrcUrl = CommonHelper.GetSongUrl("wy", "320", j["id"].ToString(), "krc"),

                        MvId = j["mv"].ToString(),
                        MvHdUrl = "",
                        MvLdUrl = "",

                        Language = "",
                        Company = "",
                        Year = "",
                        Disc = j["cd"].ToString(),
                        TrackNum = j["no"].ToString(),
                        Type = "wy"
                    };
                    if (string.IsNullOrEmpty(song.Disc))
                    {
                        song.Disc = "1";
                    }
                    if (!string.IsNullOrEmpty(song.MvId))
                    {
                        if (song.MvId != "0")
                        {
                            song.MvHdUrl = CommonHelper.GetSongUrl("wy", "hd", song.MvId, "mp4");
                            song.MvLdUrl = CommonHelper.GetSongUrl("wy", "ld", song.MvId, "mp4");
                        }
                    }
                    var maxBr = j["privilege"]["maxbr"].ToString();
                    if (maxBr == "999000")
                    {
                        song.BitRate = "无损";
                        song.FlacUrl = CommonHelper.GetSongUrl("wy", "999", song.SongId, "flac");
                        song.SqUrl = CommonHelper.GetSongUrl("wy", "320", song.SongId, "mp3");
                        song.HqUrl = CommonHelper.GetSongUrl("wy", "160", song.SongId, "mp3");
                        song.LqUrl = CommonHelper.GetSongUrl("wy", "128", song.SongId, "mp3");
                        song.CopyUrl = song.SqUrl;
                    }
                    else if (maxBr == "320000")
                    {
                        song.BitRate = "320K";
                        song.SqUrl = CommonHelper.GetSongUrl("wy", "320", song.SongId, "mp3");
                        song.HqUrl = CommonHelper.GetSongUrl("wy", "160", song.SongId, "mp3");
                        song.LqUrl = CommonHelper.GetSongUrl("wy", "128", song.SongId, "mp3");
                        song.CopyUrl = song.SqUrl;
                    }
                    else if (maxBr == "160000")
                    {
                        song.BitRate = "192K";
                        song.HqUrl = CommonHelper.GetSongUrl("wy", "160", song.SongId, "mp3");
                        song.LqUrl = CommonHelper.GetSongUrl("wy", "128", song.SongId, "mp3");
                        song.CopyUrl = song.HqUrl;
                    }
                    else
                    {
                        song.BitRate = "128K";
                        song.LqUrl = CommonHelper.GetSongUrl("wy", "128", song.SongId, "mp3");
                        song.CopyUrl = song.LqUrl;
                    }
                    if (j["fee"].ToString() == "4" || j["privilege"]["st"].ToString() != "0")
                    {
                        if (song.BitRate == "无损")
                        {
                            song.BitRate = "320K";
                            song.FlacUrl = "";
                        }
                    }
                    list.Add(song);
                }
                catch (Exception ex)
                {
                    CommonHelper.AddLog(ex);
                }
            }
            return list;
        }

        private static SongResult SearchSingle(string id)
        {
            var text = new Dictionary<string, string>
            {
                {"c", "[{\"id\":\"" + id + "\"}]"}
            };
            var html = CommonHelper.PostData("http://music.163.com/api/v3/song/detail", text, 0, 0,
                new Dictionary<string, string>
                {
                    {"Cookie", WyNewCookie}
                });
            if (string.IsNullOrEmpty(html) || html == "null")
            {
                return null;
            }
            var json = JObject.Parse(html);
            return GetListByJToken(json)?[0];
        }

        private static List<SongResult> GetListByJToken(JObject json,bool isPlayList = false)
        {
            var datas = isPlayList? json["playlist"]["tracks"] : json["songs"];
            var list = new List<SongResult>();
            var index = -1;
            foreach (var j in datas)
            {
                try
                {
                    index++;
                    var ar = j["ar"].Aggregate("", (current, jToken) => current + (jToken["name"].ToString() + ";"));
                    var song = new SongResult
                    {
                        SongId = j["id"].ToString(),
                        SongName = WebUtility.HtmlDecode(j["name"].ToString()),
                        SongSubName = WebUtility.HtmlDecode(j["alia"].First?.ToString()),
                        SongLink = "http://music.163.com/#/song?id=" + j["id"],

                        ArtistId = j["ar"].First["id"].ToString(),
                        ArtistName = WebUtility.HtmlDecode(ar).TrimEnd(';'),
                        ArtistSubName = "",

                        AlbumId = j["al"]["id"].ToString(),
                        AlbumName = WebUtility.HtmlDecode(j["al"]["name"].ToString()),
                        AlbumSubName = WebUtility.HtmlDecode(j["al"]["alia"]?.First?.ToString()),
                        AlbumArtist = WebUtility.HtmlDecode(j["ar"].First["name"].ToString()),

                        Length = CommonHelper.NumToTime((Convert.ToInt32(j["dt"].ToString()) / 1000).ToString()),
                        Size = "",
                        BitRate = "128K",

                        FlacUrl = "",
                        ApeUrl = "",
                        WavUrl = "",
                        SqUrl = "",
                        HqUrl = "",
                        LqUrl = "",
                        CopyUrl = "",

                        PicUrl = CommonHelper.GetSongUrl("wy", "320", j["id"].ToString(), "jpg"),
                        LrcUrl = CommonHelper.GetSongUrl("wy", "320", j["id"].ToString(), "lrc"),
                        TrcUrl = CommonHelper.GetSongUrl("wy", "320", j["id"].ToString(), "trc"),
                        KrcUrl = CommonHelper.GetSongUrl("wy", "320", j["id"].ToString(), "krc"),

                        MvId = j["mv"].ToString(),
                        MvHdUrl = "",
                        MvLdUrl = "",

                        Language = "",
                        Company = "",
                        Year = "",
                        Disc = j["cd"].ToString(),
                        TrackNum = j["no"].ToString(),
                        Type = "wy"
                    };
                    if (string.IsNullOrEmpty(song.Disc))
                    {
                        song.Disc = "1";
                    }
                    if (!string.IsNullOrEmpty(song.MvId))
                    {
                        if (song.MvId != "0")
                        {
                            song.MvHdUrl = CommonHelper.GetSongUrl("wy", "hd", song.MvId, "mp4");
                            song.MvLdUrl = CommonHelper.GetSongUrl("wy", "ld", song.MvId, "mp4");
                        }
                    }
                    var maxBr = json["privileges"][index]["maxbr"].ToString();
                    if (maxBr == "999000")
                    {
                        song.BitRate = "无损";
                        song.FlacUrl = CommonHelper.GetSongUrl("wy", "999", song.SongId, "flac");
                        song.SqUrl = CommonHelper.GetSongUrl("wy", "320", song.SongId, "mp3");
                        song.HqUrl = CommonHelper.GetSongUrl("wy", "160", song.SongId, "mp3");
                        song.LqUrl = CommonHelper.GetSongUrl("wy", "128", song.SongId, "mp3");
                        song.CopyUrl = song.SqUrl;
                    }
                    else if (maxBr == "320000")
                    {
                        song.BitRate = "320K";
                        song.SqUrl = CommonHelper.GetSongUrl("wy", "320", song.SongId, "mp3");
                        song.HqUrl = CommonHelper.GetSongUrl("wy", "160", song.SongId, "mp3");
                        song.LqUrl = CommonHelper.GetSongUrl("wy", "128", song.SongId, "mp3");
                        song.CopyUrl = song.SqUrl;
                    }
                    else if (maxBr == "160000")
                    {
                        song.BitRate = "192K";
                        song.HqUrl = CommonHelper.GetSongUrl("wy", "160", song.SongId, "mp3");
                        song.LqUrl = CommonHelper.GetSongUrl("wy", "128", song.SongId, "mp3");
                        song.CopyUrl = song.HqUrl;
                    }
                    else
                    {
                        song.BitRate = "128K";
                        song.LqUrl = CommonHelper.GetSongUrl("wy", "128", song.SongId, "mp3");
                        song.CopyUrl = song.LqUrl;
                    }
                    if (j["fee"].ToString() == "4" || json["privileges"][index]["st"].ToString() != "0")
                    {
                        if (song.BitRate == "无损")
                        {
                            song.BitRate = "320K";
                            song.FlacUrl = "";
                        }
                    }
                    list.Add(song);
                }
                catch (Exception ex)
                {
                    CommonHelper.AddLog(ex);
                }
            }
            return list;
        }

        private static AlbumResult SearchAlbum(string id)
        {
            var text = "{\"id\":\"" + id + "\"}";
            var html = GetEncHtml("http://music.163.com/weapi/v1/album/" + id + "?csrf_token=", text);
            var result = new AlbumResult
            {
                ErrorCode = 200,
                ErrorMsg = "OK",
                AlbumLink = "http://music.163.com/#/album?id="+id,
                Songs = new List<SongResult>()
            };
            if (string.IsNullOrEmpty(html) || html == "null")
            {
                result.ErrorCode = 300;
                result.ErrorMsg = "获取源代码失败";
                return result;
            }
            try
            {
                var json = JObject.Parse(html);
                var ar = json["album"]["artist"]["name"].ToString();
                var pic = json["album"]["picUrl"].ToString();
                var cmp = json["album"]["company"].ToString();
                var year =
                    CommonHelper.UnixTimestampToDateTime(Convert.ToInt64(json["album"]["publishTime"].ToString())/1000)
                        .ToString("yyyy-MM-dd");
                result.AlbumType = json["album"]["type"].ToString();
                result.AlbumInfo = json["album"]["briefDesc"].ToString();
                result.AlbumGenre = json["album"]["tags"].ToString();
                var datas = json["songs"];
                result.Songs = GetListByJson(datas);
                foreach (var s in result.Songs)
                {
                    s.AlbumArtist = ar;
                    s.PicUrl = pic;
                    s.Company = cmp;
                    s.Year = year;
                }
                return result;
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex);
                result.ErrorCode = 500;
                result.ErrorMsg = "专辑解析失败";
                return result;
            }
        }

        private static ArtistResult SearchArtist(string id)
        {
            var text = "{\"id\":\"" + id + "\"}";
            var html = GetEncHtml("http://music.163.com/weapi/v1/artist/" + id + "?csrf_token=", text);
            var result = new ArtistResult
            {
                ErrorCode = 200,
                ErrorMsg = "OK",
                ArtistLink = "http://music.163.com/#/artist?id=" + id,
                Songs = new List<SongResult>()
            };
            if (string.IsNullOrEmpty(html) || html == "null")
            {
                result.ErrorCode = 300;
                result.ErrorMsg = "获取源代码失败";
                return result;
            }
            try
            {
                var json = JObject.Parse(html);
                var alias = json["artist"]["alias"]?.First?.ToString();
                result.TransName = json["artist"]["trans"].ToString();
                result.ArtistInfo = json["artist"]["briefDesc"].ToString();
                result.ArtistLogo = json["artist"]["img1v1Url"].ToString();
                result.Page = 1;
                result.AlbumSize = json["artist"]["albumSize"].Value<int>();
                result.SongSize = json["artist"]["musicSize"].Value<int>();
                var datas = json["hotSongs"];
                result.Songs = GetListByJson(datas);
                if (!string.IsNullOrEmpty(alias))
                {
                    foreach (var s in result.Songs)
                    {
                        s.ArtistSubName = alias;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex);
                result.ErrorCode = 500;
                result.ErrorMsg = "热门歌曲解析失败";
                return result;
            }
        }

        private static CollectResult SearchCollect(string id, int page, int size)
        {
            var text = "{\"id\":\"" + id + "\",\"n\":" + size + ",\"offset\":" + (page - 1)*size + ",\"limit\":" + size +
                       ",\"total\":true}";
            var html = GetEncHtml("http://music.163.com/weapi/v3/playlist/detail?csrf_token=", text);
            var result = new CollectResult
            {
                ErrorCode = 200,
                ErrorMsg = "OK",
                CollectId = id,
                CollectLink = "http://music.163.com/#/playlist?id=" + id,
                Songs = new List<SongResult>()
            };

            if (string.IsNullOrEmpty(html) || html == "null")
            {
                result.ErrorCode = 300;
                result.ErrorMsg = "获取源代码失败";
                return result;
            }
            try
            {
                var json = JObject.Parse(html);
                result.Songs = GetListByJToken(json,true);
                result.CollectInfo = json["playlist"]["description"].ToString();
                result.CollectName = json["playlist"]["name"].ToString();
                result.Page = page;
                result.SongSize = json["playlist"]["trackCount"].Value<int>();
                result.Date =
                    CommonHelper.UnixTimestampToDateTime(Convert.ToInt64(json["playlist"]["createTime"].ToString()) /1000)
                        .ToString("yyyy-MM-dd");
                var picId = json["playlist"]["coverImgId"].ToString();
                var encryptPath = EncryptId(picId);
                result.CollectLogo = $"http://p4.music.126.net/{encryptPath}/{picId}.jpg";
                result.CollectMaker = json["playlist"]["creator"]["nickname"].ToString();
                return result;
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex);
                result.ErrorCode = 500;
                result.ErrorMsg = "歌单解析失败";
                return result;
            }
        }

        private static string GetUrl(string id, string quality, string format)
        {
            var text = "";
            switch (format.ToLower())
            {
                case "mp3":
                    if (quality == "320")
                    {
                        text = GetPlayUrl(id, "320000");
                    }
                    else if (quality == "160")
                    {
                        text = GetPlayUrl(id, "192000");
                    }
                    else
                    {
                        text = GetPlayUrl(id, "128000");
                    }
                    break;
                case "flac":
                    text = GetPlayUrl(id, "999000");
                    break;
                case "mp4":
                    text = GetMvUrl(id, quality.ToLower());
                    break;
                case "lrc":
                    text = GetLrc(id);
                    break;
                case "krc":
                    text = GetLrc(id, format.ToLower());
                    break;
                case "trc":
                    text = GetLrc(id, format.ToLower());
                    break;
                case "jpg":
                    text = GetPic(id);
                    break;
            }
            return text;
        }

        #region 新版API
        private static string GetPlayUrl(string id, string quality)
        {
            var text = "{\"ids\":[\"" + id + "\"],\"br\":" + quality + ",\"csrf_token\":\"\"}";
            var html = GetEncHtml("http://music.163.com/weapi/song/enhance/player/url?csrf_token=", text);
            if (string.IsNullOrEmpty(html) || html == "null")
            {
                return null;
            }
            var json = JObject.Parse(html);
            if (json["data"].First["code"].ToString() != "200")
            {
                return GetLostUrl(id, quality);
            }
            var link = json["data"].First["url"].ToString();
            return string.IsNullOrEmpty(link) || link == "null" ? "" : link;
        }

        #endregion

        #region 旧版API

        private const string WyCookie =
            "__remember_me=true; MUSIC_U=c53e8fa763502bebd61488daaeb8d023c24f26a510528dd47e6d28bcb50e1191537eb7bb353d336fa0bd216452aa999b29453cbd46c07c3cbf122d59fa1ed6a2; __csrf=ae0c8a4af8e070d4c68d2c631501052c; os=WP; appver=1.2.2; deviceId=PByT0lnU4lzRlgzmqYWxZCbHHoE=; osver=Microsoft+Windows+NT+10.0.13067.0";

        private static string GetLostUrl(string id, string quality)
        {
            var singleSong = SearchSingle(id);
            var html = CommonHelper.GetHtmlContent("http://music.163.com/api/album/" + singleSong.AlbumId, 4,
                new Dictionary<string, string>
                {
                    {"Cookie", WyCookie}
                });
            if (string.IsNullOrEmpty(html) || html == "null")
            {
                return null;
            }
            var json = JObject.Parse(html);
            var datas = json["album"]["songs"];
            var link = "";
            foreach (JToken song in datas.Where(song => song["id"].ToString() == id))
            {
                switch (quality)
                {
                    case "320000":
                        string dfsId;
                        if (song["hMusic"].Type == JTokenType.Null)
                        {
                            if (song["mMusic"].Type == JTokenType.Null)
                            {
                                return song["mp3Url"]?.ToString();
                            }
                            dfsId = song["mMusic"]["dfsId"]?.ToString();
                        }
                        else
                        {
                            dfsId = song["hMusic"]["dfsId"]?.ToString();
                        }
                        link = GetUrlBySid(dfsId);
                        break;
                    case "192000":
                        link = song["mMusic"].Type == JTokenType.Null ? song["mp3Url"]?.ToString() : GetUrlBySid(song["mMusic"]["dfsId"]?.ToString());
                        break;
                    default:
                        link = song["mp3Url"]?.ToString();
                        break;
                }
            }
            return string.IsNullOrEmpty(link) ? GetLostUrlByPid(id, quality) : link;
        }

        private static string GetLostUrlByPid(string id, string quality)
        {
            var text = "{\"songid\":\"" + id + "\",\"offset\":0,\"limit\":10,\"total\":true}";
            var html = GetEncHtml("http://music.163.com/weapi/discovery/simiPlaylist", text);
            if (string.IsNullOrEmpty(html) || html == "null")
            {
                return null;
            }
            var json = JObject.Parse(html);
            foreach (JToken jToken in json["playlists"])
            {
                var pid = jToken["id"].ToString();
                var url = "http://music.163.com/api/playlist/detail?id=" + pid;
                html = CommonHelper.GetHtmlContent(url, 0, new Dictionary<string, string>
                {
                    {"Cookie", WyNewCookie}
                });
                if (string.IsNullOrEmpty(html))
                {
                    return null;
                }
                var total = JObject.Parse(html);
                var datas = total["result"]["tracks"];
                var song = datas.SingleOrDefault(t => t["id"].ToString() == id);
                if (song == null)
                {
                    continue;
                }
                switch (quality)
                {
                    case "320000":
                        string dfsId;
                        if (song["hMusic"].Type == JTokenType.Null)
                        {
                            if (song["mMusic"].Type == JTokenType.Null)
                            {
                                return song["mp3Url"]?.ToString();
                            }
                            dfsId = song["mMusic"]["dfsId"]?.ToString();
                        }
                        else
                        {
                            dfsId = song["hMusic"]["dfsId"]?.ToString();
                        }
                        return GetUrlBySid(dfsId);
                    case "192000":
                        if (song["mMusic"].Type == JTokenType.Null)
                        {
                            return song["mp3Url"]?.ToString();
                        }
                        return GetUrlBySid(song["mMusic"]["dfsId"]?.ToString());
                    default:
                        return song["mp3Url"]?.ToString();
                }
            }
            return null;
        }

        private static string GetUrlBySid(string dfsId)
        {
            var encryptPath = EncryptId(dfsId);
            var url = $"http://m2.music.126.net/{encryptPath}/{dfsId}.mp3";
            return url;
        }

        private static string GetPic(string id)
        {
            var html = CommonHelper.GetHtmlContent("http://music.163.com/api/song/detail/?ids=%5B" + id + "%5D", 0,
                new Dictionary<string, string>
                {
                    {"Cookie", WyNewCookie}
                });
            return string.IsNullOrEmpty(html) ? null : JObject.Parse(html)["songs"].First["album"]["blurPicUrl"].ToString();
        }

        private static string GetLrc(string sid, string type = "lrc")
        {
            var url = "http://music.163.com/api/song/lyric?os=pc&id=" + sid + "&lv=-1&kv=-1&tv=-1";
            var html = CommonHelper.GetHtmlContent(url, 4, new Dictionary<string, string>
            {
                {"Cookie", WyCookie}
            });
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            if (html.Contains("uncollected"))
            {
                return null;
            }
            var json = JObject.Parse(html);
            var text = "";
            switch (type)
            {
                case "krc":
                    if (json["klyric"]?["lyric"] != null)
                    {
                        text = json["klyric"]["lyric"].Value<string>();
                    }
                    break;
                case "trc":
                    if (json["tlyric"]?["lyric"] != null)
                    {
                        text = json["tlyric"]["lyric"].Value<string>();
                    }
                    break;
                default:
                    if (json["lrc"]?["lyric"] != null)
                    {
                        text = json["lrc"]["lyric"].Value<string>();
                    }
                    break;
            }
            return text;
        }

        private static string GetMvUrl(string mid, string quality = "hd")
        {
            var url = "http://music.163.com/api/song/mv?id=" + mid + "&type=mp4";
            var html = CommonHelper.GetHtmlContent(url, 4, new Dictionary<string, string>
            {
                {"Cookie", WyCookie}
            });
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            var json = JObject.Parse(html);
            var dic = new Dictionary<int, string>();
            var max = 0;
            foreach (JToken jToken in json["mvs"])
            {
                if (Convert.ToInt32(jToken["br"]) > max)
                {
                    max = Convert.ToInt32(jToken["br"]);
                }
                dic.Add(Convert.ToInt32(jToken["br"]), jToken["mvurl"].ToString());
            }
            if (quality != "ld")
            {
                return dic[max];
            }
            switch (max)
            {
                case 1080:
                    return dic[720];
                case 720:
                    return dic[480];
                case 480:
                    return dic.ContainsKey(320) ? dic[320] : dic[240];
                default:
                    return dic[240];
            }
        }

        private static string EncryptId(string dfsId)
        {
            var encoding = new ASCIIEncoding();
            var bytes1 = encoding.GetBytes("3go8&$8*3*3h0k(2)2");
            var bytes2 = encoding.GetBytes(dfsId);
            for (var i = 0; i < bytes2.Length; i++)
                bytes2[i] = (byte)(bytes2[i] ^ bytes1[i % bytes1.Length]);
            using (var md5Hash = MD5.Create())
            {
                var res = Convert.ToBase64String(md5Hash.ComputeHash(bytes2));
                res = res.Replace('/', '_').Replace('+', '-');
                return res;
            }
        }
        #endregion

        public SearchResult SongSearch(string key, int page, int size)
        {
            return Search(key, page, size);
        }

        public AlbumResult AlbumSearch(string id)
        {
            return SearchAlbum(id);
        }

        public ArtistResult ArtistSearch(string id, int page, int size)
        {
            return SearchArtist(id);
        }

        public CollectResult CollectSearch(string id, int page, int size)
        {
            return SearchCollect(id, page, size);
        }

        public SongResult GetSingleSong(string id, bool isDetials = false)
        {
            return SearchSingle(id);
        }

        public string GetSongUrl(string id, string quality, string format)
        {
            return GetUrl(id,quality,format);
        }
    }
}