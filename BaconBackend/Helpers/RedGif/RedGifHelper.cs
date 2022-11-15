using System;
using System.Threading.Tasks;
using BaconBackend.Managers;
using Newtonsoft.Json;

namespace BaconBackend.Helpers.RedGif
{
    public static class RedGifHelper
    {
        private const string GetAgentString = "https://api.redgifs.com/info";
        public const string BotUserAgent = "NSPlayer/12.00.15254.0603 WMFSDK/12.00.15254.0603";
        private const string GetInfoUrl = "https://api.redgifs.com/v2/gifs/";

        public static async Task<RedGifInfo> GetVideoInfoAsync(string gifId)
        {
            var response = await HttpGetAsync($"{GetInfoUrl}{gifId}");
            if (string.IsNullOrWhiteSpace(response))
            {
                throw new Exception($"Could not find video info for redgif id {gifId}");
            }
            var data = JsonConvert.DeserializeObject<RedGifInfo>(response);
            return data;
        }
        
        private static async Task<string> HttpGetAsync(string uri)
        {
            var response = await NetworkManager.MakeGetRequest(uri, string.Empty, BotUserAgent);
            return await response.ReadAsStringAsync();
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
