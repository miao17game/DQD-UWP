using DQD.Core.Controls;
using DQD.Core.Helpers;
using DQD.Core.Models;
using DQD.Core.Models.TeamModels;
using DQD.Core.Tools;
using DQD.Net.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
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
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DataContentPage : BaseContentPage {

        #region Constructor

        public DataContentPage() {
            Current = this;
            this.InitializeComponent();
            ButtonShadow = ButtonStack;
            ButtonNoShadow = ButtonStackNoShadow;
            this.NavigationCacheMode = NavigationCacheMode.Required;
            loadingAnimation = MainPage.Current.LoadingProgress;
            cacheDicList = new Dictionary<Uri, Dictionary<string, IList<object>>>();
        }

        #endregion

        #region Events

        /// <summary>
        /// trigger the event when this frame is first navigated
        /// </summary>
        /// <param name="e">navigate args</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            this.Opacity = 0;
            navigatedToOrNot = true;
            InitFloatButtonView();
            var parameter = e.Parameter as ParameterNavigate;
            hostSource = parameter.Uri;
            ContentTitle.Text = parameter.Summary;
            if (hostSource == null)
                return;
            targetHost = hostSource.ToString() + "&type={0}";
            targetDicList =
                cacheDicList[hostSource] =
                cacheDicList.ContainsKey(hostSource) ?
                cacheDicList[hostSource] :
                new Dictionary<string, IList<object>>();
            if (RootPivot.SelectedIndex == 0)
                await InsertListResources("IntergralPItem");
            else RootPivot.SelectedIndex = 0;
            if (StatusBarInit.HaveAddMobileExtensions()) { BackBtn.Visibility = Visibility.Collapsed; ContentTitle.Margin = new Thickness(15, 0, 0, 0); }
            loadingAnimation.IsActive = false;
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e) {
            storyToSideGridOut.Begin();
        }

        private async void MyPivot_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var item = ((sender as PersonalPivot).SelectedItem as PivotItem).Name;
            InsideResources.GetTListSource(item).Source = new List<object>();
            loadingAnimation.IsActive = true;
            await InsertListResources(item);
        }

        private async void BoudListBtnClick(object sender, RoutedEventArgs e) {
            scheduleDicList = scheduleDicList == null ? new Dictionary<string, IList<object>>() : scheduleDicList;
            var item = ((sender as Button).CommandParameter as Uri).ToString();
            loadingAnimation.IsActive = true;
            await InsertScheduleResources(item);
        }

        private async void RefreshBtn_Click(object sender, RoutedEventArgs e) {
            InsideResources.FlushAllResources();
            loadingAnimation.IsActive = true;
            cacheDicList.Clear();
            targetDicList =
               cacheDicList[hostSource] =
               cacheDicList.ContainsKey(hostSource) ?
               cacheDicList[hostSource] :
               new Dictionary<string, IList<object>>();
            if (RootPivot.SelectedIndex == 0)
                await InsertListResources("IntergralPItem");
            else RootPivot.SelectedIndex = 0;
        }

        #endregion

        #region Methods

        private async System.Threading.Tasks.Task InsertListResources(string item) {
            InsideResources.GetTListSource(item).Source =
                            targetDicList[item] =
                            targetDicList.ContainsKey(item) ?
                            targetDicList[item] :
                            InsideResources.GetEventHandler(item).Invoke(
                                (await WebProcess.GetHtmlResources(
                                    string.Format(
                                        targetHost, InsideResources.GetTTargetRank(item))))
                                        .ToString());
            loadingAnimation.IsActive = false;
            if (navigatedToOrNot) {
                this.Opacity = 1;
                InitStoryBoard();
                navigatedToOrNot = false;
            }
        }

        private async System.Threading.Tasks.Task InsertScheduleResources(string item) {
            ScheduleListResources.Source =
                            scheduleDicList[item] =
                            scheduleDicList.ContainsKey(item) ?
                            scheduleDicList[item] :
                            InsideResources.GetEventHandler("SchedulePItem").Invoke(
                                (await WebProcess.GetHtmlResources(item))
                                .ToString());
            loadingAnimation.IsActive = false;
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

        #endregion

        #region Static Inside Class
        /// <summary>
        /// Page inside resources of navigating and choosing frames.
        /// </summary>
        static class InsideResources {

            public static void FlushAllResources() { foreach (var item in ListViewResourcesMaps) { item.Value.Source = new List<object>(); } }
            public static CollectionViewSource GetTListSource(string str) { return ListViewResourcesMaps.ContainsKey(str) ? ListViewResourcesMaps[str] : null; }
            static private Dictionary<string, CollectionViewSource> ListViewResourcesMaps = new Dictionary<string, CollectionViewSource> {
            {"IntergralPItem",Current.IntergralListResources},
            {"ShootPItem",Current.ShootListResources},
            {"HelpPItem",Current.HelpListResources},
            {"SchedulePItem",Current.ScheduleListResources},
        };

            public static string GetTTargetRank(string str) { return TargetUrlMaps.ContainsKey(str) ? TargetUrlMaps[str] : "team_rank"; }
            static private Dictionary<string, string> TargetUrlMaps = new Dictionary<string, string> {
            {"IntergralPItem","team_rank"},
            {"ShootPItem","goal_rank"},
            {"HelpPItem","assist_rank"},
            {"SchedulePItem","schedule"},
        };

            public static NavigateEventHandler GetEventHandler(string str) { return EventHandlerMaps.ContainsKey(str) ? EventHandlerMaps[str] : null; }
            static private Dictionary<string, NavigateEventHandler> EventHandlerMaps = new Dictionary<string, NavigateEventHandler> {
                { "IntergralPItem", new NavigateEventHandler(path=> { return DataProcess.GetLeagueTeamsContent(path).ToArray(); })},
                { "ShootPItem", new NavigateEventHandler(path=> { return DataProcess.GetSoccerMemberContent(path).ToArray(); })},
                { "HelpPItem", new NavigateEventHandler(path=> { return DataProcess.GetSoccerMemberContent(path).ToArray(); })},
                { "SchedulePItem", new NavigateEventHandler(path=> { return DataProcess.GetScheduleTeamsContent(path).ToArray(); })},
            };

        }
        #endregion

        #region Properties and State

        public static DataContentPage Current { get; private set; }
        public StackPanel ButtonShadow { get; private set; }
        public StackPanel ButtonNoShadow { get; private set; }
        private Dictionary<Uri, Dictionary<string,IList<object>>> cacheDicList;
        private Dictionary<string, IList<object>> targetDicList;
        private Dictionary<string, IList<object>> scheduleDicList;
        private delegate IList<object> NavigateEventHandler(string path);
        private string targetHost =default(string);
        private Uri hostSource;
        private int hostNumber;
        private ProgressRing loadingAnimation;
        private bool navigatedToOrNot = false;

        #endregion

    }
}
