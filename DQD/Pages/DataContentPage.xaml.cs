using DQD.Core.Controls;
using DQD.Core.Models;
using DQD.Core.Models.TeamModels;
using DQD.Core.Tools;
using System;
using System.Collections.Generic;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace DQD.Net.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DataContentPage : Page {
        public DataContentPage() {
            Current = this;
            transToSideGrid = this.RenderTransform as TranslateTransform;
            if (transToSideGrid == null) this.RenderTransform = transToSideGrid = new TranslateTransform();
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            cacheDicList = new Dictionary<Uri, Dictionary<string, List<TeamLeagueModel>>>();
        }

        #region Events

        /// <summary>
        /// trigger the event when this frame is first navigated
        /// </summary>
        /// <param name="e">navigate args</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            this.Opacity = 0;
            var parameter = e.Parameter as ParameterNavigate;
            HostSource = parameter.Uri;
            ContentTitle.Text = parameter.Summary;
            if (HostSource == null)
                return;
            targetHost = HostSource.ToString() + "&type={0}";
            Debug.WriteLine(targetHost);
            targetDicList =
                cacheDicList[HostSource] =
                cacheDicList.ContainsKey(HostSource) ?
                cacheDicList[HostSource] :
                new Dictionary<string, List<TeamLeagueModel>>();
            await InsertListResources("IntergralPItem");
            if (StatusBarInit.HaveAddMobileExtensions()) { BackBtn.Visibility = Visibility.Collapsed; ContentTitle.Margin = new Thickness(15, 0, 0, 0); }
            this.Opacity = 1;
            InitStoryBoard();
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e) {
            MainPage.Current.SideGrid.Visibility = Visibility.Collapsed;
        }

        private async void MyPivot_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var item = ((sender as PersonalPivot).SelectedItem as PivotItem).Name;
            await InsertListResources(item);
        }

        private async System.Threading.Tasks.Task InsertListResources(string item) {
            InsideResources.GetTListSource(item).Source =
                            targetDicList[item] =
                            targetDicList.ContainsKey(item) ?
                            targetDicList[item] :
                            DataProcess.GetLeagueTeamsContent(
                                (await WebProcess.GetHtmlResources(
                                    string.Format(
                                        targetHost, InsideResources.GetTTargetRank(item))))
                                        .ToString());
        }

        private void BaseGrid_SizeChanged(object sender, SizeChangedEventArgs e) {
            
        }

        #endregion

        #region Static Inside Class
        /// <summary>
        /// Page inside resources of navigating and choosing frames.
        /// </summary>
        static class InsideResources {

            /// <summary>
            /// The dictionary map of frames collection. 
            /// </summary>
            static private Dictionary<string, CollectionViewSource> ListViewResourcesMaps = new Dictionary<string, CollectionViewSource> {
            {"IntergralPItem",Current.IntergralListResources},
            {"ShootPItem",Current.ShootListResources},
            {"HelpPItem",Current.HelpListResources},
            {"SchedulePItem",Current.ScheduleListResources},
        };

            /// <summary>
            /// Get target frame by item name.
            /// </summary>
            /// <param name="str">item name</param>
            /// <returns></returns>
            public static CollectionViewSource GetTListSource(string str) { return ListViewResourcesMaps.ContainsKey(str) ? ListViewResourcesMaps[str] : null; }

            static private Dictionary<string, string> TargetUrlMaps = new Dictionary<string, string> {
            {"IntergralPItem","team_rank"},
            {"ShootPItem","goal_rank"},
            {"HelpPItem","assist_rank"},
            {"SchedulePItem","schedule"},
        };

            public static string GetTTargetRank(string str) { return TargetUrlMaps.ContainsKey(str) ? TargetUrlMaps[str] : "team_rank"; }

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
            };
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

        public static DataContentPage Current;
        private Dictionary<Uri, Dictionary<string,List<TeamLeagueModel>>> cacheDicList;
        private Dictionary<string, List<TeamLeagueModel>> targetDicList;
        private string targetHost =default(string);
        private Uri HostSource;
        private int HostNumber;

        #endregion
    }
}
