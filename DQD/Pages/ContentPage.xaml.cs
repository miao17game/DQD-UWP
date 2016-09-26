using DQD.Core.Controls;
using DQD.Core.DataVirtualization;
using DQD.Core.Helpers;
using DQD.Core.Models;
using DQD.Core.Models.CommentModels;
using DQD.Core.Models.PageContentModels;
using DQD.Core.Tools;
using DQD.Net.Base;
using ImageLib;
using ImageLib.Cache.Memory;
using ImageLib.Cache.Storage;
using ImageLib.Cache.Storage.CacheImpl;
using ImageLib.Controls;
using ImageLib.Gif;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Profile;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace DQD.Net.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ContentPage : BaseContentPage {

        #region Constructor

        public ContentPage() {
            Current = this;
            loadingAnimation = MainPage.Current.LoadingProgress;
            loadingAnimation.IsActive = true;
            this.Opacity = 0;
            this.InitializeComponent();
            ButtonShadow = ButtonStack;
            ButtonNoShadow = ButtonStackNoShadow;
            InitImageLoader();
            isAutoLoadImages = (bool?)SettingsHelper.ReadSettingsValue(SettingsConstants.IsPicturesAutoLoad) ?? true;
            fontSize = (double?)SettingsHelper.ReadSettingsValue(SettingsConstants.TextFieldSize) ?? 14;
        }

        #endregion

        #region Events

        /// <summary>
        /// trigger the event when this frame is first navigated
        /// </summary>
        /// <param name="e">navigate args</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            var parameter = e.Parameter as ParameterNavigate;
            HostSource = parameter.Uri;
            HostNumber = parameter.Number;
            if (HostSource == null) return;
            await HandleHtmlResources();
            if (StatusBarInit.HaveAddMobileExtensions()) { BackBtn.Visibility = Visibility.Collapsed; TitleScroll.Margin = new Thickness(15, 0, 0, 0); }
            loadingAnimation.IsActive = false;
            this.Opacity = 1;
            base.InitStoryBoard();
            InitFloatButtonView();
        }

        private async Task HandleHtmlResources() {
            var urlString = await WebProcess.GetHtmlResources(HostSource.ToString());
            AddChildrenToStackPanel(urlString.ToString());
            AddChildrenToCommentsStack(urlString.ToString());
            //var timer = new DispatcherTimer();
            //timer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            //timer.Tick += (sender, args) => { TitleScroll.ChangeView(TitleScroll.HorizontalOffset + 1, 0, 1); };
            //timer.Start();
        }

        private void MoreCommentsBtn_Click(object sender, RoutedEventArgs e) {
            if (CommentsResources.Source == null) {
                var sources = new DQDDataContext<AllCommentModel>(FetchMoreResources, HostNumber, 30, targetHost, InitSelector.Default);
                CommentsResources.Source = sources; }
            PopupAllComments.IsOpen = true;
            PopupBackBorder.Visibility = Visibility.Visible;
            EnterPopupBorder.Begin();
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e) {
            storyToSideGridOut.Begin();
        }

        private void PopupAllComments_SizeChanged(object sender, SizeChangedEventArgs e) {
            InnerGrid.Width = (sender as Popup).ActualWidth;
            InnerGrid.Height = (sender as Popup).ActualHeight;
        }

        private void CloseAllComsBtn_Click(object sender, RoutedEventArgs e) {
            PopupAllComments.IsOpen = false;
        }

        private void PopupAllComments_Closed(object sender, object e) {
            OutPopupBorder.Completed += OnOutPopupBorderOut;
            OutPopupBorder.Begin();
        }

        private void OnOutPopupBorderOut(object sender, object e) {
            OutPopupBorder.Completed -= OnOutPopupBorderOut;
            PopupBackBorder.Visibility = Visibility.Collapsed;
        }

        private void BackToTopBtn_Click(object sender, RoutedEventArgs e) {
            ContentScroll.ChangeView(0, 0, 1);
        }

        private async void RefreshBtn_Click(object sender, RoutedEventArgs e) {
            loadingAnimation.IsActive = true;
            ContentStack.Children.Clear();
            CommentsStack.Children.Clear();
            await HandleHtmlResources();
            loadingAnimation.IsActive = false;
        }

        #endregion

        #region Methods

        public void ChangeTextFieldSize(double newSize) {
            foreach(var item in InsideResources.GetBlocksList()) {
                item.FontSize = newSize;
            }
        }

        private static void InitImageLoader() {
            ImageLoader.Initialize(new ImageConfig.Builder() {
                CacheMode = ImageLib.Cache.CacheMode.MemoryAndStorageCache,
                MemoryCacheImpl = new LRUCache<string, IRandomAccessStream>(),
                StorageCacheImpl = new LimitedStorageCache(
                    ApplicationData.Current.LocalCacheFolder,
                    "cache", 
                    new SHA1CacheGenerator(), 
                    1024 * 1024 * 1024)
            }.AddDecoder<GifDecoder>().Build(), false);
        }

        private async Task<ObservableCollection<AllCommentModel>> FetchMoreResources(int number, uint rollNum, uint nowWholeCountX) {
            targetHost = string.Format(targetHost, number, nowWholeCountX / rollNum);
            return await DataProcess.GetPageAllComments(targetHost);
        }

        /// <summary>
        /// Add children to the content stackpanel
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private void AddChildrenToStackPanel(string value) {
            var PConModel = DataProcess.GetPageInnerContent(value);
            ContentTitle.Text = PConModel.Title;
            ContentAuthor.Text = "来源：" + PConModel.Author;
            ContentDate.Text = PConModel.Date;
            int num = PConModel.ContentImage.Count +
                PConModel.ContentString.Count +
                PConModel.ContentGif.Count +
                PConModel.ContentVideo.Count +
                PConModel.ContentFlash.Count +
                PConModel.ContentSelfUri.Count;
            for (int index = 1; index <= num; index++) {
                object item = default(object);
                ContentType type =
                    (item = PConModel.ContentString.Find(i => i.Index == index)) != null ? ContentType.String :
                    (item = PConModel.ContentImage.Find(i => i.Index == index)) != null ? ContentType.Image :
                    (item = PConModel.ContentGif.Find(i => i.Index == index)) != null ? ContentType.Gif :
                    (item = PConModel.ContentVideo.Find(i => i.Index == index)) != null ? ContentType.Video :
                    (item = PConModel.ContentFlash.Find(i => i.Index == index)) != null ? ContentType.Flash :
                    (item = PConModel.ContentSelfUri.Find(i => i.Index == index)) != null ? ContentType.SelfUri :
                    ContentType.None;

                switch (type) {
                    case ContentType.String:
                        var textBlock = new TextBlock {
                            Text = (item as ContentStrings).Content,
                            TextWrapping = TextWrapping.WrapWholeWords,
                            Margin = new Thickness(2, 3, 2, 3),
                            FontSize = fontSize,
                        };
                        ContentStack.Children.Add(textBlock);
                        InsideResources.AddTextBlockInList(textBlock);
                        break;

                    case ContentType.Image:
                        if (isAutoLoadImages) {
                            ContentStack.Children.Add(new Image {
                                Source = (item as ContentImages).Image,
                                Margin = new Thickness(5, 5, 5, 5),
                                Stretch = Stretch.UniformToFill,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                MinHeight = 200,
                                MinWidth = 300,
                            });
                        } else {
                            var gridImg = new Grid {
                                Margin = new Thickness(5, 5, 5, 5),
                                MinHeight = 200,
                                MinWidth = 300,
                                HorizontalAlignment = HorizontalAlignment.Center,
                            };
                            var btnImg = new Button {
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                VerticalAlignment = VerticalAlignment.Stretch,
                                Content = "点击加载图片",
                            };
                            btnImg.CommandParameter = (item as ContentImages).Image;
                            btnImg.Click += (sender, args) => {
                                (sender as Button).Visibility = Visibility.Collapsed;
                                gridImg.Children.Add(new Image {
                                    Source = (sender as Button).CommandParameter as ImageSource,
                                    Stretch = Stretch.UniformToFill,
                                });
                            };
                            gridImg.Children.Add(btnImg);
                            ContentStack.Children.Add(gridImg);
                        }
                        break;

                    case ContentType.Gif:
                        var grid = new Grid {
                            Margin = new Thickness(5, 5, 5, 5),
                            MinHeight = 200,
                            MinWidth = 300,
                            HorizontalAlignment = HorizontalAlignment.Center,
                        };
                        var btn = new Button {
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch,
                        };
                        if (isAutoLoadImages) {
                            btn.Content = "正在加载GIF";
                            btn.IsEnabled = false;
                            var timerForImage = new DispatcherTimer();
                            timerForImage.Interval = new TimeSpan(0, 0, 0, 1);
                            int numberForLoad = 0;
                            timerForImage.Tick += (sender, args) => {
                                if (isAutoGifFree || numberForLoad >= 15) {
                                    isAutoGifFree = false;
                                    btn.Visibility = Visibility.Collapsed;
                                    var gif = new ImageView {
                                        Margin = new Thickness(5, 5, 5, 5),
                                        MinHeight = 200,
                                        MinWidth = 300,
                                        HorizontalAlignment = HorizontalAlignment.Center,
                                        UriSource = (item as ContentGifs).ImageUri,
                                        Stretch = Stretch.UniformToFill,
                                    };
                                    gif.LoadingCompleted += (control, e) => { isAutoGifFree = true; };
                                    grid.Children.Add(gif);
                                    timerForImage.Stop();
                                } else { numberForLoad++;}
                            };
                            timerForImage.Start();
                        } else {
                            btn.Content = "点击加载GIF";
                            btn.CommandParameter = (item as ContentGifs).ImageUri;
                            btn.Click += (sender, args) => {
                                (sender as Button).Visibility = Visibility.Collapsed;
                                grid.Children.Add(new ImageView {
                                    UriSource = (sender as Button).CommandParameter as Uri,
                                    Stretch = Stretch.UniformToFill,
                                });
                            };
                        }
                        grid.Children.Add(btn);
                        ContentStack.Children.Add(grid);
                        break;

                    case ContentType.Video:
                        ContentStack.Children.Add(new MediaElement {
                            Source = (item as ContentVideos).VideoUri,
                            Margin = new Thickness(5),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            MinHeight = 200,
                            MinWidth = 300,
                            AreTransportControlsEnabled = true,
                            AutoPlay = false,
                        });
                        break;

                    case ContentType.Flash:
                        ContentStack.Children.Add(new MediaElement {
                            Source = (item as ContentFlashs).FlashUri,
                            Margin = new Thickness(5),
                            AreTransportControlsEnabled = true,
                            AutoPlay = false,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            MinHeight = 200,
                            MinWidth = 300,
                        });
                        ContentStack.Children.Add(!AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Mobile") ? new TextBlock {
                            Text = "若不支持，请转到浏览器查看",
                            Margin = new Thickness(5),
                            FontSize = 12,
                        } : null);
                        ContentStack.Children.Add(new HyperlinkButton {
                            NavigateUri = (item as ContentFlashs).FlashUri,
                            Margin = new Thickness(5),
                            Content = ContentTitle.Text,
                            Foreground = (Brush)Application.Current.Resources["DQDBackground"],
                            FontSize = 12,
                        });
                        break;

                    case ContentType.SelfUri:
                        var button = new Button {
                            Margin = new Thickness(5),
                            Content = (item as ContentSelfUris).Title,
                            Foreground = (Brush)Application.Current.Resources["DQDBackground"],
                            Background = null,
                        };
                        button.Click += (obj, args) => {
                            MainPage.Current.ItemClick?.Invoke(
                                this,
                                typeof(ContentPage),
                                MainPage.Current.ContFrame,
                                (item as ContentSelfUris).Uri,
                                (item as ContentSelfUris).Number,
                                null);
                            MainPage.Current.SideGrid.Visibility = Visibility.Visible;
                        };
                        ContentStack.Children.Add(button);
                        break;

                    case ContentType.None: break;
                }
            }
        }

        /// <summary>
        /// Add children to the Topcomments stackpanel
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private void AddChildrenToCommentsStack(string value) {
            var PConModel = DataProcess.GetPageTopComments(value);
            foreach (var item in PConModel) {
                CommentsStack.Children.Add(new CommentPanel {
                    ComContent = item.Content,
                    ComImage = item.Image,
                    ComName = item.Name,
                    ComTime = item.Time,
                    ComZan = item.Zan,
                });
            }
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

        #region Static Inside Class

        static class InsideResources {

            public static void AddTextBlockInList(TextBlock textBlock) { BlocksList.Add(textBlock); }
            public static List<TextBlock> GetBlocksList() { return BlocksList; }
            static public List<TextBlock> BlocksList = new List<TextBlock>();

        }

        #endregion

        #region Properties and State

        public static ContentPage Current { get; private set; }
        public StackPanel ButtonShadow { get; private set; }
        public StackPanel ButtonNoShadow { get; private set; }
        private string targetHost = "http://dongqiudi.com/article/{0}?page={1}#comment_anchor";
        private enum ContentType { None = 0, String = 1, Image = 2, Gif = 3, Video = 4, Flash = 5, SelfUri = 6 }
        private Uri HostSource;
        private int HostNumber;
        private bool isAutoLoadImages;
        private bool isAutoGifFree = true;
        private double fontSize;
        private ProgressRing loadingAnimation;

        #endregion
    }
}
