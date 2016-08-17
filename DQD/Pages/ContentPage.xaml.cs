using DQD.Core.Controls;
using DQD.Core.DataVirtualization;
using DQD.Core.Models;
using DQD.Core.Models.CommentModels;
using DQD.Core.Models.PageContentModels;
using DQD.Core.Tools;
using ImageLib;
using ImageLib.Cache.Memory;
using ImageLib.Cache.Storage;
using ImageLib.Cache.Storage.CacheImpl;
using ImageLib.Controls;
using ImageLib.Gif;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
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
    public sealed partial class ContentPage : Page {
        #region Constructor

        public ContentPage() {
            transToSideGrid = this.RenderTransform as TranslateTransform;
            if (transToSideGrid == null) this.RenderTransform = transToSideGrid = new TranslateTransform();
            this.Opacity = 0;
            this.InitializeComponent();
            InitImageLoader();
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
            if (HostSource == null)
                return;
            var urlString = await WebProcess.GetHtmlResources(HostSource.ToString());
            AddChildrenToStackPanel(urlString.ToString());
            AddChildrenToCommentsStack(urlString.ToString());
            if (StatusBarInit.IsTargetMobile()) { BackBtn.Visibility = Visibility.Collapsed; ContentTitle.Margin = new Thickness(15,0,0,0); }
        }

        private void MoreCommentsBtn_Click(object sender, RoutedEventArgs e) {
            if (CommentsResources.Source == null) {
                var sources = new DQDDataContext<AllCommentModel>(FetchMoreResources, HostNumber, 30, targetHost, InitSelector.Default);
                CommentsResources.Source = sources;
            }
            PopupAllComments.IsOpen = true;
            PopupBackBorder.Visibility = Visibility.Visible;
            EnterPopupBorder.Begin();
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e) {
            MainPage.Current.SideGrid.Visibility = Visibility.Collapsed;
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

        #endregion

        #region Methods

        private static void InitImageLoader() {
            ImageLoader.Initialize(new ImageConfig.Builder() {
                CacheMode = ImageLib.Cache.CacheMode.MemoryAndStorageCache,
                MemoryCacheImpl = new LRUCache<string, IRandomAccessStream>(),
                StorageCacheImpl = new LimitedStorageCache(ApplicationData.Current.LocalCacheFolder,
                            "cache", new SHA1CacheGenerator(), 1024 * 1024 * 1024)
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
            ContentStack.Children.Clear();
            int num = PConModel.ContentImage.Count + 
                PConModel.ContentString.Count + 
                PConModel.ContentGif.Count + 
                PConModel.ContentVideo.Count + 
                PConModel.ContentFlash.Count ;
            for (int index = 1; index <= num; index++) {
                object item = default(object);
                ContentType type =
                    (item = PConModel.ContentString.Find(i => i.Index == index)) != null ? ContentType.String :
                    (item = PConModel.ContentImage.Find(i => i.Index == index)) != null ? ContentType.Image :
                    (item = PConModel.ContentGif.Find(i => i.Index == index)) != null ? ContentType.Gif :
                    (item = PConModel.ContentVideo.Find(i => i.Index == index)) != null ? ContentType.Video :
                    (item = PConModel.ContentFlash.Find(i => i.Index == index)) != null ? ContentType.Flash :
                    ContentType.None;
                switch (type) {
                    case ContentType.String:
                        ContentStack.Children.Add(new TextBlock {
                            Text = (item as ContentStrings).Content,
                            TextWrapping = TextWrapping.WrapWholeWords,
                            Margin = new Thickness(2, 3, 2, 3),
                            FontSize = 14,
                        });
                        break;
                    case ContentType.Image:
                        ContentStack.Children.Add(new Image {
                            Source = (item as ContentImages).Image,
                            Margin = new Thickness(5, 5, 5, 5),
                            Stretch = Stretch.UniformToFill,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            MinHeight = 200,
                            MinWidth = 300,
                        });
                        break;
                    case ContentType.Gif:
                        ContentStack.Children.Add(new ImageView {
                            UriSource = (item as ContentGifs).ImageUri,
                            Margin = new Thickness(5, 5, 5, 5),
                            Stretch = Stretch.UniformToFill,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            MinHeight = 200,
                            MinWidth = 300,
                        });
                        break;
                    case ContentType.Video:
                        ContentStack.Children.Add(new MediaElement {
                            Source = (item as ContentVideos).VideoUri,
                            Margin = new Thickness(5),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            MinHeight = 200,
                            MinWidth = 300,
                            AreTransportControlsEnabled = true,
                        });
                        break;
                    case ContentType.Flash:
                        ContentStack.Children.Add(new MediaElement {
                            Source = (item as ContentFlashs).FlashUri,
                            Margin = new Thickness(5),
                            AreTransportControlsEnabled = true,
                            AutoPlay = false,
                            HorizontalAlignment=HorizontalAlignment.Center,
                            MinHeight = 200,
                            MinWidth = 300,
                        });
                        ContentStack.Children.Add(new TextBlock {
                            Text = "若不支持Youku视频播放，请转到浏览器查看" ,
                            Margin = new Thickness(5),
                            FontSize = 12,
                        });
                        ContentStack.Children.Add(new HyperlinkButton {
                            NavigateUri = (item as ContentFlashs).FlashUri,
                            Margin = new Thickness(5),
                            Content = ContentTitle.Text,
                            Foreground = ((Brush)Application.Current.Resources["DQDBackground"]),
                            FontSize = 12,
                        });
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
            CommentsStack.Children.Clear();
            foreach (var item in PConModel) {
                CommentsStack.Children.Add(new CommentPanel {
                    ComContent = item.Content,
                    ComImage = item.Image,
                    ComName = item.Name,
                    ComTime = item.Time,
                });
            }
            this.Opacity = 1;
            InitStoryBoard();
        }

        #endregion

        #region Animations
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
            } ;
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

        #region Properties and State

        private string targetHost = "http://dongqiudi.com/article/{0}?page={1}#comment_anchor"; 
        private enum ContentType { None = 0, String = 1, Image = 2, Gif = 3 , Video = 4, Flash = 5}
        private Uri HostSource;
        private int HostNumber;

        #endregion
    }
}
