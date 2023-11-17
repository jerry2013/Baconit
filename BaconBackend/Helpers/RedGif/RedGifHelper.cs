using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BaconBackend.Managers;
using Newtonsoft.Json;

namespace BaconBackend.Helpers.RedGif
{
    public static class RedGifHelper
    {
        private const string GetInfoUrl = "https://redgifs.com/ifr/";
        private static readonly Regex rx = new Regex(@"(?<url>[^'""]+.mp4[^'""]*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static async Task<RedGifInfo> GetVideoInfoAsync(string gifId)
        {
            var response = await HttpGetAsync($"{GetInfoUrl}{gifId}");
            if (string.IsNullOrWhiteSpace(response))
            {
                throw new Exception($"Could not find video info for redgif id {gifId}");
            }
            var url = response.Replace("-silent.mp4", ".mp4");
            return new RedGifInfo
            {
                DataInfo = new DataInfo
                {
                    Id = gifId,
                    VideoInfo = new VideoInfo
                    {
                        StandardDefUrl = url,
                        HighDefUrl = url
                    }
                }
            };
        }
        
        private static async Task<string> HttpGetAsync(string uri)
        {
            using (var response = await NetworkManager.MakeGetRequest(uri, string.Empty))
            {
                var content = await response.ReadAsStringAsync();

                var matches = rx.Matches(content);
                if (matches.Count > 0)
                {
                    return matches[0].Groups["url"].Value;
                }
            }

            return String.Empty;
        }
    }

    public class RedGifInfo
    {
        [JsonProperty("gif")]
        public DataInfo DataInfo { get; set; }
    }

    public class DataInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("urls")]
        public VideoInfo VideoInfo { get; set; }
    }

    public class VideoInfo
    {
        [JsonProperty("sd")]
        public string StandardDefUrl { get; set; }

        [JsonProperty("hd")]
        public string HighDefUrl { get; set; }
    }
}
