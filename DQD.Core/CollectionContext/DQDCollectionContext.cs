using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DQD.Core.CollectionContext {
    class DQDCollectionContext<T> : IList, INotifyCollectionChanged where T : class {
        public DQDCollectionContext(List<T> list){
            ReceivedList = list;
            var items = LoadItemsAsync();
            var baseIndex = sourceList.Count;
            sourceList.AddRange(items);
            NotifyWhenItemsInserted(baseIndex, items.Count);
        }

        #region IList

        public object this[int index] { get { return sourceList[index]; } set { sourceList[index] = value; } }

        public int Count { get { return sourceList.Count; } }

        public bool IsFixedSize { get { return false; } }

        public bool IsReadOnly { get { return false; } }

        public bool IsSynchronized { get { return false; } }

        public object SyncRoot { get { throw new NotImplementedException(); } }

        public int Add(object value) { sourceList.Add(value); return sourceList.Count; }

        public void Clear() { sourceList.Clear(); }

        public bool Contains(object value) { return sourceList.Contains(value); }

        public void CopyTo(Array array, int index) { ((IList)sourceList).CopyTo(array, index); }

        public IEnumerator GetEnumerator() { return sourceList.GetEnumerator(); }

        public int IndexOf(object value) { return sourceList.IndexOf(value); }

        public void Insert(int index, object value) { throw new NotImplementedException(); }

        public void Remove(object value) { throw new NotImplementedException(); }

        public void RemoveAt(int index) { throw new NotImplementedException(); }

        #endregion

        #region Methods
        protected IList<object> LoadItemsAsync() {
            return ReceivedList.ToArray();
        }

        void NotifyWhenItemsInserted(int baseIndex, int count) {
            if (CollectionChanged == null) { return; }
            for (int i = 0; i < count; i++) {
                var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, sourceList[i + baseIndex], i + baseIndex);
                CollectionChanged(this, args);
            }
        }

        public bool HasMoreItems { get { return HasMoreItemsOrNot(); } }

        private bool HasMoreItemsOrNot() {
            return true;
        }

        #endregion

        #region State
        List<T> ReceivedList = new List<T>(); 
        List<object> sourceList = new List<object>();
        bool IsOnAsyncWorkOrNot = false;
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        #endregion
    }
}
