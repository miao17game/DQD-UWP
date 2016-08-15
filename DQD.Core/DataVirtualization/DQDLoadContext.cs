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
using DQD.Core.Models.CommentModels;
using DQD.Core.Tools;

namespace DQD.Core.DataVirtualization {
    // This class implements IncrementalLoadingBase. 
    // To create your own Infinite List, you can create a class like this one that doesn't have 'generator' or 'maxcount', 
    //  and instead downloads items from a live data source in LoadMoreItemsOverrideAsync.
    public class DQDLoadContext<T>:IncrementalLoadingBase {
        #region Constructors

        public DQDLoadContext(FetchDataCallbackHandler back,int num,string targetHost) {
            FetchCallback=back;
            number = num;
            this.targetHost = targetHost;
            _Flag = InitSituation.Special;
            dataType = DataIncrementalType.BaseListContent;
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
            dataType = DataIncrementalType.BaseListContent;
            LoadPreview();
        }

        public DQDLoadContext(int num, string targetHost, DataIncrementalType type) {
            number = num;
            this.targetHost = targetHost;
            _Flag = InitSituation.Special;
            dataType = type;
            LoadPreview();
        }

        public DQDLoadContext() {

        }

        #endregion

        #region Founctions Overrided

        private async void LoadPreview() { await LoadMoreItemsAsync(0); }

        /// <summary>
        /// load resources of list from target html
        /// </summary>
        /// <param name="token">CancellationToken</param>
        /// <param name="count">i don not know what num it is...maybe no use here</param>
        /// <returns></returns>
        protected async override Task<IList<object>> LoadMoreItemsOverrideAsync(CancellationToken token,uint count) {
            // Wait for work 
            await Task.Delay(10);
            if (_Flag == InitSituation.Default) {
                var coll = new ObservableCollection<ContentListModel>();
                wholeCount += 15;
                var targetHost = "http://www.dongqiudi.com?tab={0}&page={1}";
                targetHost = string.Format(targetHost, number, wholeCount / 15);
                coll = await DataHandler.SetHomeListResources(targetHost);
                _Flag = InitSituation.Special;
                return (coll).ToArray();
            }
            /// allComments handler
            if (dataType == DataIncrementalType.AllComsContent) {
                var coll = new List<AllCommentModel>();
                wholeCount += 30;
                // This code simply generates
                var targetHost = "http://dongqiudi.com/article/{0}?page={1}#comment_anchor"; 
                targetHost = string.Format(targetHost, number, wholeCount / 30);
                coll = await DataProcess.GetPageAllComments(targetHost);
                return (coll).ToArray();
            }
            var _coll = new ObservableCollection<ContentListModel>();
            wholeCount += 15;
            // This code simply generates
            var _targetHost= "http://www.dongqiudi.com?tab={1}&page={0}";
            targetHost = string.Format(_targetHost, wholeCount / 15, number);
            _coll = await DataHandler.SetHomeListResources(_targetHost);
            return (_coll).ToArray();
        }

        protected override bool HasMoreItemsOverride() { return true; }

        #endregion

        #region Properties and State

        uint wholeCount = 0;
        public delegate Task<ObservableCollection<T>> FetchDataCallbackHandler(string targetHost);
        public FetchDataCallbackHandler FetchCallback;
        private int number;
        private string targetHost;
        private const string HomeHost = "http://www.dongqiudi.com/";
        private const string HomeHostInsert = "http://www.dongqiudi.com";
        private enum InitSituation { Default=1, Special=2, }
        private InitSituation _Flag;
        private DataIncrementalType dataType;

        #endregion
    }
}
