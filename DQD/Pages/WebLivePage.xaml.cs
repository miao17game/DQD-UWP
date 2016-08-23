using DQD.Core.Helpers;
using DQD.Core.Models;
using DQD.Core.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class WebLivePage : Page {
        
        #region Constructor

        public WebLivePage() {
            Current = this;
            transToSideGrid = this.RenderTransform as TranslateTransform;
            if (transToSideGrid == null) this.RenderTransform = transToSideGrid = new TranslateTransform();
            loadingAnimation = MainPage.Current.LoadingProgress;
            loadingAnimation.IsActive = true;
            this.Opacity = 0;
            this.InitializeComponent();
            ButtonShadow = ButtonStack;
            ButtonNoShadow = ButtonStackNoShadow;
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
            MainPage.Current.SideGrid.Visibility = Visibility.Collapsed;
            Current.Content = null;
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

        #region Animations

        #region Page Animation

        #region Animations Properties
        Storyboard storyToSideGrid = new Storyboard();
        TranslateTransform transToSideGrid;
        DoubleAnimation doubleAnimation;
        #endregion

        public void InitStoryBoard() {
            doubleAnimation = new DoubleAnimation() {
                Duration = new Duration(TimeSpan.FromMilliseconds(220)),
                From = this.ActualWidth,
                To = 0,
            };
            doubleAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
            doubleAnimation.Completed += DoublAnimation_Completed;
            storyToSideGrid = new Storyboard();
            Storyboard.SetTarget(doubleAnimation, transToSideGrid);
            Storyboard.SetTargetProperty(doubleAnimation, "X");
            storyToSideGrid.Children.Add(doubleAnimation);
            storyToSideGrid.Begin();
        }

        private void DoublAnimation_Completed(object sender, object e) {
            storyToSideGrid.Stop();
            doubleAnimation.Completed -= DoublAnimation_Completed;
        }
        #endregion

        #region Button Animations

        #region Animations Properties
        public StackPanel ButtonThisPage { get; private set; }
        private double listViewOffset = default(double);
        Storyboard BtnStackSlideIn = new Storyboard();
        Storyboard BtnStackSlideOut = new Storyboard();
        TranslateTransform transToButtonThisPage;
        DoubleAnimation doubleAnimationBtn;
        bool IsAnimaEnabled;
        #endregion

        #region Handler of ListView Scroll 

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e) {
            try {
                if (listViewOffset - (sender as ScrollViewer).VerticalOffset < -10
                    && ButtonThisPage.Visibility == Visibility.Visible
                    && IsAnimaEnabled) {
                    ContentScroll.ViewChanged -= ScrollViewer_ViewChanged;
                    BtnStackSlideOut.Begin();
                }
                if (listViewOffset - (sender as ScrollViewer).VerticalOffset > 10
                    && ButtonThisPage.Visibility == Visibility.Collapsed
                    && IsAnimaEnabled) {
                    ContentScroll.ViewChanged -= ScrollViewer_ViewChanged;
                    ButtonThisPage.Visibility = Visibility.Visible;
                    BtnStackSlideIn.Begin();
                }
                listViewOffset = (sender as ScrollViewer).VerticalOffset;
            } catch { Debug.WriteLine("Save scroll positions error."); }
        }

        #endregion

        #region Animation Methods

        private void InitFloatButtonView() {
            ButtonThisPage = MainPage.Current.IsButtonShadowVisible ? ButtonStack : ButtonStackNoShadow;
            if (MainPage.Current.IsFloatButtonEnable) {
                ButtonStack.Visibility = VisiEnumHelper.GetVisibility(MainPage.Current.IsButtonShadowVisible);
                ButtonStackNoShadow.Visibility = VisiEnumHelper.GetVisibility(!MainPage.Current.IsButtonShadowVisible);
                if (MainPage.Current.IsButtonAnimationEnable) {
                    ContentScroll.ViewChanged += ScrollViewer_ViewChanged;
                    InitStoryBoardBtn();
                }
            } else {
                ButtonStack.Visibility = VisiEnumHelper.GetVisibility(false);
                ButtonStackNoShadow.Visibility = VisiEnumHelper.GetVisibility(false);
                transToButtonThisPage = null;
            }
        }

        public void HandleAnimation(bool isAnima) { if (isAnima) { InitStoryBoardBtn(); } else { DestoryAnimation(); } }

        private void DestoryAnimation() { IsAnimaEnabled = false; }

        private void InitStoryBoardBtn() {
            IsAnimaEnabled = true;
            transToButtonThisPage = ButtonThisPage.RenderTransform as TranslateTransform;
            if (transToButtonThisPage == null) ButtonThisPage.RenderTransform = transToButtonThisPage = new TranslateTransform();
            doubleAnimationBtn = new DoubleAnimation() {
                Duration = new Duration(TimeSpan.FromMilliseconds(520)),
                From = -100,
                To = 0,
            };
            doubleAnimationBtn.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
            doubleAnimationBtn.Completed += DoublAnimationIn_Completed;
            BtnStackSlideIn = new Storyboard();

            Storyboard.SetTarget(doubleAnimationBtn, transToButtonThisPage);
            Storyboard.SetTargetProperty(doubleAnimationBtn, "X");
            BtnStackSlideIn.Children.Add(doubleAnimationBtn);
            doubleAnimationBtn = new DoubleAnimation() {
                Duration = new Duration(TimeSpan.FromMilliseconds(520)),
                From = 0,
                To = -100,
            };
            doubleAnimationBtn.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
            doubleAnimationBtn.Completed += DoublAnimationOut_Completed;
            BtnStackSlideOut = new Storyboard();
            Storyboard.SetTarget(doubleAnimationBtn, transToButtonThisPage);
            Storyboard.SetTargetProperty(doubleAnimationBtn, "X");
            BtnStackSlideOut.Children.Add(doubleAnimationBtn);
        }

        #endregion

        #region Animation Events
        private void DoublAnimationIn_Completed(object sender, object e) {
            ContentScroll.ViewChanged += ScrollViewer_ViewChanged;
        }

        private void DoublAnimationOut_Completed(object sender, object e) {
            ButtonThisPage.Visibility = Visibility.Collapsed;
            ContentScroll.ViewChanged += ScrollViewer_ViewChanged;
        }
        #endregion

        #endregion

        #endregion

        #region Properties and State

        public static WebLivePage Current { get; private set; }
        public StackPanel ButtonShadow { get; private set; }
        public StackPanel ButtonNoShadow { get; private set; }
        private Uri HostSource;
        private int HostNumber;
        private bool IsFirstNavigated;
        private ProgressRing loadingAnimation;

        #endregion

    }
}
