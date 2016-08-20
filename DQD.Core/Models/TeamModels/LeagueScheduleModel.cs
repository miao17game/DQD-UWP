using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQD.Core.Models.TeamModels {
    public class LeagueScheduleModel {
        #region Model Type
        public ScheduleType ModelType { get; set; }
        #endregion

        #region Title Model
        public Uri PreTitleUri { get; set; }
        public Uri NextTitleUri { get; set; }
        public string PreTitle { get; set; }
        public string NextTitle { get; set; }
        public string ScheduleTitle { get; set; }
        #endregion

        #region Schedule Model
        public string AwayTeam { get; set; }
        public Uri AwayTeamIcon { get; set; }
        public string HomeTeam { get; set; }
        public Uri HomeTeamIcon { get; set; }
        public string Score { get; set; }
        public string Time { get; set; }
        #endregion

        #region State
        public enum ScheduleType { Content = 1, Title = 2, }
        #endregion
    }
}
