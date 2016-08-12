//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace DQD.Core.DataVirtualization {
    // This class can used as a jumpstart for implementing ISupportIncrementalLoading. 
    // Implementing the ISupportIncrementalLoading interfaces allows you to create a list that loads
    //  more data automatically when the user scrolls to the end of of a GridView or ListView.
    public abstract class IncrementalLoadingBase:IList, ISupportIncrementalLoading, INotifyCollectionChanged {
        #region IList

        public int Add(object value) {
            throw new NotImplementedException();
        }

        public void Clear() {
            throw new NotImplementedException();
        }

        public bool Contains(object value) {
            return storageList.Contains(value);
        }

        public int IndexOf(object value) {
            return storageList.IndexOf(value);
        }

        public void Insert(int index,object value) {
            throw new NotImplementedException();
        }

        public bool IsFixedSize {
            get { return false; }
        }

        public bool IsReadOnly {
            get { return true; }
        }

        public void Remove(object value) {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index) {
            throw new NotImplementedException();
        }

        public object this[int index] {
            get {
                return storageList[index];
            }
            set {
                throw new NotImplementedException();
            }
        }

        public void CopyTo(Array array,int index) {
            ((IList)storageList).CopyTo(array,index);
        }

        public int Count {
            get { return storageList.Count; }
        }

        public bool IsSynchronized {
            get { return false; }
        }

        public object SyncRoot {
            get { throw new NotImplementedException(); }
        }

        public IEnumerator GetEnumerator() {
            return storageList.GetEnumerator();
        }

        #endregion

        #region ISupportIncrementalLoading

        public bool HasMoreItems { get { return HasMoreItemsOverride(); } }

        public Windows.Foundation.IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count) {
            if(isBusyOrNot)
                /// don not load too much!
                //throw new InvalidOperationException("Only one operation in flight at a time");
            isBusyOrNot =true;
            return AsyncInfo.Run((c) => LoadMoreItemsAsync(c,count));
        }

        #endregion 

        #region INotifyCollectionChanged

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion 

        #region Private methods

        async Task<LoadMoreItemsResult> LoadMoreItemsAsync(CancellationToken c,uint count) {
            try {
                var items = await LoadMoreItemsOverrideAsync(c,count);
                var baseIndex = storageList.Count;
                storageList.AddRange(items);
                // Now notify of the new items
                NotifyOfInsertedItems(baseIndex,items.Count);
                return new LoadMoreItemsResult { Count=(uint)items.Count };
            } finally { isBusyOrNot=false; }
        }

        void NotifyOfInsertedItems(int baseIndex,int count) {
            if(CollectionChanged==null) { return; }
            for(int i = 0;i<count;i++) {
                var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,storageList[i+baseIndex],i+baseIndex);
                CollectionChanged(this,args);
            }
        }

        #endregion

        #region Overridable methods

        protected abstract Task<IList<object>> LoadMoreItemsOverrideAsync(CancellationToken c,uint count);
        protected abstract bool HasMoreItemsOverride();

        #endregion 

        #region State

        List<object> storageList = new List<object>();
        protected bool isBusyOrNot = false;
        protected string Flag = "Default_Flag";

        #endregion 
    }
}
