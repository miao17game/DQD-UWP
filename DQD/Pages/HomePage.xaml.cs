﻿using System;
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
using Windows.System.Profile;

namespace DQD.Net.Pages {
    /// <summary>
    /// HomePage Code Page
    /// </summary>
    public sealed partial class HomePage:Page {
        public static HomePage Current;
        //private ListView thisList;
        private string NowItem;
        private Dictionary<string,DQDLoadContext<ContentListModel>> cacheDic;
        private Dictionary<string, double> ListViewOffset;
        private DQDLoadContext<ContentListModel>HomeLlistResources;
        private const string HomeHost = "http://www.dongqiudi.com/";
        private const string HomeHostInsert = "http://www.dongqiudi.com";

        public HomePage() {
            Current = this;
            cacheDic = new Dictionary<string, DQDLoadContext<ContentListModel>>();
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
            foreach (var item in headerList) { cacheDic.Add(item.Title, new DQDLoadContext<ContentListModel>(item.Number, HomeHost)); }
            HeaderResources.Source = headerList;
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
            if (AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Mobile"))
                MainPage.Current.baseGrid.Visibility = Visibility.Collapsed;
        }

        private void MyPivot_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var item = (sender as Pivot).SelectedItem as HeaderModel;
            NowItem = item.Title;
            if (cacheDic.ContainsKey(item.Title)) {
                //if (thisList != null && ListViewOffset.ContainsKey(NowItem)) {
                //    var scroll = GetScrollViewer(thisList);
                //    Debug.WriteLine("---->" + ListViewOffset[NowItem]);
                //    var num = ListViewOffset[NowItem];
                //    ListResources.Source = cacheDic[item.Title];
                //    scroll.ChangeView(0, num, 1);
                //} else {
                    ListResources.Source = cacheDic[item.Title];
                //}
            }
            else {
                HomeLlistResources = new DQDLoadContext<ContentListModel>(DataHandler.SetHomeListResources, item.Number, HomeHost);
                cacheDic.Add(item.Title, HomeLlistResources);
                ListResources.Source = HomeLlistResources;
            }
        }

        #region Save the position of listview scroll (Dropped)
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
            }
            catch { Debug.WriteLine("Save scroll positions error."); }
        }

        private void LocalPageListView_Loaded(object sender, RoutedEventArgs e) {
            //thisList = sender as ListView;
            //var vierew = GetScrollViewer(thisList);
            //vierew.ViewChanged += ScrollViewer_ViewChanged;
        }

        #endregion
    }
}
