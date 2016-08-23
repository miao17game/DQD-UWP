using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQD.Core.Models.MatchModels {
    public class MatchListModel {
        public long Rel { get; set; }
        public string ID { get; set; }
        public string Time { get; set; }
        public string MatchRound { get; set; }
        public string AwayTeam { get; set; }
        public Uri AwayImage { get; set; }
        public string HomeTeam { get; set; }
        public Uri HomeImage { get; set; }
        public string Score { get; set; }
        public bool IsOverOrNot { get; set; }
        public Uri ArticleLink { get; set; }
        public int? ArticleID { get; set; }
        public string GroupCategory { get; set; }
        public MatchListType MatchType { get; set; }
    }
}
