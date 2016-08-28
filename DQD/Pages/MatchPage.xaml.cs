using DQD.Core.Controls;
using DQD.Core.Helpers;
using DQD.Core.Models;
using DQD.Core.Models.MatchModels;
using DQD.Core.Tools;
using Newtonsoft.Json.Linq;
using Noodles_Blog_ClassLibrary.Tools;
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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace DQD.Net.Pages {
    /// <summary>
    /// MatchPage Code Page
    /// </summary>
    public sealed partial class MatchPage:Page {
       
        #region Constructor

        public MatchPage() {
            Current = this;
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            cacheDic = new Dictionary<string, List<AlphaKeyGroup<MatchListModel>>>();
            resources = new List<AlphaKeyGroup<MatchListModel>>();
            ButtonShadow = ButtonStack;
            ButtonNoShadow = ButtonStackNoShadow;
            InitHeaderGroup();
        }

        #endregion

        #region Methods

        private async void InitHeaderGroup() {
            ObservableCollection<HeaderModel> headerList = new ObservableCollection<HeaderModel>();
            headerList = await DataHandler.SetMatchGroupResources();
            HeaderResources.Source = headerList;
            InitFloatButtonView();
        }

        private string GetFormatDateNow() {
            string stringModel = "{0}-{1}-{2}";
            var nowDate = DateTime.Now;
            return string.Format(
                stringModel,
                nowDate.Year.ToString(),
                nowDate.Month >= 10 ? nowDate.Month.ToString() : "0" + nowDate.Month.ToString(),
                nowDate.Day > 10 ? nowDate.Day.ToString() : "0" + nowDate.Day.ToString());
        }

        public async Task<List<AlphaKeyGroup<MatchListModel>>> FetchHtml(string rel,string scrolltimes,string timezone) {
            return GetAlphaKeyGroup.GetAlphaGroupSampleItems(
                DataProcess.GetMatchItemsContent(
                    JObject.Parse(
                        (await WebProcess.GetHtmlResources(
                            string.Format(TargetUrl, rel, GetFormatDateNow(), scrolltimes, timezone)))
                            .ToString())["html"]
                            .ToString()));
        }

        private void InitFloatButtonView() {
            if (MainPage.Current.IsFloatButtonEnable) {
                ButtonStack.Visibility = VisiEnumHelper.GetVisibility(MainPage.Current.IsButtonShadowVisible);
                ButtonStackNoShadow.Visibility = VisiEnumHelper.GetVisibility(!MainPage.Current.IsButtonShadowVisible);
            } else {
                ButtonStack.Visibility = VisiEnumHelper.GetVisibility(false);
                ButtonStackNoShadow.Visibility = VisiEnumHelper.GetVisibility(false);
            }
        }

        private void MatchListView_ItemClick(object sender, ItemClickEventArgs e) {
            var itemUri = (e.ClickedItem as MatchListModel).ArticleLink;
            var itemNum = (e.ClickedItem as MatchListModel).ArticleID;
            if (itemNum != null) {
                MainPage.Current.ItemClick?.Invoke(
                    this,
                    typeof(ContentPage),
                    MainPage.Current.ContFrame,
                    itemUri,
                    (int)itemNum,
                    null);
                MainPage.Current.SideGrid.Visibility = Visibility.Visible; }
            if (itemUri != null && itemNum == null) {
                MainPage.Current.ItemClick?.Invoke(
                    this,
                    typeof(WebLivePage),
                    MainPage.Current.ContFrame,
                    itemUri,
                    0,
                    null);
                MainPage.Current.SideGrid.Visibility = Visibility.Visible; }
        }

        #endregion

            #region Events

        private async void MyPivot_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ItemsGrouped.Source = null;
            var item = (sender as Pivot).SelectedItem as HeaderModel;
            nowItem = item.Title;
            itemNumber = item.Number.ToString();
            if (!cacheDic.ContainsKey(item.Title)) {
                resources = await FetchHtml(itemNumber, "0", "-8");
                if (resources.Count == 0) { new ToastSmooth("近期没有比赛").Show(); }
                cacheDic.Add(item.Title, resources);
            }
            ItemsGrouped.Source = cacheDic[nowItem];
        }

        private async void RefreshBtn_Click(object sender, RoutedEventArgs e) {
            ListResources.Source = cacheDic[nowItem] = await FetchHtml(itemNumber, "0", "-8");
        }

        #endregion

        #region Properties and State
        public static MatchPage Current { get; private set; }
        public StackPanel ButtonShadow { get; private set; }
        public StackPanel ButtonNoShadow { get; private set; }
        private string nowItem;
        private string itemNumber;
        private string TargetUrl = "http://dongqiudi.com/match/fetch?tab={0}&date={1}&scroll_times={2}&tz={3}";
        private List<AlphaKeyGroup<MatchListModel>> resources;
        private Dictionary<string, List<AlphaKeyGroup<MatchListModel>>> cacheDic;
        #endregion

    }
}
