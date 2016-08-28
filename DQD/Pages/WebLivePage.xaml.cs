using DQD.Core.Helpers;
using DQD.Core.Models;
using DQD.Core.Tools;
using DQD.Net.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Profile;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace DQD.Net.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WebLivePage : BaseContentPage {
        
        #region Constructor

        public WebLivePage() {
            Current = this;
            loadingAnimation = MainPage.Current.LoadingProgress;
            loadingAnimation.IsActive = true;
            IsScreenOpen = false;
            this.Opacity = 0;
            this.InitializeComponent();
            ButtonShadow = ButtonStack;
            ButtonNoShadow = ButtonStackNoShadow;
            ScreenShadow = ScreenStack;
            ScreenNoShadow = ScreenStackNoShadow;
            GC.Collect();
        }

        #endregion

        #region Events

        /// <summary>
        /// trigger the event when this frame is first navigated
        /// </summary>
        /// <param name="e">navigate args</param>
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            IsFirstNavigated = true;
            var parameter = e.Parameter as ParameterNavigate;
            HostSource = parameter.Uri;
            HostNumber = parameter.Number;
            if (HostSource == null) return;
            webView.Source = HostSource;
            if (StatusBarInit.IsMobile) { BackBtn.Visibility = Visibility.Collapsed; ContentTitle.Margin = new Thickness(15, 0, 0, 0); }
            InitFloatButtonView();
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e) {
            storyToSideGridOut.Begin();
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e) {
            loadingAnimation.IsActive = true;
            webView.Refresh();
        }

        private void webView_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args) {
            loadingAnimation.IsActive = false;
        }

        private void webView_ContentLoading(WebView sender, WebViewContentLoadingEventArgs args) {
            this.Opacity = 1;
            loadingAnimation.IsActive = false;
            InitFloatButtonView();
            if (IsFirstNavigated) { InitStoryBoard(); IsFirstNavigated = false; }
        }

        #endregion

        #region Methods

        #endregion

        #region Button Animations

        #region Animations Properties
        public StackPanel ButtonThisPage { get; private set; }
        public StackPanel ScreenBtnThisPage { get; private set; }
        #endregion

        #region Animation Methods

        private void InitFloatButtonView() {
            ButtonThisPage = MainPage.Current.IsButtonShadowVisible ? ButtonStack : ButtonStackNoShadow;
            ScreenBtnThisPage = MainPage.Current.IsButtonShadowVisible ? ScreenStack : ScreenStackNoShadow;
            if (MainPage.Current.IsFloatButtonEnable) {
                ScreenStack.Visibility = ButtonStack.Visibility = VisiEnumHelper.GetVisibility(MainPage.Current.IsButtonShadowVisible);
                ScreenStackNoShadow.Visibility = ButtonStackNoShadow.Visibility = VisiEnumHelper.GetVisibility(!MainPage.Current.IsButtonShadowVisible);
            } else {
                ScreenStack.Visibility = ButtonStack.Visibility = VisiEnumHelper.GetVisibility(false);
                ScreenStackNoShadow.Visibility = ButtonStackNoShadow.Visibility = VisiEnumHelper.GetVisibility(false);
            }
        }
        
        #endregion

        #endregion

        #region Properties and State

        public static WebLivePage Current { get; private set; }
        public StackPanel ButtonShadow { get; private set; }
        public StackPanel ButtonNoShadow { get; private set; }
        public StackPanel ScreenShadow { get; private set; }
        public StackPanel ScreenNoShadow { get; private set; }
        public bool IsScreenOpen { get; set; }
        private Uri HostSource;
        private int HostNumber;
        private bool IsFirstNavigated;
        private ProgressRing loadingAnimation;

        #endregion

        private void ScreenBtn_Click(object sender, RoutedEventArgs e) {
            if (!IsScreenOpen) {
                ButtonThisPage.Visibility = Visibility.Collapsed;
                TitleBorder.Visibility = Visibility.Collapsed;
                IsScreenOpen = true;
                ContentBord.Margin = new Thickness(0, 0, 0, 0);
                if (AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Mobile")) { return; }
                Grid.SetColumnSpan(MainPage.Current.BaseBorderTarget, 2);
            } else {
                ButtonThisPage.Visibility = Visibility.Visible;
                TitleBorder.Visibility = Visibility.Visible;
                IsScreenOpen = false;
                ContentBord.Margin = new Thickness(0, 60, 0, 0);
                if (AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Mobile")) { return; }
                Grid.SetColumnSpan(MainPage.Current.BaseBorderTarget, 1);
            }
        }

        private void BaseGrid_SizeChanged(object sender, SizeChangedEventArgs e) {
            webView.Height = (sender as Grid).ActualHeight;
            var wholeHeight = Window.Current.Bounds.Height;
            var wholeWidth = Window.Current.Bounds.Width;
            if (wholeHeight >= 400)
                ContentBord.Margin = wholeWidth < 800 ? new Thickness(0, 76, 0, 0) : new Thickness(0, 60, 0, 0);
            if (IsScreenOpen) {
                ButtonThisPage.Visibility = Visibility.Collapsed;
                TitleBorder.Visibility = Visibility.Collapsed;
                ContentBord.Margin = new Thickness(0, 0, 0, 0);
                if (AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Mobile")) { return; }
                Grid.SetColumnSpan(MainPage.Current.BaseBorderTarget, 2);
            } else {
                ButtonThisPage.Visibility = Visibility.Visible;
                TitleBorder.Visibility = Visibility.Visible;
                ContentBord.Margin = new Thickness(0, 60, 0, 0);
                if (AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Mobile")) { return; }
                Grid.SetColumnSpan(MainPage.Current.BaseBorderTarget, 1);
            }
        }
    }
}
