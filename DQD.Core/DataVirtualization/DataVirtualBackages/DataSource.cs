﻿//********************************************************************************************
//*
//* 注 ︰ 此示例使用一个自定义编译器常数来启用跟踪。如果您添加
//* TRACE_DATASOURCE 的生成选项卡的条件编译符号
//* 项目属性窗口，然后应用程序会吐到跟踪数据
//* 输出窗口在调试时。
//
// 版权所有 （c） 微软。保留所有权利。
// 此代码被许可下 MIT 许可证 （麻省理工学院）。
// 提供此代码 没有任何形式的保证， 无论明示或暗示
// 任何包括用于某一特定商业用途的适用性的
// 暗示或保证都被视为构成侵权。
//
//********************************************************************************************
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Core;
using Windows.UI.Xaml.Data;

namespace DQD.Core.DataVirtualization {
    /// <summary>
    /// 在支持数据虚拟化文件系统的自定义数据源
    /// </summary>
    public class DataSource<T> : INotifyCollectionChanged, System . Collections . IList, IItemsRangeInfo, IDataSource<T> {
        /// <summary>
        /// 文件夹是 browsingQuery 对象，它将告诉我们是否更改文件夹内容的文件夹
        /// </summary>
        internal ObservableCollection<T> CurrentListSources;
        /// <summary>
        /// 封送回 UI 线程调用的调度
        /// </summary>
        internal CoreDispatcher CurrentDispatcher;
        /// <summary>
        /// 当前正在使用的文件数据缓存
        /// </summary>
        internal ItemCacheManager<T> ItemsCache;
        /// <summary>
        /// 可用的文件总数
        /// </summary>
        private int COUNT = 1;
        /// <summary>
        /// 虚拟化的视口范围
        /// </summary>
        public int UP_MAX_NUMBER = 20;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public DataSource ( ) {
            ///  用户界面线程调度程序
            CurrentDispatcher = Windows . UI . Xaml . Window . Current . Dispatcher;

            ///  ItemCacheManager 负责处理最繁重的工作。我们将它传递一个回调，
            ///  它将使用实际提取数据和请求的最大大小
            this . ItemsCache = new ItemCacheManager<T> ( fetchDataCallback , UP_MAX_NUMBER );
            this . ItemsCache . CacheChanged += ItemCache_CacheChanged;
        }

        /// <summary>
        ///  工厂方法来创建数据源 ,
        ///  需要异步工作这就是为什么它需要工厂，而不是构造函数的一部分
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static DataSource<T> GetDataSoure(ObservableCollection<T> list) {
            DataSource<T> dataSource = new DataSource<T>();
            dataSource.SetFolder(list);
            return dataSource;
        }

        /// <summary>
        /// 文件夹的设置的功能
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public virtual void SetFolder(ObservableCollection<T> list) {
            /// 解开旧的内容已经更改事件，如果适用
            if(CurrentListSources!=null) {
                CurrentListSources.CollectionChanged-=Current_CollectionChanged;
            }
            CurrentListSources=list;
            CurrentListSources.CollectionChanged+=Current_CollectionChanged;
            UpdateCount();
        }

        /// <summary>
        /// 当文件系统通知我们更改到文件列表的处理程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        internal void Current_CollectionChanged(object sender,NotifyCollectionChangedEventArgs e) {
            /// 此回调可以发生在不同的线程上，因此我们需要将封送回 UI 线程
            if(!CurrentDispatcher.HasThreadAccess) {
                var t = CurrentDispatcher.RunAsync(CoreDispatcherPriority.Normal,ResetCollection);
            } else {
                ResetCollection();
            }
        }

        /// <summary>
        /// 处理文件从操作系统列表更改的通知
        /// </summary>
        private void ResetCollection ( ) {
            ///  解开旧的更改通知
            if ( ItemsCache != null ) {
                this . ItemsCache . CacheChanged -= ItemCache_CacheChanged;
            }

            /// 创建新实例的缓存管理器
            this . ItemsCache = new ItemCacheManager<T> ( fetchDataCallback , UP_MAX_NUMBER );
            this . ItemsCache . CacheChanged += ItemCache_CacheChanged;
            CollectionChanged?.Invoke ( this , new NotifyCollectionChangedEventArgs ( NotifyCollectionChangedAction . Reset ) );
        }

        internal void UpdateCount() {
            COUNT=CurrentListSources.Count;
            CollectionChanged?.Invoke(this,new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// 所需的 IItemsRangeInfo 接口
        /// </summary>
        public void Dispose ( ) {
            ItemsCache = null;
        }

        /// <summary>
        /// IItemsRangeInfo 接口的主要方法,当列表控件视图更改时调用
        /// </summary>
        /// <param name="visibleRange">是实际可见的项目的范围</param>
        /// <param name="trackedItems">另外一组的范围列表中使用，例如缓冲区域和集中的元素</param>
        public void RangesChanged ( ItemIndexRange visibleRange , IReadOnlyList<ItemIndexRange> trackedItems ) {

            #if TRACE_DATASOURCE
            string s = string.Format("* RangesChanged fired: Visible {0}->{1}", visibleRange.FirstIndex, visibleRange.LastIndex);
            foreach (ItemIndexRange r in trackedItems) { s += string.Format(" {0}->{1}", r.FirstIndex, r.LastIndex); }
            Debug.WriteLine(s);
            #endif

            ItemIndexRange[] newRange = new ItemIndexRange[2];
            Array.Copy(trackedItems.ToArray(),0,newRange,0,2);

            /// 我们知道在更广的范围，所以并不需要把它的 UpdateRanges 调用中包含可见范围
            /// 更新缓存中的项目基于一套新的范围。它将回调的额外数据，如果需要
            ItemsCache.UpdateRanges(newRange);
        }

        /// <summary>
        /// 从它需要要检索的项的 itemcache 回调 ,使用此回调模型摘要从缓存实现此特定数据源的详细信息
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual async Task<T[]> fetchDataCallback(ItemIndexRange batch,CancellationToken token) {
            /// 从文件系统读取文件对象
            var results = CurrentListSources;
            List<T> files = new List<T>();
            if(results!=null) {
                for(int i = 0;i<results.Count;i++) {
                    /// 检查是否请求已被取消，如果这么中止获取额外数据
                    token.ThrowIfCancellationRequested();
                    /// 我们 FileItem 对象创建的文件数据与缩略图
                    //T newItem = await DataProcess.fromHomeListResources(results[i],token);
                    //files.Add(newItem);
                }
            }
            return files.ToArray();
        }

        /// <summary>
        /// 在缓存中插入项时激发的事件,用于释放我们集合更改事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        internal void ItemCache_CacheChanged ( object sender , CacheChangedEventArgs<T> args ) {
            CollectionChanged?.Invoke ( this , new NotifyCollectionChangedEventArgs ( NotifyCollectionChangedAction . Replace , args . oldItem , args . newItem , args . itemIndex ) );
        }

        #region IList 接口实现

        public bool Contains ( object value ) {
            return IndexOf ( value ) != -1;
        }

        public int IndexOf ( object value ) {
            return ( value != null ) ? ItemsCache . IndexOf ( ( T ) value ) : -1;
        }

        public object this [ int index ] {
            get {
                /// 缓存中将返回 null，如果它不具备的项目。
                /// 一旦该项目提取它会释放已更改的事件，以便我们可以通知列表控件
                return ItemsCache [ index ];
            }
            set {
                throw new NotImplementedException ( );
            }
        }
        public int Count {
            get { return COUNT; }
        }

        #endregion

        #region IList 接口未实现的功能

        public int Add ( object value ) {
            throw new NotImplementedException ( );
        }

        public void Clear ( ) {
            throw new NotImplementedException ( );
        }

        public void Insert ( int index , object value ) {
            throw new NotImplementedException ( );
        }

        public bool IsFixedSize {
            get { return false; }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public void Remove ( object value ) {
            throw new NotImplementedException ( );
        }

        public void RemoveAt ( int index ) {
            throw new NotImplementedException ( );
        }
        public void CopyTo ( Array array , int index ) {
            throw new NotImplementedException ( );
        }

        public bool IsSynchronized {
            get { throw new NotImplementedException ( ); }
        }

        public object SyncRoot {
            get { throw new NotImplementedException ( ); }
        }

        public System . Collections . IEnumerator GetEnumerator ( ) {
            throw new NotImplementedException ( );
        }

        #endregion
    }


}
