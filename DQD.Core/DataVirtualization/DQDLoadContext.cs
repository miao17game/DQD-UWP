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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DQD.Core.Models;
using System.Threading;

namespace DQD.Core.DataVirtualization {
    // This class implements IncrementalLoadingBase. 
    // To create your own Infinite List, you can create a class like this one that doesn't have 'generator' or 'maxcount', 
    //  and instead downloads items from a live data source in LoadMoreItemsOverrideAsync.
    public class DQDLoadContext<T>:IncrementalLoadingBase {
        public  DQDLoadContext(FetchDataCallbackHandler back,int num,string targetHost) {
            FetchCallback=back;
            number = num;
            this.targetHost = targetHost;
            _Flag = InitSituation.Special;
        }

        /// <summary>
        /// load resources preview
        /// </summary>
        /// <param name="num"></param>
        /// <param name="targetHost"></param>
        public DQDLoadContext(int num, string targetHost) {
            number = num;
            this.targetHost = targetHost;
            _Flag = InitSituation.Default;
            LoadPreview();
        }

        public DQDLoadContext() {

        }

        /// <summary>
        /// do not wait haha - -
        /// </summary>
        private async void LoadPreview() { await LoadMoreItemsAsync(0); }

        protected async override Task<IList<object>> LoadMoreItemsOverrideAsync(System.Threading.CancellationToken c,uint count) {
            // Wait for work 
            await Task.Delay(10);
            var coll = new ObservableCollection<ContentListModel>();
            if (_Flag == InitSituation.Default) {
                var targetHost = "http://www.dongqiudi.com?tab={1}&page={0}";
                targetHost = string.Format(targetHost, number, 1);
                coll = await DataHandler.SetHomeListResources(targetHost);
                _Flag = InitSituation.Special;
            } else {
                // This code simply generates
                var targetHost = "http://www.dongqiudi.com?tab={1}&page={0}";
                targetHost = string.Format(targetHost, wholeCount / 15 + 1, number);
                coll = await DataHandler.SetHomeListResources(targetHost);
                wholeCount += (uint)coll.Count;
            }
            return (coll).ToArray();
        }

        protected override bool HasMoreItemsOverride() { return true; }

        #region State

        uint wholeCount = 0;
        public delegate Task<ObservableCollection<T>> FetchDataCallbackHandler(string targetHost);
        public FetchDataCallbackHandler FetchCallback;
        private int number;
        private string targetHost;
        private const string HomeHost = "http://www.dongqiudi.com/";
        private const string HomeHostInsert = "http://www.dongqiudi.com";
        private enum InitSituation { Default=1, Special=2, }
        private InitSituation _Flag;

        #endregion
    }
}
