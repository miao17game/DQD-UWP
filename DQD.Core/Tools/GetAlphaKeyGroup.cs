using DQD.Core.Models.MatchModels;
using DQD.Core.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noodles_Blog_ClassLibrary.Tools {
    /// <summary>
    /// 建立AlphaKeyGroupListView绑定数据源
    /// </summary>
    public static class GetAlphaKeyGroup {
        /// <summary>
        /// 以ItemContent分组
        /// </summary>
        /// <param name="list">ArchiveCategory列表</param>
        /// <returns></returns>
        public static List<AlphaKeyGroup<MatchListModel>> GetAlphaGroupSampleItems(List<MatchListModel> list) {
            List<MatchListModel> data = new List<MatchListModel>();
            data = list;
            List<AlphaKeyGroup<MatchListModel>> groupData = AlphaKeyGroup<MatchListModel>.CreateGroupsForMatch(
                data, (MatchListModel s) => {
                    return s.GroupCategory;
                }, true);
            return groupData;
        }
    }
}
