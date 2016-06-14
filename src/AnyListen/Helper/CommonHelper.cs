using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using NLog;

namespace AnyListen.Helper
{
    public class CommonHelper
    {
        private static readonly Logger MyLogger = LogManager.GetLogger("WebError");

        public static void AddLog(Exception ex)
        {
            MyLogger.Error(ex.ToString);
        }

        public static string GetHtmlContent(string url, int userAgent = 0, Dictionary<string,string> headers = null)
        {
            try
            {
                var myHttpWebRequest = new HttpClient { Timeout = new TimeSpan(0, 0, 5) };
                myHttpWebRequest.DefaultRequestHeaders.Add("Method", "GET");
                myHttpWebRequest.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                switch (userAgent)
                {
                    case 1:
                        myHttpWebRequest.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 5_0 like Mac OS X) AppleWebKit/534.46 (KHTML, like Gecko) Version/5.1 Mobile/9A334 Safari/7534.48.3");
                        break;
                    case 2:
                        myHttpWebRequest.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 4.0.4; Galaxy Nexus Build/IMM76B) AppleWebKit/535.19 (KHTML, like Gecko) Chrome/18.0.1025.133 Mobile Safari/535.19");
                        break;
                    case 3:
                        myHttpWebRequest.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows Phone 8.0; Trident/6.0; IEMobile/10.0; ARM; Touch; NOKIA; Lumia 920)");
                        break;
                    case 4:
                        myHttpWebRequest.DefaultRequestHeaders.Add("User-Agent", "NativeHost");
                        break;
                    default:
                        myHttpWebRequest.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.101 Safari/537.36");
                        break;
                }
                if (headers != null)
                {
                    foreach (var k in headers)
                    {
                        myHttpWebRequest.DefaultRequestHeaders.Add(k.Key, k.Value);
                    }
                }
                var result = myHttpWebRequest.GetStringAsync(url).Result;
                return result;
            }
            catch (Exception ex)
            {
                AddLog(ex);
                return null;
            }
        }

        public static string PostData(string url,Dictionary<string,string> data, int contentType = 0, int userAgent = 0, Dictionary<string, string> headers = null)
        {
            try
            {
                var myHttpWebRequest = new HttpClient { Timeout = new TimeSpan(0, 0, 5) };
                myHttpWebRequest.DefaultRequestHeaders.Add("Method", "POST");
                myHttpWebRequest.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                myHttpWebRequest.DefaultRequestHeaders.Add("ContentType",
                    userAgent == 0 ? "application/x-www-form-urlencoded" : "application/json;charset=UTF-8");
                switch (userAgent)
                {
                    case 1:
                        myHttpWebRequest.DefaultRequestHeaders.Add("UserAgent", "Mozilla/5.0 (iPhone; CPU iPhone OS 5_0 like Mac OS X) AppleWebKit/534.46 (KHTML, like Gecko) Version/5.1 Mobile/9A334 Safari/7534.48.3");
                        break;
                    case 2:
                        myHttpWebRequest.DefaultRequestHeaders.Add("UserAgent", "Mozilla/5.0 (Linux; Android 4.0.4; Galaxy Nexus Build/IMM76B) AppleWebKit/535.19 (KHTML, like Gecko) Chrome/18.0.1025.133 Mobile Safari/535.19");
                        break;
                    case 3:
                        myHttpWebRequest.DefaultRequestHeaders.Add("UserAgent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows Phone 8.0; Trident/6.0; IEMobile/10.0; ARM; Touch; NOKIA; Lumia 920)");
                        break;
                    case 4:
                        myHttpWebRequest.DefaultRequestHeaders.Add("UserAgent", "NativeHost");
                        break;
                    default:
                        myHttpWebRequest.DefaultRequestHeaders.Add("UserAgent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.101 Safari/537.36");
                        break;
                }
                if (headers != null)
                {
                    foreach (var k in headers)
                    {
                        myHttpWebRequest.DefaultRequestHeaders.Add(k.Key, k.Value);
                    }
                }
                var response = myHttpWebRequest.PostAsync(url,new FormUrlEncodedContent(data)).Result;
                var result = response.Content.ReadAsStringAsync().Result;
                return result;
            }
            catch (Exception ex)
            {
                AddLog(ex);
                return null;
            }
        }

        public static string GetSongUrl(string type, string quality, string id, string format)
        {
            var key = type + "_" + quality + "_" + id + "." + format;
            var md5 = Md5(key + "$$itwusun.com$$");
            return "http://yourdomain/api/music/" + key + "?sign=" + md5;   //需要将yourdomain替换成IP或者域名
        }

        public static string Md5(string input)
        {
            var strs = Encoding.UTF8.GetBytes(input);
            var md5 = MD5.Create();
            var output = md5.ComputeHash(strs);
            return BitConverter.ToString(output).Replace("-", "").ToLower();
        }

        public static string NumToTime(string originalTime)
        {
            if (originalTime.Contains("."))
            {
                originalTime = originalTime.Split('.')[0].Trim();
            }
            if (string.IsNullOrEmpty(originalTime))
            {
                return "00:00";
            }
            var num = Convert.ToInt32(originalTime);
            var mins = num / 60;
            var seds = num % 60;
            var time = mins.ToString().PadLeft(2, '0') + ":" + seds.ToString().PadLeft(2, '0');
            return time;
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <param name="isMills"></param>
        /// <returns></returns>
        public static long GetTimeSpan(bool isMills = false)
        {
            var startTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1, 0, 0, 0, 0), TimeZoneInfo.Local);
            return Convert.ToInt64((DateTime.Now.Ticks - startTime.Ticks) / (isMills ? 10000 : 10000000));
        }

        /// <summary>
        /// unix时间戳转换成日期
        /// </summary>
        /// <param name="timestamp">时间戳（秒）</param>
        /// <returns></returns>
        public static DateTime UnixTimestampToDateTime(long timestamp)
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return start.AddSeconds(timestamp);
        }

        public static string GetContentType(string fileType)
        {
            switch (fileType)
            {
                case "mp3":
                    return "audio/mp3";
                case "wav":
                    return "audio/x-wav";
                case "ape":
                    return "audio/x-ape";
                case "flac":
                    return "audio/x-flac";
                case "mp4":
                    return "video/mp4";
                case "flv":
                    return "video/x-flv";
                case "wmv":
                    return "video/x-ms-wmv";
                case "lrc":
                case "trc":
                case "krc":
                    return "text/plain";
                case "jpg":
                    return "image/jpeg";
                default:
                    return "application/json";
            }
        }
    }
}