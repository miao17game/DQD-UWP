using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQD.Core.Models.TeamModels {
    public class TeamLeagueModel {
        #region Model Type
        public TeamModelType ModelType { get; set; }
        #endregion

        #region List Title Model
        public string ListTitle { get; set; }
        #endregion

        #region LeagueTeam Header Model
        public string TeamHeader { get; set; }
        public string RankHeader { get; set; }
        public string AmountHeader { get; set; }
        public string WinHeader { get; set; }
        public string DrawHeader { get; set; }
        public string LoseHeader { get; set; }
        public string ScoreBallHeader { get; set; }
        public string LostBallHeader { get; set; }
        public string NetBallHeader { get; set; }
        public string IntegralHeader { get; set; }
        #endregion

        #region League Team Model
        public string Team { get; set; }
        public Uri TeamIcon { get; set; }
        public TopOrBottom UpOrDown { get; set; }
        public uint Rank { get; set; }
        public uint Amount { get; set; }
        public uint Win { get; set; }
        public uint Draw { get; set; }
        public uint Lose { get; set; }
        public uint ScoreBall { get; set; }
        public uint LostBall { get; set; }
        public int NetBall { get; set; }
        public uint Integral { get; set; }
        #endregion

        #region Cup Team Model
        public string AwayTeam { get; set; }
        public Uri AwayTeamIcon { get; set; }
        public string HomeTeam { get; set; }
        public Uri HomeTeamIcon { get; set; }
        public string Score { get; set; }
        public string Time { get; set; }
        public TopOrBottom TopRankOrNot { get; set; }
        #endregion

        #region State
        public enum TeamModelType { ListTitle = 1, LeagueTeam = 2, CupModel = 3, LeaTeamHeader = 4,}
        public enum TopOrBottom { Top = 1, Bottom = 2, None = 3, }
        #endregion
    }
}
