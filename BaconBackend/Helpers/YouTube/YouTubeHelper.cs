using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BaconBackend.Managers;
using Newtonsoft.Json;

namespace BaconBackend.Helpers.YouTube
{
    public static class YouTubeHelper
    {
        private const string BotUserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
        private const string GetInfoUrl = "https://www.youtube.com/get_video_info?video_id=";
        private const string EmbedUrl = "https://www.youtube.com/embed/";

        public static async Task<YouTubeVideoInfo> GetVideoInfoAsync(string youTubeId)
        {
            throw new Exception("deprecated");

            var response = await HttpGetAsync($"{GetInfoUrl}{youTubeId}");
            if (string.IsNullOrWhiteSpace(response))
            {
                throw new Exception($"Could not find video info for youtube id {youTubeId}");
            }

            var segments = response.Split('&');
            var playerResponse = string.Empty;

            foreach (var segment in segments)
            {
                if(string.IsNullOrWhiteSpace(segment)) continue;
                if (!segment.StartsWith("player_response")) continue;
                playerResponse = segment.Replace("player_response=", "");
                playerResponse = WebUtility.UrlDecode(Regex.Unescape(playerResponse));
                break;
            }

            if (string.IsNullOrWhiteSpace(playerResponse))
            {
                throw new Exception($"Could not find video info for youtube id {youTubeId}");
            }

            return JsonConvert.DeserializeObject<YouTubeVideoInfo>(playerResponse);
        }

        public static string GetYoutubeId(string sourceUrl)
        {
            // Try to find the ID
            var youtubeVideoId = string.Empty;
            var urlLower = sourceUrl.ToLower();
            if (urlLower.Contains("youtube.com"))
            {
                // Check for an attribution link
                var attribution = urlLower.IndexOf("attribution_link?", StringComparison.OrdinalIgnoreCase);
                if (attribution != -1)
                {
                    // We need to parse out the video id
                    // looks like this attribution_link?a=bhvqtDGQD6s&amp;u=%2Fwatch%3Fv%3DrK0D1ehO7CA%26feature%3Dshare
                    var uIndex = urlLower.IndexOf("u=", attribution, StringComparison.OrdinalIgnoreCase);
                    var encodedUrl = sourceUrl.Substring(uIndex + 2);
                    var decodedUrl = WebUtility.UrlDecode(encodedUrl);
                    urlLower = decodedUrl.ToLower();
                    // At this point urlLower should be something like "v=jfkldfjl&feature=share"
                }

                var beginId = urlLower.IndexOf("v=", StringComparison.OrdinalIgnoreCase);
                var endId = urlLower.IndexOf("&", beginId, StringComparison.OrdinalIgnoreCase);
                if (beginId == -1) return youtubeVideoId;
                if (endId == -1)
                {
                    endId = urlLower.Length;
                }
                // Important! Since this might be case sensitive use the original url!
                beginId += 2;
                youtubeVideoId = sourceUrl.Substring(beginId, endId - beginId);
            }
            else if (urlLower.Contains("youtu.be"))
            {
                var domain = urlLower.IndexOf("youtu.be", StringComparison.OrdinalIgnoreCase);
                var beginId = urlLower.IndexOf("/", domain, StringComparison.OrdinalIgnoreCase);
                var endId = urlLower.IndexOf("?", beginId, StringComparison.OrdinalIgnoreCase);
                // If we can't find a ? search for a &
                if (endId == -1)
                {
                    endId = urlLower.IndexOf("&", beginId, StringComparison.OrdinalIgnoreCase);
                }

                if (beginId == -1) return youtubeVideoId;

                if (endId == -1)
                {
                    endId = urlLower.Length;
                }
                // Important! Since this might be case sensitive use the original url!
                beginId++;
                youtubeVideoId = sourceUrl.Substring(beginId, endId - beginId);
            }

            return youtubeVideoId;
        }

        private static async Task<string> HttpGetAsync(string uri)
        {
            var response = await NetworkManager.MakeGetRequest(uri, string.Empty, BotUserAgent);
            return await response.ReadAsStringAsync();
        }
    }
}