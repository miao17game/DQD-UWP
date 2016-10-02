﻿#region Using
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
using Windows.UI.Xaml.Media.Imaging;
using DQD.Core.Controls;
using Windows.Storage;
using Windows.Storage.FileProperties;
#endregion
namespace DQD.Net {
    /// <summary>
    /// MainPage Code Page
    /// </summary>
    public sealed partial class MainPage:Page {

        #region Constructor

        public MainPage() {
            Current = this;
            InitializeComponent();
            InitFlipTimer();
            InitSliderValue();
            InitSwitchState();
            PrepareFrame.Navigate(typeof(PreparePage));
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
            BaseLoadingProgress = BaseLoadingAnimation;
            LoadingProgress = LoadingAnimation;
            ContFrame = ContentFrame;
            SideGrid = sideGrid;
            MainGrid = grid;
            BaseBorderTarget = BaseBorder;
            if (AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Mobile")) {
                ApplicationView.GetForCurrentView().VisibleBoundsChanged += (s, e) => { ChangeViewWhenNavigationBarChanged(); };
                ChangeViewWhenNavigationBarChanged(); }
            VersionText.Text = "版本号：" + Edi.UWP.Helpers.Utils.GetAppVersion();
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
            SelectionChanged?.Invoke(
                this,
                InsideResources.GetTPageype(item),
                InsideResources.GetTFrameype(item));
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e) {
            if (AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Mobile"))
                if (ContFrame.Content == null) {
                    if (!isNeedClose) { InitCloseAppTask(); } else { Application.Current.Exit(); }
                    e.Handled = true;
                    return;
                } else new BackPressedEvent(() => {
                    if (ContentPage.Current != null)
                        ContentPage.Current.storyToSideGridOut.Begin();
                    if (DataContentPage.Current != null)
                        DataContentPage.Current.storyToSideGridOut.Begin();
                    if (WebLivePage.Current != null)
                        WebLivePage.Current.ClearThisPageByAnima(); }).Invoke();
            else {
                ContentFrame.Content = null;
                if (ContentPage.Current != null)
                    ContentPage.Current.ClearThisPage();
                if (DataContentPage.Current != null)
                    DataContentPage.Current.ClearThisPage();
                if (WebLivePage.Current != null)
                    WebLivePage.Current.ClearThisPage();
            } e.Handled = true;
        }

        private void grid_SizeChanged(object sender, SizeChangedEventArgs e) {
            sideGrid.Visibility = 
                (sender as Grid).ActualWidth > 800? 
                Visibility.Visible:
                ContentFrame.Content == null ? 
                Visibility.Collapsed : 
                Visibility.Visible;
        }

        private void BaseGrid_SizeChanged(object sender,SizeChangedEventArgs e) {
            RootPivot.HeaderWidth=((sender as Grid).ActualWidth-1)/4;
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
            InsideResources.GetSwitchHandler((sender as ToggleSwitch).Name)
                .Invoke((sender as ToggleSwitch).Name);
        }

        private async void SettingsBtn_Click(object sender, RoutedEventArgs e) {
            InsideResources.CollapsedAllPanel();
            SettingsPopup.IsOpen = true;
            PopupBorder.Visibility = Visibility.Visible;
            EnterPopupBorder.Begin();
            await ShowCacheSize();
        }

        private void SettingsPopup_SizeChanged(object sender, SizeChangedEventArgs e) {
            PopupStack.Height = (sender as Popup).ActualHeight;
        }

        private void PopupBackButton_Click(object sender, RoutedEventArgs e) {
            foreach (var item in InsideResources.GetPopupPanelColl()) 
                if (item.Value.Visibility == Visibility.Visible){item.Value.Visibility = Visibility.Collapsed; return;}
            SettingsPopup.IsOpen = false;
        }

        private async void CacheClearBtn_Click(object sender, RoutedEventArgs e) {
            CacheClearBtn.IsEnabled = false;
            ClearRing.IsActive = true;
            await ClearCacheSize();
            CacheClearBtn.IsEnabled = true;
            ClearRing.IsActive = false;
        }

        private void PopupInnerClick(object sender, RoutedEventArgs e) {
            InsideResources.CollapsedAllPanel();
            InsideResources.GetPanelInstance((sender as Button).Name).Visibility = Visibility.Visible;
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

        private void TextFieldSizeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
            if (ExampleText != null)
                ExampleText.FontSize = e.NewValue;
            if (ContentPage.Current != null)
                ContentPage.Current.ChangeTextFieldSize(e.NewValue);
            if (!isInitSliderValue)
                SettingsHelper.SaveSettingsValue(SettingsConstants.TextFieldSize, e.NewValue);
            isInitSliderValue = false;
        }

        #endregion

        #region Methods

        private void InitCloseAppTask() {
            isNeedClose = true;
            new ToastSmooth("再按一次返回键退出").Show();
            Task.Run(async () => {
                await Task.Delay(2000);
                isNeedClose = false; });
        }

        private void InitFlipTimer() {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 10);
            timer.Tick += (obj, args) => {
                if (nowBackgroundPic < InsideResources.GetBackground().Count) {
                    BackgroundImage.Source = new BitmapImage(InsideResources.GetBackground()[nowBackgroundPic]);
                    nowBackgroundPic++;
                } else {
                    BackgroundImage.Source = new BitmapImage(InsideResources.GetBackground()[0]);
                    nowBackgroundPic = 0;
                }
            };
            timer.Start();
        }

        private void InitSliderValue() {
            TextFieldSizeSlider.Value = (double?)SettingsHelper.ReadSettingsValue(SettingsConstants.TextFieldSize) ?? 14;
        }

        private async Task ShowCacheSize() {
            var localCF = ApplicationData.Current.LocalCacheFolder;
            var folders = await localCF.GetFoldersAsync();
            double sizeOfCache = 0.00;
            BasicProperties propertiesOfItem;
            foreach (var item in folders) {
                var files = await item.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.DefaultQuery);
                foreach (var file in files) {
                    propertiesOfItem = await file.GetBasicPropertiesAsync();
                    sizeOfCache += propertiesOfItem.Size;
                }
            }
            var sizeInMb = sizeOfCache / (1024 * 1024);
            CacheSizeTitle.Text = sizeInMb - 0 > 0.00000001 ? sizeInMb.ToString("#.##") + "MB" : "0 MB";
        }

        private async Task ClearCacheSize() {
            var localCF = ApplicationData.Current.LocalCacheFolder;
            var folders = await localCF.GetFoldersAsync();
            foreach (var item in folders) {
                var files = await item.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.DefaultQuery);
                foreach (var file in files) {
                    await file.DeleteAsync();
                }
            }
            await ShowCacheSize();
        }

        /// <summary>
        /// help to change style of statusbar
        /// </summary>
        /// <param name="isColorfulOrNot"></param>
        /// <param name="isLightOrNot"></param>
        public void ChangeStatusBar(bool isColorfulOrNot, bool isLightOrNot) {
            if (isColorfulOrNot) {
                StatusBarInit.InitDesktopStatusBar(isLightOrNot);
                Window.Current.SetTitleBar(TitleBarRec);
                if (AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Mobile")) {
                    StatusBarInit.InitInnerMobileStatusBar(true);
                    BaseGrid.Margin = new Thickness(0, 16, 0, 0);
                    sideGrid.Margin = new Thickness(0, 0, 0, 0);
                }
            } else {
                StatusBarInit.InitDesktopStatusBarToPrepare(isLightOrNot);
                StatusBarInit.InitMobileStatusBarToPrepare(isLightOrNot);
                if (AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Mobile")) {
                    StatusBarInit.InitInnerMobileStatusBar(false);
                    BaseGrid.Margin = new Thickness(0, 0, 0, 0);
                    sideGrid.Margin = new Thickness(0, -16, 0, 0);
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
                          $"\n\n\n\n\n\n（程序版本：{Edi.UWP.Helpers.Utils.GetAppVersion()} ";

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

        private void ChangeViewWhenNavigationBarChanged() {
            Width = ApplicationView.GetForCurrentView().VisibleBounds.Width;
            var wholeHeight = Window.Current.Bounds.Height;
            var wholeWidth = Window.Current.Bounds.Width;
            var isColorfulOrNot = (bool?)SettingsHelper.ReadSettingsValue(SettingsConstants.IsColorfulOrNot) ?? false;
            if (Window.Current.Bounds.Height < Window.Current.Bounds.Width) {
                Height = ApplicationView.GetForCurrentView().VisibleBounds.Height;
                if (isColorfulOrNot) {
                    Width = ApplicationView.GetForCurrentView().VisibleBounds.Width + 48;
                    Margin =
                        Width - wholeWidth > -0.1 ?
                        new Thickness(0, 0, 0, 0) :
                        new Thickness(-24, 0, 0, 0);
                } else {
                    Width = ApplicationView.GetForCurrentView().VisibleBounds.Width;
                    Margin =
                        Width + 48 - wholeWidth > -0.1 ?
                        new Thickness(40, 0, 0, 0) :
                        new Thickness(-24, 0, 0, 0);
                } return;
            }
            if (isColorfulOrNot) {
                Height = ApplicationView.GetForCurrentView().VisibleBounds.Height + 24;
                Margin =
                    Height - wholeHeight > -0.1 ?
                    new Thickness(0, 0, 0, 0) :
                    new Thickness(0, -48, 0, 0);
            } else {
                Height = ApplicationView.GetForCurrentView().VisibleBounds.Height;
                Margin =
                    Height + 24 - wholeHeight > -0.1 ?
                    new Thickness(0, 24, 0, 0) :
                    new Thickness(0, -24, 0, 0);
            }
        }

        #region Swith Methods

        private async void InitSwitchState() {
            IsFloatButtonEnable =
                AnimationSwitch.IsEnabled =
                ShadowSwitch.IsEnabled =
                ButtonSwitch.IsOn =
                (bool?)SettingsHelper.ReadSettingsValue(SettingsConstants.IsFloatButtonEnabled) ?? false;
            IsButtonShadowVisible =
                ShadowSwitch.IsOn =
                (bool?)SettingsHelper.ReadSettingsValue(SettingsConstants.IsFloatButtonShadow) ?? false;
            IsButtonAnimationEnable =
                AnimationSwitch.IsOn =
                (bool?)SettingsHelper.ReadSettingsValue(SettingsConstants.IsFloatButtonAnimation) ?? false;
            PicturesAutoSwitch.IsOn = (bool?)SettingsHelper.ReadSettingsValue(SettingsConstants.IsPicturesAutoLoad) ?? false;
            ColorSwitch.IsOn = (bool?)SettingsHelper.ReadSettingsValue(SettingsConstants.IsColorfulOrNot) ?? true;
            var isLightOrNot = (bool?)SettingsHelper.ReadSettingsValue(SettingsConstants.IsLigheOrNot) ?? false;
            RequestedTheme = isLightOrNot ? ElementTheme.Light : ElementTheme.Dark;
            ThemeModeBtn.Content = isLightOrNot ? char.ConvertFromUtf32(0xEC46) : char.ConvertFromUtf32(0xEC8A);
            await ShowCacheSize();
        }

        private void OnStatusBarSwitchToggled(ToggleSwitch sender) {
            SettingsHelper.SaveSettingsValue(SettingsConstants.IsColorfulOrNot, sender.IsOn);
            var isLightOrNot = (bool?)SettingsHelper.ReadSettingsValue(SettingsConstants.IsLigheOrNot) ?? false;
            var wholeHeight = Window.Current.Bounds.Height;
            if (sender.IsOn) {
                StatusBarInit.InitDesktopStatusBar(isLightOrNot);
                StatusBarInit.InitInnerDesktopStatusBar(true);
                Window.Current.SetTitleBar(TitleBarRec);
                if (AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Mobile")) {
                    StatusBarInit.InitInnerMobileStatusBar(true);
                    if(Window.Current.Bounds.Height < Window.Current.Bounds.Width) {
                        BaseGrid.Margin = new Thickness(0, 0, 0, 0);
                        sideGrid.Margin = new Thickness(0, 0, 0, 0);
                        new Thickness(0, -0, 0, 0);
                        return; }
                    Height = ApplicationView.GetForCurrentView().VisibleBounds.Height + 24;
                    BaseGrid.Margin = new Thickness(0, 16, 0, 0);
                    sideGrid.Margin = new Thickness(0, 0, 0, 0);
                    Margin = Height + 24 - wholeHeight > -0.1 ? new Thickness(0, -0, 0, 0) : new Thickness(0, -48, 0, 0);
                }
            } else {
                StatusBarInit.InitDesktopStatusBarToPrepare(isLightOrNot);
                StatusBarInit.InitMobileStatusBarToPrepare(isLightOrNot);
                StatusBarInit.InitInnerDesktopStatusBar(false);
                if (AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Mobile")) {
                    if (Window.Current.Bounds.Height < Window.Current.Bounds.Width) {
                        BaseGrid.Margin = new Thickness(0, 0, 0, 0);
                        sideGrid.Margin = new Thickness(0, 0, 0, 0);
                        new Thickness(0, -0, 0, 0);
                        return; }
                    Height = ApplicationView.GetForCurrentView().VisibleBounds.Height;
                    BaseGrid.Margin = new Thickness(0, 0, 0, 0);
                    sideGrid.Margin = new Thickness(0, -16, 0, 0);
                    StatusBarInit.InitInnerMobileStatusBar(false);
                    Margin = Height + 24 - wholeHeight > -0.1 ? new Thickness(0, 24, 0, 0) : new Thickness(0, -24, 0, 0);
                }
            }
        }

        private void OnFloatButtonSwitchToggled(ToggleSwitch sender) {
            IsFloatButtonEnable = sender.IsOn;
            AnimationSwitch.IsEnabled = ShadowSwitch.IsEnabled = IsFloatButtonEnable;
            SettingsHelper.SaveSettingsValue(SettingsConstants.IsFloatButtonEnabled, IsFloatButtonEnable);
            if (DataContentPage.Current != null)
                SyncVisibility(IsFloatButtonEnable, DataContentPage.Current.ButtonShadow, DataContentPage.Current.ButtonNoShadow);
            if (ContentPage.Current != null)
                SyncVisibility(IsFloatButtonEnable, ContentPage.Current.ButtonShadow, ContentPage.Current.ButtonNoShadow);
            if (HomePage.Current != null)
                SyncVisibility(IsFloatButtonEnable, HomePage.Current.ButtonShadow, HomePage.Current.ButtonNoShadow);
            if (MatchPage.Current != null)
                SyncVisibility(IsFloatButtonEnable, MatchPage.Current.ButtonShadow, MatchPage.Current.ButtonNoShadow);
            if (VideoPage.Current != null)
                SyncVisibility(IsFloatButtonEnable, VideoPage.Current.ButtonShadow, VideoPage.Current.ButtonNoShadow);
            if (WebLivePage.Current != null) {
                SyncVisibility(IsFloatButtonEnable, WebLivePage.Current.ButtonShadow, WebLivePage.Current.ButtonNoShadow);
                SyncVisibility(IsFloatButtonEnable, WebLivePage.Current.ScreenShadow, WebLivePage.Current.ScreenNoShadow); }
        }

        private void OnButtonShadowSwitchToggled(ToggleSwitch sender) {
            IsButtonShadowVisible = sender.IsOn;
            SettingsHelper.SaveSettingsValue(SettingsConstants.IsFloatButtonShadow, IsButtonShadowVisible);
            if (DataContentPage.Current != null)
                DivideVisibility(IsButtonShadowVisible, DataContentPage.Current.ButtonShadow, DataContentPage.Current.ButtonNoShadow);
            if (ContentPage.Current != null)
                DivideVisibility(IsButtonShadowVisible, ContentPage.Current.ButtonShadow, ContentPage.Current.ButtonNoShadow);
            if (HomePage.Current != null)
                DivideVisibility(IsButtonShadowVisible, HomePage.Current.ButtonShadow, HomePage.Current.ButtonNoShadow);
            if (MatchPage.Current != null)
                DivideVisibility(IsButtonShadowVisible, MatchPage.Current.ButtonShadow, MatchPage.Current.ButtonNoShadow);
            if (VideoPage.Current != null)
                DivideVisibility(IsButtonShadowVisible, VideoPage.Current.ButtonShadow, VideoPage.Current.ButtonNoShadow);
            if (WebLivePage.Current != null) {
                DivideVisibility(IsButtonShadowVisible, WebLivePage.Current.ButtonShadow, WebLivePage.Current.ButtonNoShadow);
                DivideVisibility(IsButtonShadowVisible, WebLivePage.Current.ScreenShadow, WebLivePage.Current.ScreenNoShadow); }
        }

        private void OnButtonAutoAnimaSwitchToggled(ToggleSwitch sender) {
            IsButtonAnimationEnable = sender.IsOn;
            SettingsHelper.SaveSettingsValue(SettingsConstants.IsFloatButtonAnimation, IsButtonAnimationEnable);
            if (HomePage.Current != null)
                HomePage.Current.HandleAnimation(IsButtonAnimationEnable);
            if (VideoPage.Current != null)
                VideoPage.Current.HandleAnimation(IsButtonAnimationEnable);
            if (ContentPage.Current != null)
                ContentPage.Current.HandleAnimation(IsButtonAnimationEnable);
        }

        private void OnGifsAutoLoadSwitchToggled(ToggleSwitch sender) {
            SettingsHelper.SaveSettingsValue(SettingsConstants.IsPicturesAutoLoad, (sender as ToggleSwitch).IsOn);
        }

        private void DivideVisibility(bool isVisible, StackPanel sp1, StackPanel sp2) { sp1.Visibility = VisiEnumHelper.GetVisibility(isVisible); sp2.Visibility = VisiEnumHelper.GetVisibility(!isVisible); }

        private void SyncVisibility(bool isVisible, StackPanel sp1, StackPanel sp2) { sp2.Visibility = sp1.Visibility = VisiEnumHelper.GetVisibility(isVisible); }

        #endregion

        #region Handler of ListView Scroll 

        public static ScrollViewer GetScrollViewer(DependencyObject depObj) {
            if (depObj is ScrollViewer)
                return depObj as ScrollViewer;
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++) {
                var child = VisualTreeHelper.GetChild(depObj, i);
                var result = GetScrollViewer(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        public static PivotItem GetPVItemViewer(DependencyObject depObj, ref int num) {
            if (depObj is PivotItem) {
                if (num == 0)
                    return depObj as PivotItem;
                else
                    num--;
                return null;
            }
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++) {
                var child = VisualTreeHelper.GetChild(depObj, i);
                var result = GetPVItemViewer(child, ref num);
                if (result != null)
                    return result;
            }
            return null;
        }

        #endregion

        #endregion

        #region Static Inside Class

        /// <summary>
        /// Page inside resources of navigating and choosing frames.
        /// </summary>
        static class InsideResources {

            public static Type GetTPageype(string str) { return PagesMaps.ContainsKey(str) ? PagesMaps[str] : null; }
            static public Dictionary<string, Type> PagesMaps = new Dictionary<string, Type> {
            {"HomePItem",typeof(HomePage)},
            {"MatchPItem",typeof(MatchPage)},
            {"VideoPItem",typeof(VideoPage)},
            {"DataPItem",typeof(DataPage)},
        };

            public static Frame GetTFrameype(string str) { return FramesMaps.ContainsKey(str) ? FramesMaps[str] : null; }
            static private Dictionary<string, Frame> FramesMaps = new Dictionary<string, Frame> {
            {"HomePItem",Current.HomePFrame},
            {"MatchPItem",Current.MatchPFrame},
            {"VideoPItem",Current.VideoPFrame},
            {"DataPItem",Current.DataPFrame},
        };

            public static Dictionary<string, ScrollViewer> GetPopupPanelColl() { return PopupInnerMaps; }
            public static ScrollViewer GetPanelInstance(string str) { return PopupInnerMaps.ContainsKey(str) ? PopupInnerMaps[str] : null; }
            public static void CollapsedAllPanel() { foreach (var item in PopupInnerMaps) { item.Value.Visibility = Visibility.Collapsed; } }
            static private Dictionary<string, ScrollViewer> PopupInnerMaps = new Dictionary<string, ScrollViewer> {
            {"SettBtn",Current.SettMenu},
            {"AboutBtn",Current.AboutMenu},
            {"AboutDongQDBtn",Current.AboutDongQDMenu},
        };

            public static ToggleSwitch GetSwitchInstance(string str) { return SwitchSettingsMaps.ContainsKey(str) ? SwitchSettingsMaps[str] : null; }
            static private Dictionary<string, ToggleSwitch> SwitchSettingsMaps = new Dictionary<string, ToggleSwitch> {
            {"ColorSwitch",Current.ColorSwitch},
            {"ButtonSwitch",Current.ButtonSwitch},
            {"ShadowSwitch",Current.ShadowSwitch},
            {"AnimationSwitch",Current.AnimationSwitch},
            {"PicturesAutoSwitch",Current.PicturesAutoSwitch},
        };

            public static SwitchEventHandler GetSwitchHandler(string str) { return SwitchHandlerMaps.ContainsKey(str) ? SwitchHandlerMaps[str] : null; }
            static private Dictionary<string, SwitchEventHandler> SwitchHandlerMaps = new Dictionary<string, SwitchEventHandler> {
            {"ColorSwitch", new SwitchEventHandler(instance=> { Current.OnStatusBarSwitchToggled(GetSwitchInstance(instance)); }) },
            {"ButtonSwitch", new SwitchEventHandler(instance=> { Current .OnFloatButtonSwitchToggled(GetSwitchInstance(instance)); }) },
            {"ShadowSwitch", new SwitchEventHandler(instance=> { Current .OnButtonShadowSwitchToggled(GetSwitchInstance(instance)); }) },
            {"AnimationSwitch", new SwitchEventHandler(instance=> { Current .OnButtonAutoAnimaSwitchToggled(GetSwitchInstance(instance)); }) },
            {"PicturesAutoSwitch", new SwitchEventHandler(instance=> { Current .OnGifsAutoLoadSwitchToggled(GetSwitchInstance(instance)); }) },
        };

            public static List<Uri> GetBackground() { return BackgroundPicMaps; }
            static private List<Uri> BackgroundPicMaps = new List < Uri > {
                new Uri("ms-appx:///Assets/bg1.jpg"),
                new Uri("ms-appx:///Assets/bg2.jpg"),
                new Uri("ms-appx:///Assets/bg3.jpg"),
                new Uri("ms-appx:///Assets/bg4.jpg"),
                new Uri("ms-appx:///Assets/bg5.jpg"),
                new Uri("ms-appx:///Assets/bg6.jpg"),
                new Uri("ms-appx:///Assets/bg7.jpg"),
                new Uri("ms-appx:///Assets/bg8.jpg"),
                new Uri("ms-appx:///Assets/bg9.jpg"),
                new Uri("ms-appx:///Assets/bg10.jpg"),
                new Uri("ms-appx:///Assets/bg11.jpg"),
                new Uri("ms-appx:///Assets/bg12.jpg"),
        };

        }
        #endregion

        #region Properties and States

        public static MainPage Current { get; private set; }
        public Frame ContFrame { get; private set; }
        public Grid SideGrid { get; private set; }
        public Grid MainGrid { get; private set; }
        public Border BaseBorderTarget { get; set; }
        public ProgressRing BaseLoadingProgress { get; private set; }
        public ProgressRing LoadingProgress { get; private set; }
        public bool IsFloatButtonEnable { get; private set; }
        public bool IsButtonShadowVisible { get; private set; }
        public bool IsButtonAnimationEnable { get; private set; }
        private int nowBackgroundPic = 0;
        private bool isNeedClose = false;
        private bool isInitSliderValue = true;
        public delegate void BackPressedEvent();
        public delegate void NavigateEventHandler(object sender, Type type, Frame frame);
        public delegate void SwitchEventHandler(string instance);
        public delegate void ClickEventHandler(object sender, Type type, Frame frame, Uri uri ,int num, string content);
        private NavigateEventHandler SelectionChanged = (sender, type, frame) => { frame.Navigate(type); };
        public ClickEventHandler ItemClick = (sender, type, frame, uri ,num ,content) => { frame.Navigate(type, new ParameterNavigate { Number=num,Uri=uri,Summary= content }); };

        #endregion

    }
}
