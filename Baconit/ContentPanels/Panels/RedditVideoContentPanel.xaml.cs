using System.Threading.Tasks;
using Baconit.Interfaces;

namespace Baconit.ContentPanels.Panels
{
    public sealed partial class RedditVideoContentPanel : IContentPanel
    {
        private readonly IContentPanelBaseInternal _contentPanelBase;

        public PanelMemorySizes PanelMemorySize => PanelMemorySizes.Medium;

        public RedditVideoContentPanel(IContentPanelBaseInternal contentPanelBase)
        {
            InitializeComponent();
            _contentPanelBase = contentPanelBase;
            
        }

        public static bool CanHandlePost(ContentPanelSource source)
        {
            return source.IsRedditVideo && !string.IsNullOrWhiteSpace(source.VideoUrl.AbsoluteUri);
        }

        public void OnPrepareContent()
        {
            var source = _contentPanelBase.Source;

            Task.Run(async () =>
            {
                await _contentPanelBase.CreateVideoPlayerAsync(
                    () => source.VideoUrl,
                    (player) => ui_contentRoot.Children.Add(player)
                );
            });
        }

        public void OnVisibilityChanged(bool isVisible)
        {
            _contentPanelBase.ToggleVideoPlayer(isVisible);
        }

        public void OnDestroyContent()
        {
            _contentPanelBase.DestroyVideoPlayer();
            ui_contentRoot.Children.Clear();
        }

        public void OnHostAdded()
        {
            // Ignore for now.
        }
    }
}
