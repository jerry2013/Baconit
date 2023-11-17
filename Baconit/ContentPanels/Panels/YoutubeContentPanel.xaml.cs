using Baconit.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using BaconBackend.Helpers.YouTube;
using BaconBackend.Managers;

namespace Baconit.ContentPanels.Panels
{
    public sealed partial class YoutubeContentPanel : IContentPanel
    {
        /// <summary>
        /// Holds a reference to our base.
        /// </summary>
        private readonly IContentPanelBaseInternal _contentPanelBase;

        public YoutubeContentPanel(IContentPanelBaseInternal panelBase)
        {
            InitializeComponent();
            _contentPanelBase = panelBase;
        }

        /// <summary>
        /// Called by the host when it queries if we can handle a post.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool CanHandlePost(ContentPanelSource source)
        {
            // Note! We can't do the full Uri get because it relays on an Internet request and
            // we can't lose the time for this quick check. If we can get the youtube id assume we are good.

            // See if we can get a link
            return !string.IsNullOrWhiteSpace(TryToGetYouTubeId(source));
        }

        #region IContentPanel

        /// <summary>
        /// Indicates how large the panel is in memory.
        /// </summary>
        // #todo can we figure this out?
        public PanelMemorySizes PanelMemorySize => PanelMemorySizes.Medium;

        /// <summary>
        /// Fired when we should load the content.
        /// </summary>
        public void OnPrepareContent()
        {
            // Since this can be costly kick it off to a background thread so we don't do work
            // as we are animating.
            Task.Run(async () =>
            {
                // Get the video Uri
                var youTubeVideoInfo = await GetYouTubeVideoInfoAsync(_contentPanelBase.Source);
                var youtubeUrl = GetYouTubeUrl(youTubeVideoInfo);

                await _contentPanelBase.CreateVideoPlayerAsync(
                    () =>
                    {
                        if (string.IsNullOrWhiteSpace(youtubeUrl))
                        {
                            // If we failed fallback to the browser.
                            _contentPanelBase.FireOnFallbackToBrowser();
                            TelemetryManager.ReportUnexpectedEvent(this, "FailedToGetYoutubeVideoAfterSuccess");
                            return null;
                        }
                        if (_contentPanelBase.IsDestroyed)
                        {
                            return null;
                        }
                        return new Uri(youtubeUrl);
                    },
                    (player) => ui_contentRoot.Children.Add(player)
                );
            });
        }

        /// <summary>
        /// Fired when we should destroy our content.
        /// </summary>
        public void OnDestroyContent()
        {
            _contentPanelBase.DestroyVideoPlayer();
            ui_contentRoot.Children.Clear();
        }

        /// <summary>
        /// Fired when a new host has been added.
        /// </summary>
        public void OnHostAdded()
        {
            // Ignore for now.
        }

        /// <summary>
        /// Fired when this post becomes visible
        /// </summary>
        public void OnVisibilityChanged(bool isVisible)
        {
            _contentPanelBase.ToggleVideoPlayer(isVisible);
        }

        #endregion

        #region Youtube Id

        /// <summary>
        /// Tries to get a youtube link from a post. If it fails
        /// it returns null.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static async Task<YouTubeVideoInfo> GetYouTubeVideoInfoAsync(ContentPanelSource source)
        {
            if (string.IsNullOrWhiteSpace(source.Url))
            {
                return null;
            }

            try
            {
                // Try to find the ID
                var youtubeVideoId = TryToGetYouTubeId(source);

                if (!string.IsNullOrWhiteSpace(youtubeVideoId))
                {
                    return await YouTubeHelper.GetVideoInfoAsync(youtubeVideoId);
                }
            }
            catch (Exception ex)
            {
                TelemetryManager.ReportEvent("YoutubeString", "Failed to find youtube video");
            }

            return null;
        }

        /// <summary>
        /// Attempts to get a youtube id from a url.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static string TryToGetYouTubeId(ContentPanelSource source)
        {
            if (string.IsNullOrWhiteSpace(source.Url))
            {
                return null;
            }

            try
            {
                return YouTubeHelper.GetYoutubeId(source.Url);
            }
            catch (Exception)
            {
                TelemetryManager.ReportEvent("YoutubeString", "Failed to find youtube video");
            }
            return null;
        }

        private static string GetYouTubeUrl(YouTubeVideoInfo youTubeVideoInfo)
        {
            var url = string.Empty;
            var streamingData = youTubeVideoInfo?.StreamingData;
            if(streamingData == null) return url;

            // sometimes the url comes back with no value, there is a cipher value that has a secondary URL, however there is some kind
            // of auth needed to access the url, I suspect that it's in the players JS library and it makes an auth token
            // for the player behind the scenes.
            var format = streamingData.Formats.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Url));
            if (format != null)
            {
                url = format.Url;
            }

            return url;
        }
        #endregion
    }
}
