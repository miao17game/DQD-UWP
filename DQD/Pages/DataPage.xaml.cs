using DQD.Core.Models.TeamModels;
using DQD.Core.Tools;
using System;
using System.Collections.Generic;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace DQD.Net.Pages {
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DataPage:Page {
        public DataPage() {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            InitBounldResources();
        }

        private async void InitBounldResources() {
            ListResources.Source = DataProcess.GetLeagueContent((await WebProcess.GetHtmlResources(TargetHost)).ToString());
        }

        #region State
        const string TargetHost = "http://www.dongqiudi.com/data";
        #endregion

        private void ListView_ItemClick(object sender, ItemClickEventArgs e) {
            var item = e.ClickedItem as LeagueModel;
            MainPage.Current.LoadingProgress.IsActive = true;
            MainPage.Current.ItemClick?.Invoke(this, typeof(DataContentPage), MainPage.Current.ContFrame, item.Href, 0,item.LeagueName);
            MainPage.Current.SideGrid.Visibility = Visibility.Visible;
        }
    }
}
