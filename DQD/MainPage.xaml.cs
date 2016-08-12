using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using DQD.Net.Pages;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;
using Windows.System.Profile;
using DQD.Core.Tools;

namespace DQD.Net {
    /// <summary>
    /// MainPage Code Page
    /// </summary>
    public sealed partial class MainPage:Page {
        public static MainPage Current;
        public Frame contentFrame;
        public Grid baseGrid;
        public delegate void NavigateEventHandler(object sender,Type type,Frame frame);
        public delegate void ClickEventHandler(object sender, Type type, Frame frame,Uri uri);
        private NavigateEventHandler SelectionChanged = (sender,type,frame) => { frame.Navigate(type); };
        public ClickEventHandler ItemClick = (sender, type, frame, uri) => { frame.Navigate(type,uri); };

        public MainPage() {
            Current=this;
            this.InitializeComponent();
            contentFrame = this.ContentFrame;
            baseGrid = this.BaseGrid;
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
            StatusBarInit.InitDesktopStatusBar(RequestedTheme != ElementTheme.Dark);
        }

        /// <summary>
        /// When pivot item changed, show the right page by agent event.
        /// </summary>
        /// <param name="sender">pivot</param>
        /// <param name="e">changedArgs</param>
        private void RootPivot_SelectionChanged(object sender,SelectionChangedEventArgs e) {
            var item = ((sender as Pivot).SelectedItem as PivotItem).Name;
            SelectionChanged?.Invoke(this,InsideResources.GetTPageype(item),InsideResources.GetTFrameype(item));
        }

        #region Static Inside Class
        /// <summary>
        /// Page inside resources of navigating and choosing frames.
        /// </summary>
        static class InsideResources {
            /// <summary>
            /// The dictionary map of pivotItems names collection. 
            /// </summary>
            static public  Dictionary<string,Type> PagesMaps = new Dictionary<string,Type> {
            {"HomePItem",typeof(HomePage)},
            {"MatchPItem",typeof(MatchPage)},
            {"VideoPItem",typeof(VideoPage)},
            {"DataPItem",typeof(DataPage)},
        };

            /// <summary>
            /// The dictionary map of frames collection. 
            /// </summary>
            static private Dictionary<string,Frame> FramesMaps = new Dictionary<string,Frame> {
            {"HomePItem",Current.HomePFrame},
            {"MatchPItem",Current.MatchPFrame},
            {"VideoPItem",Current.VideoPFrame},
            {"DataPItem",Current.DataPFrame},
        };

            /// <summary>
            /// Get target page type by typeof.
            /// </summary>
            /// <param name="str">item name</param>
            /// <returns></returns>
            public static Type GetTPageype(string str) { return PagesMaps.ContainsKey(str) ? PagesMaps[str] : null; }

            /// <summary>
            /// Get target frame by item name.
            /// </summary>
            /// <param name="str">item name</param>
            /// <returns></returns>
            public static Frame GetTFrameype(string str) { return FramesMaps.ContainsKey(str) ? FramesMaps[str] : null; }
        }
        #endregion

        /// <summary>
        /// OnBackRequested
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackRequested(object sender, BackRequestedEventArgs e) {
            if (AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Mobile"))
                BaseGrid.Visibility = Visibility.Visible;
            e.Handled = true;
        }

        private void grid_SizeChanged(object sender, SizeChangedEventArgs e) {
            var TargetWidth = (sender as Grid).ActualWidth;
            if (TargetWidth > 800) {
                BaseGrid.Visibility = Visibility.Visible;
            } else {
                //if (AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Mobile") && TargetWidth < 600 ) {
                //    if (ContentFrame.Content == null) { BaseGrid.Visibility = Visibility.Visible; } else { BaseGrid.Visibility = Visibility.Collapsed; }
                //    return;
                //}
                if (ContentFrame.Content == null) { BaseGrid.Visibility = Visibility.Visible; } else { BaseGrid.Visibility = Visibility.Collapsed; }
            }
        }

        private void BaseGrid_SizeChanged(object sender,SizeChangedEventArgs e) {
            RootPivot.HeaderWidth=(sender as Grid).ActualWidth/4;
        }

        private void ThemeModeBtn_Click(object sender, RoutedEventArgs e) {
            if (RequestedTheme == ElementTheme.Dark) {
                (sender as Button).Content = char.ConvertFromUtf32(0xEC46);
                RequestedTheme = ElementTheme.Light;
                StatusBarInit.InitDesktopStatusBar(RequestedTheme != ElementTheme.Dark);
            }
            else {
                (sender as Button).Content = char.ConvertFromUtf32(0xEC8A);
                RequestedTheme = ElementTheme.Dark;
                StatusBarInit.InitDesktopStatusBar(RequestedTheme != ElementTheme.Dark);
            }
        }
    }
}
