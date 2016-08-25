#region Using
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using DQD.Core.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using DQD.Core.DataVirtualization;
using Windows.System.Profile;
using Windows.UI.Xaml.Media.Animation;
using System.Threading.Tasks;
using DQD.Core.Controls;
using DQD.Core.Helpers;
using DQD.Core.Tools;
#endregion

namespace DQD.Net.Pages {
    /// <summary>
    /// HomePage Code Page
    /// </summary>
    public sealed partial class HomePage:Page {

        #region Constructor
        public HomePage() {
            Current = this;
            this.InitializeComponent();
            cacheDic = new Dictionary<string, DQDDataContext<ContentListModel>>();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            loadingAnimation = MainPage.Current.BaseLoadingProgress;
            loadingAnimation.IsActive = true;
            ButtonShadow = ButtonStack;
            ButtonNoShadow = ButtonStackNoShadow;
            InitView();
        }
        #endregion

        #region Methods

        /// <summary>
        /// Init the Page Initual databundle resources.
        /// </summary>
        private async void InitView() {
            StringBuilder urlString = await WebProcess.GetHtmlResources(HomeHost);
            var headerList = DataHandler.SetHeaderGroupResources(urlString.ToString());
            foreach (var item in headerList) {
                cacheDic.Add(
                    item.Title,
                    new DQDDataContext<ContentListModel>(
                        FetchMoreResources,
                        item.Number,
                        15,
                        HomeHost,
                        InitSelector.Special));
            }
            HeaderResources.Source = headerList;
            var list = DataProcess.GetFlipViewContent(urlString.ToString());
            FlipResouces.Source = list;
            InitFlipTimer(list);
            InitFloatButtonView();
        }

        private void InitFlipTimer(List<ContentListModel> list) {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 5);
            timer.Tick += (obj, args) => { if (MyFlip.SelectedIndex < list.Count - 1) MyFlip.SelectedIndex++; else MyFlip.SelectedIndex = 0; };
            timer.Start();
        }

        private async Task<ObservableCollection<ContentListModel>> FetchMoreResources(int number, uint rollNum, uint nowWholeCountX) {
            var Host = "http://www.dongqiudi.com?tab={0}&page={1}";
            Host = string.Format(
                Host, 
                number,
                nowWholeCountX / 15);
            return await DataHandler.SetHomeListResources(Host);
        }

        #endregion

        #region Events

        private void ListView_ItemClick(object sender,ItemClickEventArgs e) {
            var itemUri = (e.ClickedItem as ContentListModel).Path;
            var itemNum = (e.ClickedItem as ContentListModel).ID;
            MainPage.Current.ItemClick?.Invoke(
                this, 
                typeof(ContentPage), 
                MainPage.Current.contentFrame, 
                itemUri, 
                itemNum,
                null);
            MainPage.Current.SideGrid.Visibility = Visibility.Visible;
        }

        private void MyPivot_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var item = (sender as Pivot).SelectedItem as HeaderModel;
            var num = MyPivot.SelectedIndex;
            nowItem = item.Title;
            itemNumber = item.Number;
            if (!cacheDic.ContainsKey(item.Title)) {
                homeLlistResources =
                    new DQDDataContext<ContentListModel>(
                        FetchMoreResources,
                        item.Number,
                        15,
                        HomeHost,
                        InitSelector.Special);
                cacheDic.Add(item.Title, homeLlistResources); } 
            ListResources.Source = cacheDic[nowItem];
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e) {
            ListResources.Source =
                cacheDic[nowItem] =
                new DQDDataContext<ContentListModel>(
                    FetchMoreResources,
                    itemNumber,
                    15,
                    HomeHost,
                    InitSelector.Special);
        }

        private void BackToTopBtn_Click(object sender, RoutedEventArgs e) {
            int num = MyPivot.SelectedIndex;
            MainPage.GetScrollViewer(
                MainPage.GetPVItemViewer(
                    MyPivot, ref num))
                    .ChangeView(0, 0, 1);
        }

        private void grid_SizeChanged(object sender, SizeChangedEventArgs e) { MyPivot.Width = (sender as Grid).ActualWidth; }

        private void LocalPageListView_Loaded(object sender, RoutedEventArgs e) {
            var num = MyPivot.SelectedIndex;
            scroll = MainPage.GetScrollViewer(MainPage.GetPVItemViewer(MyPivot, ref num));
            scroll.ViewChanged += ScrollViewer_ViewChanged;
            scroll.ViewChanged += ScrollViewerChangedForFlip;
            loadingAnimation.IsActive = false;
        }

        private void FlipInnerButton_Click(object sender, RoutedEventArgs e) {
            var itemUri = (sender as Button).Content as Uri;
            var itemNum = (int)((sender as Button).CommandParameter);
            MainPage.Current.ItemClick?.Invoke(
                this,
                typeof(ContentPage),
                MainPage.Current.contentFrame,
                itemUri,
                itemNum,
                null);
            MainPage.Current.SideGrid.Visibility = Visibility.Visible;
        }

        #endregion

        #region Animations

        #region Animations Properties
        public StackPanel ButtonThisPage { get; private set; }
        private ScrollViewer scroll;
        private Dictionary<string, double> listViewOffset = new Dictionary<string, double>();
        Storyboard BtnStackSlideIn = new Storyboard();
        Storyboard BtnStackSlideOut = new Storyboard();
        TranslateTransform transToButtonThisPage;
        DoubleAnimation doubleAnimation;
        bool IsAnimaEnabled;
        #endregion

        #region Handler of ListView Scroll 

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e) {
            try {
                if (!listViewOffset.ContainsKey(nowItem)) {
                    listViewOffset[nowItem] = (sender as ScrollViewer).VerticalOffset;
                    return; }
                if (listViewOffset[nowItem] - (sender as ScrollViewer).VerticalOffset < -10
                    && ButtonThisPage.Visibility == Visibility.Visible
                    && IsAnimaEnabled) {
                    scroll.ViewChanged -= ScrollViewer_ViewChanged;
                    BtnStackSlideOut.Begin(); }
                if (listViewOffset[nowItem] - (sender as ScrollViewer).VerticalOffset > 10
                    && ButtonThisPage.Visibility == Visibility.Collapsed
                    && IsAnimaEnabled) {
                    scroll.ViewChanged -= ScrollViewer_ViewChanged;
                    ButtonThisPage.Visibility = Visibility.Visible;
                    BtnStackSlideIn.Begin(); }
                listViewOffset[nowItem] = (sender as ScrollViewer).VerticalOffset;
            } catch { Debug.WriteLine("Save scroll positions error."); }
        }

        private void ScrollViewerChangedForFlip(object sender, ScrollViewerViewChangedEventArgs e) {
            try {
                if ((sender as ScrollViewer).VerticalOffset <= 240)
                    MyFlip.Margin = new Thickness(0, -(sender as ScrollViewer).VerticalOffset, 0, 0);
                if ((sender as ScrollViewer).VerticalOffset > 240)
                    MyFlip.Margin = new Thickness(0, -240, 0, 0);
            } catch { Debug.WriteLine("Save scroll positions error."); }
        }

        #endregion

        #region Animation Methods

        private void InitFloatButtonView() {
            ButtonThisPage = MainPage.Current.IsButtonShadowVisible ? ButtonStack : ButtonStackNoShadow;
            int num = MyPivot.SelectedIndex;
            scroll = MainPage.GetScrollViewer(MainPage.GetPVItemViewer(MyPivot, ref num));
            scroll.ViewChanged += ScrollViewerChangedForFlip;
            if (MainPage.Current.IsFloatButtonEnable) {
                ButtonStack.Visibility = VisiEnumHelper.GetVisibility(MainPage.Current.IsButtonShadowVisible);
                ButtonStackNoShadow.Visibility = VisiEnumHelper.GetVisibility(!MainPage.Current.IsButtonShadowVisible);
                if (MainPage.Current.IsButtonAnimationEnable) {
                    scroll.ViewChanged += ScrollViewer_ViewChanged;
                    InitStoryBoard();
                }
            } else {
                ButtonStack.Visibility = VisiEnumHelper.GetVisibility(false);
                ButtonStackNoShadow.Visibility = VisiEnumHelper.GetVisibility(false);
                transToButtonThisPage = null;
            }
        }

        public void HandleAnimation(bool isAnima) { if (isAnima) { InitStoryBoard(); } else { DestoryAnimation(); }}

        private  void DestoryAnimation() { IsAnimaEnabled = false; }

        private void InitStoryBoard() {
            IsAnimaEnabled = true;
            transToButtonThisPage = ButtonThisPage.RenderTransform as TranslateTransform;
            if (transToButtonThisPage == null) ButtonThisPage.RenderTransform = transToButtonThisPage = new TranslateTransform();
            doubleAnimation = new DoubleAnimation() {
                Duration = new Duration(TimeSpan.FromMilliseconds(520)),
                From = -100,
                To = 0,
            };
            doubleAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
            doubleAnimation.Completed += DoublAnimationIn_Completed;
            BtnStackSlideIn = new Storyboard();

            Storyboard.SetTarget(doubleAnimation, transToButtonThisPage);
            Storyboard.SetTargetProperty(doubleAnimation, "X");
            BtnStackSlideIn.Children.Add(doubleAnimation);
            doubleAnimation = new DoubleAnimation() {
                Duration = new Duration(TimeSpan.FromMilliseconds(520)),
                From = 0,
                To = -100,
            };
            doubleAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
            doubleAnimation.Completed += DoublAnimationOut_Completed;
            BtnStackSlideOut = new Storyboard();
            Storyboard.SetTarget(doubleAnimation, transToButtonThisPage);
            Storyboard.SetTargetProperty(doubleAnimation, "X");
            BtnStackSlideOut.Children.Add(doubleAnimation);
        }

        #endregion

        #region Animation Events
        private void DoublAnimationIn_Completed(object sender, object e) {
            scroll.ViewChanged += ScrollViewer_ViewChanged;
        }

        private void DoublAnimationOut_Completed(object sender, object e) {
            ButtonThisPage.Visibility = Visibility.Collapsed;
            scroll.ViewChanged += ScrollViewer_ViewChanged;
        }
        #endregion

        #endregion

        #region Properties and States

        public static HomePage Current { get; private set; }
        public StackPanel ButtonShadow { get; private set; }
        public StackPanel ButtonNoShadow { get; private set; }
        private ProgressRing loadingAnimation;
        private Dictionary<string, DQDDataContext<ContentListModel>> cacheDic;
        private DQDDataContext<ContentListModel> homeLlistResources;
        private const string HomeHost = "http://www.dongqiudi.com/";
        private const string HomeHostInsert = "http://www.dongqiudi.com";
        private string nowItem;
        private int itemNumber;

        #endregion

    }
}
