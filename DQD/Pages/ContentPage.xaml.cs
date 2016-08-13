using DQD.Core.Controls;
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace DQD.Net.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ContentPage : Page {
        #region Constructor

        public ContentPage() {
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
            var value = e.Parameter as Uri;
            if (value == null)
                return;
            var urlString = await WebProcess.GetHtmlResources(value.ToString());
            AddChildrenToStackPanel(urlString.ToString());
            AddChildrenToCommentsStack(urlString.ToString());
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
            int num = PConModel.ContentImage.Count + PConModel.ContentString.Count + PConModel.ContentGif.Count;
            for (int index = 1; index <= num; index++) {
                object item = default(object);
                ContentType type =
                    (item = PConModel.ContentString.Find(i => i.Index == index)) != null ? ContentType.String :
                    (item = PConModel.ContentImage.Find(i => i.Index == index)) != null ? ContentType.Image :
                    (item = PConModel.ContentGif.Find(i => i.Index == index)) != null ? ContentType.Gif :
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
                            Stretch = Stretch.UniformToFill
                        });
                        break;
                    case ContentType.Gif:
                        ContentStack.Children.Add(new ImageView {
                            UriSource = (item as ContentGifs).ImageUri,
                            Margin = new Thickness(5, 5, 5, 5),
                            Stretch = Stretch.UniformToFill
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
            foreach (var item in PConModel) {
                CommentsStack.Children.Add(new CommentPanel {
                    ComContent = item.Content,
                    ComImage = item.Image,
                    ComName = item.Name,
                    ComTime = item.Time,
                });
            }
        }

        #endregion

        #region Properties and State

        private enum ContentType { String = 1, Image = 2, Gif = 3 ,None = 4, }

        #endregion
    }
}
