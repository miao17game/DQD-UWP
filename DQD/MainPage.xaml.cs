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
using Edi.UWP.Helpers;
using Windows.Security.ExchangeActiveSyncProvisioning;

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
            VersionText.Text = "版本号：" + Utils.GetAppVersion();
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
            InsideResources.CollapsedAllPanel();
            SettingsPopup.IsOpen = true;
            PopupBorder.Visibility = Visibility.Visible;
            EnterPopupBorder.Begin();
        }

        private void SettingsPopup_SizeChanged(object sender, SizeChangedEventArgs e) {
            PopupStack.Height = (sender as Popup).ActualHeight;
        }

        private void PopupBackButton_Click(object sender, RoutedEventArgs e) {
            foreach (var item in InsideResources.GetPopupPanelColl()) {
                if (item.Value.Visibility == Visibility.Visible){item.Value.Visibility = Visibility.Collapsed; return;}
            }
            SettingsPopup.IsOpen = false;
        }

        private void PopupInnerClick(object sender, RoutedEventArgs e) {
            var button = sender as Button;
            InsideResources.CollapsedAllPanel();
            InsideResources.GetPanelInstance(button.Name).Visibility = Visibility.Visible;
        }

        private void SettingsPopup_Closed(object sender, object e) {
            OutPopupBorder.Completed += OnOutPopupBorderOut;
            OutPopupBorder.Begin();
        }

        private void OnOutPopupBorderOut(object sender, object e) {
            OutPopupBorder.Completed -= OnOutPopupBorderOut;
            PopupBorder.Visibility = Visibility.Collapsed;
        }

        private async void FeedBackBtn_Click(object sender, RoutedEventArgs e) {
            await ReportError(null, "N/A", true);
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
            /// The dictionary map of Popup collection. 
            /// </summary>
            static private Dictionary<string, StackPanel> PopupInnerMaps = new Dictionary<string, StackPanel> {
            {"SettBtn",Current.SettMenu},
            {"AboutBtn",Current.AboutMenu},
            {"AboutDongQDBtn",Current.AboutDongQDMenu},
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

            public static Dictionary<string, StackPanel> GetPopupPanelColl() { return PopupInnerMaps; }

            /// <summary>
            /// Get target StackPanel by item name.
            /// </summary>
            /// <param name="str">item name</param>
            /// <returns></returns>
            public static StackPanel GetPanelInstance(string str) { return PopupInnerMaps.ContainsKey(str) ? PopupInnerMaps[str] : null; }

            /// <summary>
            /// Collapsed All StackPanel
            /// </summary>
            public static void CollapsedAllPanel() { foreach(var item in PopupInnerMaps) { item.Value.Visibility = Visibility.Collapsed; } }
        }
        #endregion

        #region Properties and States

        public static MainPage Current;
        public Frame contentFrame;
        public Grid SideGrid;
        public delegate void NavigateEventHandler(object sender, Type type, Frame frame);
        public delegate void ClickEventHandler(object sender, Type type, Frame frame, Uri uri ,int num, string content);
        private NavigateEventHandler SelectionChanged = (sender, type, frame) => { frame.Navigate(type); };
        public ClickEventHandler ItemClick = (sender, type, frame, uri ,num ,content) => { frame.Navigate(type, new ParameterNavigate { Number=num,Uri=uri,Summary= content }); };

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

        /// <summary>
        /// ReportError Method
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="msg"></param>
        /// <param name="pageSummary"></param>
        /// <param name="includeDeviceInfo"></param>
        /// <returns></returns>
        public static async Task ReportError(string msg = null, string pageSummary = "N/A", bool includeDeviceInfo = true) {
            var deviceInfo = new EasClientDeviceInformation();

            string subject = "DQD-UWP 反馈";
            string body = $"问题描述：{msg}  " +
                          $"\n\n\n\n\n\n（程序版本：{Utils.GetAppVersion()} ";

            if (includeDeviceInfo) {
                body += $", \n设备名：{deviceInfo.FriendlyName}, " +
                          $"\n操作系统：{deviceInfo.OperatingSystem}, " +
                          $"\nSKU：{deviceInfo.SystemSku}, " +
                          $"\n产品名称：{deviceInfo.SystemProductName}, " +
                          $"\n制造商：{deviceInfo.SystemManufacturer}, " +
                          $"\n固件版本：{deviceInfo.SystemFirmwareVersion}, " +
                          $"\n硬件版本：{deviceInfo.SystemHardwareVersion}）";
            } else {
                body += ")";
            }

            string to = "miao17game@qq.com";
            await Tasks.OpenEmailComposeAsync(to, subject, body);
        }

        #endregion
    }
}
