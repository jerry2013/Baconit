using System;
using System.Text.RegularExpressions;
using BaconBackend.Helpers.YouTube;
using Newtonsoft.Json;

namespace BaconBackend.Helpers.RES
{
    public static class ResHelper
    {
        public static Uri GetVideoInfoAsync(Uri sourceUri)
        {
            var youtubeId = YouTubeHelper.GetYoutubeId(sourceUri.AbsoluteUri);
            if (youtubeId.Length > 0)
            {
                return new Uri($"https://www.youtube.com/embed/{youtubeId}?feature=oembed&version=3&rel=0", UriKind.Absolute);
            }
            else if (sourceUri.Host == "dailymotion.com")
            {
                var match = Regex.Match(sourceUri.AbsoluteUri, @"^https?://(?:(?:www|touch)\.)?dailymotion.com[\\w\-/:#]+video[/=]([a-z0-9]+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return new Uri($"https://www.dailymotion.com/embed/video/{match.Value}?api=postMessage", UriKind.Absolute);
                }
            }
            else if (sourceUri.Host == "vimeo.com")
            {
                var match = Regex.Match(sourceUri.AbsolutePath, @"^/([0-9]+)(?:/|$)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return new Uri($"https://player.vimeo.com/video/{match.Value}", UriKind.Absolute);
                }
            }

            return null;
        }

        public class StreamableInfo
        {
            [JsonProperty("title")]
            public string Title { get; set; }
            [JsonProperty("files")]
            public StreamableFiles Files { get; set; }
            [JsonProperty("source")]
            public string Source { get; set; }
        }

        public class StreamableFiles {
            [JsonProperty("mp4")]
            public StreamableMp4 MP4 { get; set; }
        }

        public class StreamableMp4
        {
            [JsonProperty("url")]
            public string Url { get; set; }
        }
    }
}
