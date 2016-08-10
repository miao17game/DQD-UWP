using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DQD.Core.DataVirtualization;
using DQD.Core.Helpers;
using Windows.UI.Xaml.Data;

namespace DQD.Core.Models {
    public class HomeListDataSource:DataSource<HomeListModel>{
        int PageIndex = 0;

        public HomeListDataSource() {
            ///  用户界面线程调度程序
            CurrentDispatcher=Windows.UI.Xaml.Window.Current.Dispatcher;

            ///  ItemCacheManager 负责处理最繁重的工作。我们将它传递一个回调，
            ///  它将使用实际提取数据和请求的最大大小
            this.ItemsCache=new ItemCacheManager<HomeListModel>(fetchDataCallback,UP_MAX_NUMBER);
            this.ItemsCache.CacheChanged+=ItemCache_CacheChanged;
        }

        /// <summary>
        ///  工厂方法来创建数据源 ,
        ///  需要异步工作这就是为什么它需要工厂，而不是构造函数的一部分
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static HomeListDataSource GetHomeDataSoure(ObservableCollection<HomeListModel> list) {
            HomeListDataSource dataSource = new HomeListDataSource();
            dataSource.SetFolder(list);
            return dataSource;
        }

        /// <summary>
        /// 文件夹的设置的功能
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public override async void SetFolder(ObservableCollection<HomeListModel> list) {
            /// 解开旧的内容已经更改事件，如果适用
            if(CurrentListSources!=null) {
                CurrentListSources.CollectionChanged-=Current_CollectionChanged;
            }
            CurrentListSources=list;
            CurrentListSources.CollectionChanged+=Current_CollectionChanged;
            UpdateCount();
            PageIndex++;
        }

        /// <summary>
        /// 从它需要要检索的项的 itemcache 回调 ,使用此回调模型摘要从缓存实现此特定数据源的详细信息
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public override async Task<HomeListModel[]> fetchDataCallback(ItemIndexRange batch,CancellationToken token) {
            /// 从文件系统读取文件对象
            Debug.WriteLine(CurrentListSources.Count+"+"+batch.LastIndex);
            if(CurrentListSources.Count-1==batch.LastIndex) {
                var targetHost = "http://www.dongqiudi.com?tab=11&amp;page={0}";
                targetHost=string.Format(targetHost,PageIndex+1);
                var sourcesNew = await DataHandler.SetHomeListResources(targetHost);
                foreach(var item in sourcesNew) 
                    CurrentListSources.Add(item);
                PageIndex++;
                UpdateCount();
            }
            var newList = new HomeListModel[batch.Length];
            Array.Copy(CurrentListSources.ToArray(),batch.FirstIndex,newList,0,(int)batch.Length);
            return newList;
        }
    }
}
