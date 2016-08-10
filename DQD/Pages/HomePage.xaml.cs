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

namespace DQD.Net.Pages {
    /// <summary>
    /// HomePage Code Page
    /// </summary>
    public sealed partial class HomePage:Page {
        public static HomePage Current;
        private IncrementalLoading<HomeListModel> listResources;
        private const string HomeHost = "http://www.dongqiudi.com/";
        private const string HomeHostInsert = "http://www.dongqiudi.com";

        public HomePage() {
            Current=this;
            this.NavigationCacheMode=NavigationCacheMode.Required;
            this.InitializeComponent();
            InitListView();
        }

        /// <summary>
        /// Init the HomeListView databundle resources.
        /// </summary>
        private void InitListView() {
            if(listResources!=null)
                listResources.CollectionChanged-=_employees_CollectionChanged;
            listResources=new IncrementalLoading<HomeListModel>();
            listResources.CollectionChanged+=_employees_CollectionChanged;
            ListResources.Source=listResources;
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

        }
    }
}
