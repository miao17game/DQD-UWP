using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DQD.Core.DataVirtualization {
    public class DQDDataContext<T> : DQDDataContextBase where T : class {
        public DQDDataContext(FetchDataCallbackHandler back, int num, uint rollNum ,string targetHost ) {
            FetchCallback = back;
            number = num;
            rollNumber = rollNum;
            this.targetHost = targetHost;
            InitType = InitSelector.Default;
        }

        public DQDDataContext(FetchDataCallbackHandler back, int num, uint rollNum, string targetHost , InitSelector type) {
            FetchCallback = back;
            number = num;
            rollNumber = rollNum;
            this.targetHost = targetHost;
            InitType = type;
            if(InitType == InitSelector.Special) { LoadPreview(); }
        }

        private async void LoadPreview() { await LoadMoreItemsAsync(0); InitType = InitSelector.Default; }

        protected override bool HasMoreItemsOrNot() { return true; }

        protected override async Task<IList<object>> LoadItemsAsync(CancellationToken cancToken, uint count) {
            //await Task.Delay(10);
            wholeCount += rollNumber;
            var coll = await FetchCallback.Invoke( number, rollNumber,wholeCount);

            // Is this ok?
            //return (coll as ObservableCollection<object>).ToArray();
            return coll.ToArray();
        }

        uint wholeCount = 0;
        public delegate Task<ObservableCollection<T>> FetchDataCallbackHandler( int ID , uint rollNum, uint nowWholeCount);
        public FetchDataCallbackHandler FetchCallback;
        private int number;
        private uint rollNumber;
        private string targetHost;
        private InitSelector InitType;
    }
}
