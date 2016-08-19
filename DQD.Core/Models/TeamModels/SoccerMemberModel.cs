using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQD.Core.Models.TeamModels {
    public class SoccerMemberModel {
        #region Model Type
        public SocMenType ModelType { get; set; }
        #endregion

        #region Member Model
        public uint Rank { get; set; }
        public string Member { get; set; }
        public Uri MemberIcon { get; set; }
        public string Team { get; set; }
        public Uri TeamIcon { get; set; }
        public uint Stat { get; set; }
        #endregion

        #region Header Model
        public string RankHeader { get; set; }
        public string MemberHeader { get; set; }
        public string TeamHeader { get; set; }
        public string StatHeader { get; set; }
        #endregion

        #region State
        public enum SocMenType { Content = 2,Header = 3, }
        #endregion
    }
}
