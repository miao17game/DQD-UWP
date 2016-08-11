using DQD.Core.Models;
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
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace DQD.Net.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ContentPage : Page {
        public ContentPage() {
            this.InitializeComponent();
        }

        /// <summary>
        /// trigger the event when this frame is first navigated
        /// </summary>
        /// <param name="e">navigate args</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            var value = e.Parameter as Uri;
            if (value != null) {
                Debug.WriteLine(value.ToString());
                webView.Source = value;
            }
            var stringMessa = await DataProcess.GetPageInnerContent(value.ToString());
            PageContent.Text = stringMessa;
        }

        private void webView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args) {

        }

        private void webView_ContentLoading(WebView sender, WebViewContentLoadingEventArgs args) {

        }

        private void webView_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args) {

        }

        private void webView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args) {

        }

        private void webView_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e) {

        }
    }
}
