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

namespace DQD.Net {
    /// <summary>
    /// MainPage Code Page
    /// </summary>
    public sealed partial class MainPage:Page {
        public static MainPage Current;
        public delegate void NavigateEventHandler(object sender,Type type,Frame frame);
        private NavigateEventHandler SelectionChanged = (sender,type,frame) => { frame.Navigate(type); };

        public MainPage() {
            Current=this;
            this.InitializeComponent();
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

        private void Grid_SizeChanged(object sender,SizeChangedEventArgs e) {
            RootPivot.HeaderWidth=(sender as Grid).ActualWidth/4;
        }
    }
}
