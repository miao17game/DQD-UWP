using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using DQD.Core.Models;
using DQD.Core.Tools;
using HtmlAgilityPack;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using DQD.Core.DataVirtualization;
using DQD.Core.Controls;

namespace DQD.Net.Pages {
    /// <summary>
    /// HomePage Code Page
    /// </summary>
    public sealed partial class HomePage:Page {
        public static HomePage Current;
        private ListView thisList;
        private string NowItem;
        private Dictionary<string,IncrementalLoading<ContentListModel>> cacheDic;
        private Dictionary<string, double> ListViewOffset;
        private IncrementalLoading<ContentListModel>HomeLlistResources;
        private const string HomeHost = "http://www.dongqiudi.com/";
        private const string HomeHostInsert = "http://www.dongqiudi.com";

        public HomePage() {
            Current = this;
            cacheDic = new Dictionary<string, IncrementalLoading<ContentListModel>>();
            ListViewOffset = new Dictionary<string, double>();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            this.InitializeComponent();
            InitView();
        }

        /// <summary>
        /// Init the Page Initual databundle resources.
        /// </summary>
        private async void InitView() {
            ObservableCollection<HeaderModel> headerList = new ObservableCollection<HeaderModel>();
            headerList = await DataHandler.SetHeaderGroupResources();
            HeaderResources.Source = headerList;
            if (HomeLlistResources != null)
                HomeLlistResources.CollectionChanged -= _employees_CollectionChanged;
            HomeLlistResources = new IncrementalLoading<ContentListModel>();
            HomeLlistResources.CollectionChanged += _employees_CollectionChanged;
            ListResources.Source = HomeLlistResources;
        }

        void _employees_CollectionChanged(object sender,System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
           //Debug.WriteLine(string.Format("Collection was changed. Count = {0}",listResources.Count));
        }

        private void ContainerContentChanging(ListViewBase sender,ContainerContentChangingEventArgs args) {
            if(!args.InRecycleQueue) {
                FrameworkElement ctr = (FrameworkElement)args.ItemContainer.ContentTemplateRoot;
                if(ctr!=null) {
                    TextBlock t = (TextBlock)ctr.FindName("idx");
                    t.Text=args.ItemIndex.ToString();
                }
            }
        }

        private void ListView_ItemClick(object sender,ItemClickEventArgs e) {
            var itemUri = (e.ClickedItem as ContentListModel).Path;
            MainPage.Current.ItemClick ?.Invoke(this, typeof(ContentPage), MainPage.Current.contentFrame, itemUri);
        }

        private void MyPivot_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var item = (sender as PersonalPivot).SelectedItem as HeaderModel;
            NowItem = item.Title;
            if (cacheDic.ContainsKey(item.Title)) {
                double numSave = ListViewOffset.ContainsKey(item.Title) ? ListViewOffset[item.Title] : 0;
                ListResources.Source = cacheDic[item.Title];
                if (thisList != null) {
                    var vierew = GetScrollViewer(thisList);
                    vierew.ChangeView(0, numSave, 1);
                }
            }
            else {
                var list = new IncrementalLoading<ContentListModel>(DataHandler.SetHomeListResources, item.Number, HomeHost);
                list.CollectionChanged += _employees_CollectionChanged;
                cacheDic.Add(item.Title, list);
                ListResources.Source = list;
            }
        }

        #region Save the position of listview scroll
        /// <summary>
        /// Get scrollviewer from the target listview
        /// </summary>
        /// <param name="depObj">dependencyObject of Listview or is the target scrollviewer</param>
        /// <returns></returns>
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

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e) {
            try {
                ListViewOffset[NowItem] = (sender as ScrollViewer).VerticalOffset;
                Debug.WriteLine(ListViewOffset[NowItem]);
            } catch { Debug.WriteLine("Save scroll positions error."); }
        }

        private void LocalPageListView_Loaded(object sender, RoutedEventArgs e) {
            thisList = sender as ListView;
            var vierew = GetScrollViewer(thisList);
            vierew.ViewChanged += ScrollViewer_ViewChanged;
            if (string.IsNullOrEmpty(NowItem))
                return;
            if (!ListViewOffset.ContainsKey(NowItem))
                return;
            vierew.ChangeView(0,ListViewOffset[NowItem],1);
        }
        #endregion
    }
}
