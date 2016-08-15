using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace DQD.Core.Models.CommentModels {
    public class AllCommentModel {
        public string Name { get; set; }
        public BitmapImage Image { get; set; }
        public string Time { get; set; }
        public string Content { get; set; }
        public string Rel { get; set; }
        public string ID { get; set; }
        public string Res { get; set; }
        public string ReName { get; set; }
        public string ReTime { get; set; }
        public string ReContent { get; set; }
    }
}
