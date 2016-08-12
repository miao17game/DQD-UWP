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
            var PConModel = await DataProcess.GetPageInnerContent(value.ToString());
            ContentTitle.Text = PConModel.Title;
            ContentAuthor.Text = "来源：" + PConModel.Author;
            ContentDate.Text = PConModel.Date;
            int num = PConModel.ContentImage.Count + PConModel.ContentString.Count;
            for (int index = 1; index <= num; index++) {
                var item = PConModel.ContentString.Find(i => i.Index == index);
                if (item != null)
                    ContentStack.Children.Add(new TextBlock {
                        Text = item.Content,
                        TextWrapping = TextWrapping.WrapWholeWords,
                        Margin =new Thickness(2,3,2,3),
                    });
                else {
                    var item2 = PConModel.ContentImage.Find(i => i.Index == index);
                    if (item2 != null)
                        ContentStack.Children.Add(new Image { Source = item2.Image, Margin = new Thickness(20,5,20,5) });
                }
            }
        }
    }
}
