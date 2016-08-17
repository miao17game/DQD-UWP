using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQD.Core.Models.TeamModels {
    public class TeamLeagueModel {
        private string Team { get; set; }
        private Uri TeamIcon { get; set; }
        public TopOrBottom UpOrDown { get; set; }
        public uint Rank { get; set; }
        public uint Amount { get; set; }
        public uint Win { get; set; }
        public uint Draw { get; set; }
        public uint Lose { get; set; }
        public uint ScoreBall { get; set; }
        public uint LostBall { get; set; }
        public uint NetBall { get; set; }
        public uint Integral { get; set; }
    }
}
