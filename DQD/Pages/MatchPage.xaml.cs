using DQD.Core.Controls;
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
using Windows.UI.Xaml.Navigation;

namespace DQD.Net.Pages {
    /// <summary>
    /// MatchPage Code Page
    /// </summary>
    public sealed partial class MatchPage:Page {
        #region Properties and State
        private string NowItem;
        private string TargetUrl = "http://dongqiudi.com/match/fetch?tab={0}&date={1}&scroll_times={2}&tz={3}";
        private List<AlphaKeyGroup<MatchListModel>> Resources;
        private Dictionary<string, List<AlphaKeyGroup<MatchListModel>>> cacheDic;
        #endregion

        #region Constructor
        public MatchPage() {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            cacheDic = new Dictionary<string, List<AlphaKeyGroup<MatchListModel>>>();
            Resources = new List<AlphaKeyGroup<MatchListModel>>();
            InitHeaderGroup();
        }
        #endregion

        #region Methods
        private async void InitHeaderGroup() {
            ObservableCollection<HeaderModel> headerList = new ObservableCollection<HeaderModel>();
            headerList = await DataHandler.SetMatchGroupResources();
            HeaderResources.Source = headerList;
        }

        public async Task<List<AlphaKeyGroup<MatchListModel>>> FetchHtml(string rel,string date,string scrolltimes,string timezone) {
            var newUrl = string.Format(TargetUrl, rel, date, scrolltimes, timezone);
            Debug.WriteLine(newUrl);
            return GetAlphaKeyGroup.GetAlphaGroupSampleItems(
                DataProcess.GetMatchItemsContent(
                    JObject.Parse(
                        (await WebProcess.GetHtmlResources(newUrl)).ToString())["html"].ToString()));
        }

        #endregion

        #region Events
        private async void MyPivot_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ItemsGrouped.Source = null;
            var item = (sender as Pivot).SelectedItem as HeaderModel;
            NowItem = item.Title;
            var rel = item.Number.ToString();
            if (!cacheDic.ContainsKey(item.Title)) {
                Resources = await FetchHtml(rel, "2016-08-16","0","-8");
                if(Resources.Count == 0) { new ToastSmooth("近期没有比赛").Show(); }
                cacheDic.Add(item.Title, Resources);
            }
            ItemsGrouped.Source = cacheDic[NowItem];
        }
        #endregion
    }
}
