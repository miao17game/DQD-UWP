using DQD.Core.DataVirtualization;
using DQD.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;

namespace DQD.Net.Pages {
    /// <summary>
    /// VideoPage Code Page
    /// </summary>
    public sealed partial class VideoPage:Page {
        public VideoPage() {
            Current = this;
            cacheDic = new Dictionary<string, DQDDataContext<ContentListModel>>();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            this.InitializeComponent();
            InitView();
        }

        #region Specifc Methods

        /// <summary>
        /// Init the Page Initual databundle resources.
        /// </summary>
        private async void InitView() {
            ObservableCollection<HeaderModel> headerList = new ObservableCollection<HeaderModel>();
            headerList = await DataHandler.SetSpecialHeaderGroupResources(HomeHostInsert);
            foreach (var item in headerList) { cacheDic.Add(item.Title, new DQDDataContext<ContentListModel>(FetchMoreResources, item.Number, 15, HomeHost, InitSelector.Special)); }
            HeaderResources.Source = headerList;
        }

        private async Task<ObservableCollection<ContentListModel>> FetchMoreResources( int number, uint rollNum, uint nowWholeCountX) {
            var Host = "http://www.dongqiudi.com/video?tab={0}&page={1}";
            Host = string.Format(Host, number, nowWholeCountX / rollNum);
            return await DataHandler.SetHomeListResources(Host);
        }

        #endregion

        #region Events

        private void ListView_ItemClick(object sender, ItemClickEventArgs e) {
            var itemUri = (e.ClickedItem as ContentListModel).Path;
            var itemNum = (e.ClickedItem as ContentListModel).ID;
            MainPage.Current.ItemClick?.Invoke(this, typeof(ContentPage), MainPage.Current.contentFrame, itemUri, itemNum);
            MainPage.Current.SideGrid.Visibility = Visibility.Visible;
        }

        private void MyPivot_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var item = (sender as Pivot).SelectedItem as HeaderModel;
            NowItem = item.Title;
            if (!cacheDic.ContainsKey(item.Title)) {
                HomeLlistResources = new DQDDataContext<ContentListModel>(FetchMoreResources, item.Number, 15, HomeHost, InitSelector.Special);
                cacheDic.Add(item.Title, HomeLlistResources);
            }
            ListResources.Source = cacheDic[NowItem];
        }

        private void grid_SizeChanged(object sender, SizeChangedEventArgs e) { MyPivot.Width = (sender as Grid).ActualWidth; }

        #endregion

        #region Properties and States

        public static VideoPage Current;
        private string NowItem;
        private Dictionary<string, DQDDataContext<ContentListModel>> cacheDic;
        private DQDDataContext<ContentListModel> HomeLlistResources;
        private const string HomeHost = "http://www.dongqiudi.com/video/";
        private const string HomeHostInsert = "http://www.dongqiudi.com/video";

        #endregion
    }
}
