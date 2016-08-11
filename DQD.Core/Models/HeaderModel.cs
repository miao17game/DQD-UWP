using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DQD.Core.Models {
    [DataContract]
    public class HeaderModel {
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string pathHref { get; set; }
        [DataMember]
        public int Number { get; set; }
    }
}
