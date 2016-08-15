using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using DQD.Net.Pages;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Core;
using Windows.System.Profile;
using DQD.Core.Tools;
using DQD.Core.Helpers;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media;
using DQD.Core.Models;

namespace DQD.Net {
    /// <summary>
    /// MainPage Code Page
    /// </summary>
    public sealed partial class MainPage:Page {
        #region Constructor

        public MainPage() {
            Current=this;
            this.InitializeComponent();
            contentFrame = this.ContentFrame;
            SideGrid = this.sideGrid;
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
            var isColorfulOrNot = (bool?)SettingsHelper.ReadSettingsValue(SettingsConstants.IsColorfulOrNot) ?? false;
            var isLightOrNot = (bool?)SettingsHelper.ReadSettingsValue(SettingsConstants.IsLigheOrNot) ?? false;
            RequestedTheme = isLightOrNot ? ElementTheme.Light : ElementTheme.Dark;
            ThemeModeBtn.Content = isLightOrNot ? char.ConvertFromUtf32(0xEC46) : char.ConvertFromUtf32(0xEC8A);
            ColorSwitch.IsOn = isColorfulOrNot;
            PrepareFrame.Navigate(typeof(PreparePage));
        }

        #endregion

        #region Events

        /// <summary>
        /// When pivot item changed, show the right page by agent event.
        /// </summary>
        /// <param name="sender">pivot</param>
        /// <param name="e">changedArgs</param>
        private void RootPivot_SelectionChanged(object sender,SelectionChangedEventArgs e) {
            var item = ((sender as Pivot).SelectedItem as PivotItem).Name;
            SelectionChanged?.Invoke(this,InsideResources.GetTPageype(item),InsideResources.GetTFrameype(item));
        }

        /// <summary>
        /// OnBackRequested
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackRequested(object sender, BackRequestedEventArgs e) {
            if (AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Mobile") ) { sideGrid.Visibility = Visibility.Collapsed; }
            e.Handled = true;
        }

        private void grid_SizeChanged(object sender, SizeChangedEventArgs e) {
            sideGrid.Visibility = (sender as Grid).ActualWidth > 800? Visibility.Visible:ContentFrame.Content == null ? Visibility.Collapsed : Visibility.Visible;
        }

        private void BaseGrid_SizeChanged(object sender,SizeChangedEventArgs e) {
            RootPivot.HeaderWidth=(sender as Grid).ActualWidth/4;
        }

        private void ThemeModeBtn_Click(object sender, RoutedEventArgs e) {
            var isColorfulOrNot = (bool?)SettingsHelper.ReadSettingsValue(SettingsConstants.IsColorfulOrNot) ?? false;
            var isLightOrNot = (bool?)SettingsHelper.ReadSettingsValue(SettingsConstants.IsLigheOrNot) ?? false;
            (sender as Button).Content = !isLightOrNot ? char.ConvertFromUtf32(0xEC46) : char.ConvertFromUtf32(0xEC8A);
            RequestedTheme = !isLightOrNot ? ElementTheme.Light : ElementTheme.Dark;
            SettingsHelper.SaveSettingsValue(SettingsConstants.IsLigheOrNot, !isLightOrNot);
            if (isColorfulOrNot) {
                StatusBarInit.InitDesktopStatusBar(!isLightOrNot);
                StatusBarInit.InitMobileStatusBar(!isLightOrNot);
            } else {
                StatusBarInit.InitDesktopStatusBarToPrepare(!isLightOrNot);
                StatusBarInit.InitMobileStatusBarToPrepare(!isLightOrNot);
            }
        }

        private void Switch_Toggled(object sender, RoutedEventArgs e) {
            var isColorfulOrNot = (sender as ToggleSwitch).IsOn;
            var isLightOrNot = (bool?)SettingsHelper.ReadSettingsValue(SettingsConstants.IsLigheOrNot) ?? false;
            SettingsHelper.SaveSettingsValue(SettingsConstants.IsColorfulOrNot, (sender as ToggleSwitch).IsOn);
            if (isColorfulOrNot) {
                StatusBarInit.InitDesktopStatusBar(isLightOrNot);
                StatusBarInit.InitInnerDesktopStatusBar(true);
                Window.Current.SetTitleBar(TitleBarRec);
                if (StatusBarInit.IsTargetMobile()) {
                    StatusBarInit.InitInnerMobileStatusBar(true);
                    BaseGrid.Margin = new Thickness(0, 16, 0, 0);
                    sideGrid.Margin = new Thickness(0, 16, 0, 0);
                }
            } else {
                StatusBarInit.InitDesktopStatusBarToPrepare(isLightOrNot);
                StatusBarInit.InitMobileStatusBarToPrepare(isLightOrNot);
                StatusBarInit.InitInnerDesktopStatusBar(false);
                if (StatusBarInit.IsTargetMobile()) {
                    StatusBarInit.InitInnerMobileStatusBar(false);
                    BaseGrid.Margin = new Thickness(0, 0, 0, 0);
                    sideGrid.Margin = new Thickness(0, 0, 0, 0);
                }
            }
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e) {
            SettingsPopup.IsOpen = true;
            PopupBorder.Visibility = Visibility.Visible;
            EnterPopupBorder.Begin();
        }

        private void SettingsPopup_SizeChanged(object sender, SizeChangedEventArgs e) {
            PopupStack.Height = (sender as Popup).ActualHeight;
        }

        private void PopupBackButton_Click(object sender, RoutedEventArgs e) {
            if (SettMenu.Visibility == Visibility.Visible) { SettMenu.Visibility = Visibility.Collapsed; return; }
            if (AboutMenu.Visibility == Visibility.Visible) { AboutMenu.Visibility = Visibility.Collapsed; return; }
            SettingsPopup.IsOpen = false;
        }

        private void PopupInnerClick(object sender, RoutedEventArgs e) {
            var button = sender as Button;
            if (button.Name.Equals("SettBtn")) {
                SettMenu.Visibility = Visibility.Visible;
                AboutMenu.Visibility = Visibility.Collapsed;
            } else {
                SettMenu.Visibility = Visibility.Collapsed;
                AboutMenu.Visibility = Visibility.Visible;
            }
        }

        private void SettingsPopup_Closed(object sender, object e) {
            OutPopupBorder.Completed += OnOutPopupBorderOut;
            OutPopupBorder.Begin();
            SettMenu.Visibility = Visibility.Collapsed;
            AboutMenu.Visibility = Visibility.Collapsed;
        }

        private void OnOutPopupBorderOut(object sender, object e) {
            OutPopupBorder.Completed -= OnOutPopupBorderOut;
            PopupBorder.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Static Inside Class
        /// <summary>
        /// Page inside resources of navigating and choosing frames.
        /// </summary>
        static class InsideResources {
            /// <summary>
            /// The dictionary map of pivotItems names collection. 
            /// </summary>
            static public Dictionary<string, Type> PagesMaps = new Dictionary<string, Type> {
            {"HomePItem",typeof(HomePage)},
            {"MatchPItem",typeof(MatchPage)},
            {"VideoPItem",typeof(VideoPage)},
            {"DataPItem",typeof(DataPage)},
        };

            /// <summary>
            /// The dictionary map of frames collection. 
            /// </summary>
            static private Dictionary<string, Frame> FramesMaps = new Dictionary<string, Frame> {
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

        #region Properties and States

        public static MainPage Current;
        public Frame contentFrame;
        public Grid SideGrid;
        public delegate void NavigateEventHandler(object sender, Type type, Frame frame);
        public delegate void ClickEventHandler(object sender, Type type, Frame frame, Uri uri ,int num);
        private NavigateEventHandler SelectionChanged = (sender, type, frame) => { frame.Navigate(type); };
        public ClickEventHandler ItemClick = (sender, type, frame, uri ,num) => { frame.Navigate(type, new ParameterNavigate { Number=num,Uri=uri,}); };

        #endregion

        #region Methods

        /// <summary>
        /// help to change style of statusbar
        /// </summary>
        /// <param name="isColorfulOrNot"></param>
        /// <param name="isLightOrNot"></param>
        public void ChangeStatusBar(bool isColorfulOrNot, bool isLightOrNot) {
            if (isColorfulOrNot) {
                StatusBarInit.InitDesktopStatusBar(isLightOrNot);
                Window.Current.SetTitleBar(TitleBarRec);
                if (StatusBarInit.IsTargetMobile()) {
                    StatusBarInit.InitInnerMobileStatusBar(true);
                    BaseGrid.Margin = new Thickness(0, 16, 0, 0);
                    sideGrid.Margin = new Thickness(0, 16, 0, 0);
                }
            } else {
                StatusBarInit.InitDesktopStatusBarToPrepare(isLightOrNot);
                StatusBarInit.InitMobileStatusBarToPrepare(isLightOrNot);
                if (StatusBarInit.IsTargetMobile()) {
                    StatusBarInit.InitInnerMobileStatusBar(false);
                    BaseGrid.Margin = new Thickness(0, 0, 0, 0);
                    sideGrid.Margin = new Thickness(0, 0, 0, 0);
                }
            }
        }

        #endregion
    }
}
