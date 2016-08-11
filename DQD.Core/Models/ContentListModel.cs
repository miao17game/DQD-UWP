using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace DQD.Core.Models {
    [DataContract]
    public class ContentListModel {
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public BitmapImage Image { get; set; }
        [DataMember]
        public string Date { get; set; }
        [DataMember]
        public Uri Path { get; set; }
        [DataMember]
        public Uri Share { get; set; }
        [DataMember]
        public int ShareNum { get; set; }

        public ContentListModel() { }

    }
}
