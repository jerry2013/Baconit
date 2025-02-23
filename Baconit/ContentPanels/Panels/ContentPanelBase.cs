﻿using Baconit.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using BaconBackend.Managers;

namespace Baconit.ContentPanels.Panels
{
    internal class ContentPanelBase : IContentPanelBase, IContentPanelBaseInternal
    {
        ///
        /// Common Vars
        /// 
        
        /// <summary>
        /// If we are loading or not.
        /// </summary>
        public bool IsLoading { get; private set; } = true;

        /// <summary>
        /// If we are in error or not.
        /// </summary>
        public bool HasError { get; private set; }

        /// <summary>
        /// The text of the error if we have one.
        /// </summary>
        public string ErrorText { get; private set; }

        /// <summary>
        /// Holds a reference to the source we are currently showing.
        /// </summary>
        public ContentPanelSource Source { get; private set; }

        /// <summary>
        /// Indicates if the control is destroyed.
        /// </summary>
        public bool IsDestroyed { get; private set; } = false;

        /// <summary>
        /// The actual panel contained within.
        /// </summary>
        public IContentPanel Panel { get; private set; }

        //
        // Private vars
        //

        /// <summary>
        /// Holds the current panel host.
        /// </summary>
        private IContentPanelHost _host;

        /// <summary>
        /// Indicates if we have told the master we are done loading.
        /// </summary>
        private bool _hasDeclaredLoaded;

        #region IContentPanelBaseInternal

        /// <summary>
        /// Indicates if we are full screen.
        /// </summary>
        public bool IsFullscreen {
            get
            {
                var host = _host;
                return host != null && host.IsFullscreen;
            }
        }

        /// <summary>
        /// Indicates if we can go full screen.
        /// </summary>
        public bool CanGoFullscreen
        {
            get
            {
                var host = _host;
                if (host != null)
                {
                    return host.CanGoFullscreen;
                }
                return false;
            }
        }

        #region Fire Events

        /// <summary>
        /// Fires toggle loading.
        /// </summary>
        /// <param name="isLoading"></param>
        public void FireOnLoading(bool isLoading)
        {
            // If is the same leave.
            if(isLoading == IsLoading)
            {
                return;
            }

            // Set the value
            IsLoading = isLoading;

            // Try to tell the host
            var host = _host;
            host?.OnLoadingChanged();

            // When loading is done and we haven't before report it to the master
            if (!_hasDeclaredLoaded && !IsLoading)
            {
                _hasDeclaredLoaded = true;
                // Tell the manager that we are loaded.
                Task.Run(() =>
                {
                    IsLoading = false;
                    ContentPanelMaster.Current.OnContentLoadComplete(Source.Id);
                });
            }
        }

        /// <summary>
        /// Fires show error
        /// </summary>
        /// <param name="hasError"></param>
        /// <param name="errorText"></param>
        public void FireOnError(bool hasError, string errorText = null)
        {
            // Set the value
            HasError = hasError;
            ErrorText = errorText;

            // Try to tell the host
            var host = _host;
            host?.OnErrorChanged();

            // When loading is done report it to the master
            if (!_hasDeclaredLoaded && HasError)
            {
                _hasDeclaredLoaded = true;
                // Tell the manager that we are loaded.
                Task.Run(() =>
                {
                    ContentPanelMaster.Current.OnContentLoadComplete(Source.Id);
                });
            }
        }

        /// <summary>
        /// Fires ToggleFullscreen
        /// </summary>
        /// <param name="goFullscreen"></param>
        public bool FireOnFullscreenChanged(bool goFullscreen)
        {
            // Try to tell the host
            var host = _host;
            if (host != null)
            {
                return host.OnFullscreenChanged(goFullscreen);
            }
            return false;
        }

        /// <summary>
        /// Tells the content manager to show this as a web page instead of
        /// the current control.
        /// </summary>
        public void FireOnFallbackToBrowser()
        {
            Task.Run(() =>
            {
                ContentPanelMaster.Current.FallbackToWebrowser(Source);
            });
        }

        #endregion

        #endregion

        #region IContentPanelBase

        /// <summary>
        /// Fired when this post becomes visible
        /// </summary>
        public void OnVisibilityChanged(bool isVisible)
        {
            Panel.OnVisibilityChanged(isVisible);
        }

        /// <summary>
        /// Fired when we should destroy our content.
        /// </summary>
        public void OnDestroyContent()
        {
            // Make sure the panel isn't null, in the case where we 
            // can't load the panel we will be be destroyed before
            // the base is nulled.
            Panel?.OnDestroyContent();
        }

        /// <summary>
        /// Fired when a new host has been added.
        /// </summary>
        /// <param name="host"></param>
        public void OnHostAdded(IContentPanelHost host)
        {
            _host = host;
            Panel.OnHostAdded();

            // Also fire on visibility changed so the panel is in the correct state
            Panel.OnVisibilityChanged(host.IsVisible);
        }

        /// <summary>
        /// Fired when the host is removed.
        /// </summary>
        public void OnHostRemoved()
        {
            _host = null;
        }

        /// <summary>
        /// Indicates how large the current panel is.
        /// </summary>
        public PanelMemorySizes PanelMemorySize
        {
            get
            {
                if(Panel != null)
                {
                    return Panel.PanelMemorySize;
                }
                return PanelMemorySizes.Small;
            }
        }

        #endregion

        #region Create Control

        public static Type GetControlType(ContentPanelSource source, object callingClass = null)
        {
            try
            {
                if (GifImageContentPanel.CanHandlePost(source))
                {
                    return typeof(GifImageContentPanel);
                }
                if (RedditVideoContentPanel.CanHandlePost(source))
                {
                    return typeof(RedditVideoContentPanel);
                }
                if (YoutubeContentPanel.CanHandlePost(source))
                {
                    return typeof(YoutubeContentPanel);
                }
                if (BasicImageContentPanel.CanHandlePost(source))
                {
                    return typeof(BasicImageContentPanel);
                }
                if (MarkdownContentPanel.CanHandlePost(source))
                {
                    return typeof(MarkdownContentPanel);
                }
                if (RedditContentPanel.CanHandlePost(source))
                {
                    return typeof(RedditContentPanel);
                }
                if (CommentSpoilerContentPanel.CanHandlePost(source))
                {
                    return typeof(CommentSpoilerContentPanel);
                }
                if (WindowsAppContentPanel.CanHandlePost(source))
                {
                    return typeof(WindowsAppContentPanel);
                }
            }
            catch (Exception e)
            {
                // If we fail here we will fall back to the web browser.
                App.BaconMan.MessageMan.DebugDia("Failed to query can handle post", e);
                TelemetryManager.ReportUnexpectedEvent(callingClass, "FailedToQueryCanHandlePost", e);
            }

            return typeof(WebPageContentPanel);
        }

        public async Task<bool> CreateContentPanel(ContentPanelSource source, bool canLoadLargePanels)
        {
            // Indicates if the panel was loaded.
            var loadedPanel = true;

            // Capture the source 
            Source = source;

            // We default to web page
            var controlType = typeof(WebPageContentPanel);

            // If we are not forcing web find the control type.
            if (!source.ForceWeb)
            {
                // Try to figure out the type.
                controlType = GetControlType(source, this);
                if (controlType == typeof(WebPageContentPanel)) {
                    // This is a web browser

                    // If we are blocking large panels don't allow the
                    // browser.
                    if (!canLoadLargePanels)
                    {
                        loadedPanel = false;
                    }
                }
            }

            // Check if we should still load.
            if (loadedPanel)
            {
                // Make the control on the UI thread.
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    try
                    {
                        // Create the panel
                        Panel = (IContentPanel)Activator.CreateInstance(controlType, this);

#if DEBUG
                        Debug.WriteLine($"Content Panel: {controlType.Name}");
                        Debug.WriteLine($"Source: {source}");
#endif

                        // Fire OnPrepareContent 
                        Panel.OnPrepareContent();
                    }
                    catch (Exception e)
                    {
                        loadedPanel = false;
                        HasError = true;
                        App.BaconMan.MessageMan.DebugDia("failed to create content control", e);
                        TelemetryManager.ReportUnexpectedEvent(this, "FailedToCreateContentPanel", e);
                    }
                });
            }

            // Indicate that we have loaded.
            return loadedPanel;
        }

        #endregion
    }
}
