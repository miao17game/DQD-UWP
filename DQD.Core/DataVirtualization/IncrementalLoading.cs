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

namespace DQD.Core.DataVirtualization {
    // This class implements IncrementalLoadingBase. 
    // To create your own Infinite List, you can create a class like this one that doesn't have 'generator' or 'maxcount', 
    //  and instead downloads items from a live data source in LoadMoreItemsOverrideAsync.
    public class IncrementalLoading<T>:IncrementalLoadingBase {
        private const string HomeHost = "http://www.dongqiudi.com/";
        private const string HomeHostInsert = "http://www.dongqiudi.com";

        public  IncrementalLoading(FetchDataEventHandler back) {
            FetchCallback=back;
        }

        public IncrementalLoading() { 
        }

        protected async override Task<IList<object>> LoadMoreItemsOverrideAsync(System.Threading.CancellationToken c,uint count) {
            uint toGenerate = count;

            // Wait for work 
            await Task.Delay(10);

            // This code simply generates
            var coll = new ObservableCollection<HomeListModel>();

            var targetHost = "http://www.dongqiudi.com?tab=1&page={0}";
            targetHost=string.Format(targetHost,_count/15+1);
            coll=await DataHandler.SetHomeListResources(targetHost);

            //if(_count/15+1>1) {
            //    var targetHost = "http://www.dongqiudi.com?tab=1&page={0}";
            //    targetHost=string.Format(targetHost,_count/15+1);
            //    coll = await DataHandler.SetHomeListResources(targetHost);
            //} else { coll=await DataHandler.SetHomeListResources(HomeHost); }

            _count+=(uint)coll.Count;

            return coll.ToArray();
        }

        protected override bool HasMoreItemsOverride() {
            return true;
        }

        #region State

        uint _count = 0;
        public delegate EventHandler<T> FetchDataEventHandler(string targetHost);
        public FetchDataEventHandler FetchCallback;

        #endregion
    }
}
