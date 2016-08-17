using DQD.Core.Models;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace DQD.Net.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DataContentPage : Page {
        public DataContentPage() {
            this.InitializeComponent();
        }

        #region Events

        /// <summary>
        /// trigger the event when this frame is first navigated
        /// </summary>
        /// <param name="e">navigate args</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            var parameter = e.Parameter as ParameterNavigate;
            HostSource = parameter.Uri;
            HostNumber = parameter.Number;
            if (HostSource == null)
                return;
            
            if (StatusBarInit.IsTargetMobile()) { BackBtn.Visibility = Visibility.Collapsed; ContentTitle.Margin = new Thickness(15, 0, 0, 0); }
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e) {

        }

        private void MyPivot_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }

        private void BaseGrid_SizeChanged(object sender, SizeChangedEventArgs e) {
            RootPivot.HeaderWidth = (sender as Grid).ActualWidth / 4;
        }

        #endregion

        #region Properties and State

        private string targetHost = "http://dongqiudi.com/article/{0}?page={1}#comment_anchor";
        private Uri HostSource;
        private int HostNumber;

        #endregion
    }
}
