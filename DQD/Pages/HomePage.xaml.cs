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

namespace DQD.Net.Pages {
    /// <summary>
    /// HomePage Code Page
    /// </summary>
    public sealed partial class HomePage:Page {

        #region Constructor
        public HomePage() {
            Current = this;
            cacheDic = new Dictionary<string, DQDDataContext<ContentListModel>>();
            ListViewOffset = new Dictionary<string, double>();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            this.InitializeComponent();
            InitView();
        }
        #endregion

        #region Methods

        /// <summary>
        /// Init the Page Initual databundle resources.
        /// </summary>
        private async void InitView() {
            ObservableCollection<HeaderModel> headerList = new ObservableCollection<HeaderModel>();
            headerList = await DataHandler.SetHeaderGroupResources();
            foreach (var item in headerList) {
                cacheDic.Add(
                    item.Title, 
                    new DQDDataContext<ContentListModel>(
                        FetchMoreResources,
                        item.Number, 
                        15, 
                        HomeHost, 
                        InitSelector.Special));}
            HeaderResources.Source = headerList;
        }

        private async Task<ObservableCollection<ContentListModel>> FetchMoreResources(int number, uint rollNum, uint nowWholeCountX) {
            var Host = "http://www.dongqiudi.com?tab={0}&page={1}";
            Host = string.Format(
                Host, 
                number,
                nowWholeCountX / 15);
            return await DataHandler.SetHomeListResources(Host);
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e) {
            ListResources.Source =
                cacheDic[NowItem] =
                new DQDDataContext<ContentListModel>(
                    FetchMoreResources,
                    itemNumber,
                    15,
                    HomeHost,
                    InitSelector.Special);
        }

        private void BackToTopBtn_Click(object sender, RoutedEventArgs e) {
            int num = MyPivot.SelectedIndex;
            GetScrollViewer(
                GetPVItemViewer(
                    MyPivot, ref num))
                    .ChangeView(0, 0, 1);
        }

        #region Handler of ListView Scroll 

        public ScrollViewer GetScrollViewer(DependencyObject depObj) {
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

        public PivotItem GetPVItemViewer(DependencyObject depObj, ref int num) {
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

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e) {
            try {
                ListViewOffset[NowItem] = (sender as ScrollViewer).VerticalOffset;
                Debug.WriteLine(ListViewOffset[NowItem]);
            } catch { Debug.WriteLine("Save scroll positions error."); }
        }

        #endregion

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
            NowItem = item.Title;
            itemNumber = item.Number;
            if (!cacheDic.ContainsKey(item.Title)) {
                HomeLlistResources = 
                    new DQDDataContext<ContentListModel>(
                        FetchMoreResources, 
                        item.Number, 
                        15, 
                        HomeHost,
                        InitSelector.Special);
                cacheDic.Add(item.Title, HomeLlistResources); }
            ListResources.Source = cacheDic[NowItem];
        }

        private void grid_SizeChanged(object sender, SizeChangedEventArgs e) { MyPivot.Width = (sender as Grid).ActualWidth; }

        private void LocalPageListView_Loaded(object sender, RoutedEventArgs e) {

        }

        #endregion

        #region Properties and States

        public static HomePage Current;
        //private ListView thisList;
        private string NowItem;
        private int itemNumber;
        private Dictionary<string, DQDDataContext<ContentListModel>> cacheDic;
        private Dictionary<string, double> ListViewOffset;
        private DQDDataContext<ContentListModel> HomeLlistResources;
        private const string HomeHost = "http://www.dongqiudi.com/";
        private const string HomeHostInsert = "http://www.dongqiudi.com";

        #endregion
    }
}
