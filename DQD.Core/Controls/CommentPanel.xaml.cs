using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace DQD.Core.Controls {
    public sealed partial class CommentPanel : UserControl {

        public static readonly DependencyProperty ComImageProperty = DependencyProperty.Register("ComImage", typeof(ImageSource), typeof(CommentPanel), null);
        public ImageSource ComImage {
            get { return GetValue(ComImageProperty) as ImageSource; }
            set { SetValue(ComImageProperty, value); }
        }

        public static readonly DependencyProperty ComNameProperty = DependencyProperty.Register("ComName", typeof(string), typeof(CommentPanel), null);
        public string ComName {
            get { return GetValue(ComNameProperty) as string; }
            set { SetValue(ComNameProperty, value); }
        }

        public static readonly DependencyProperty ComTimeProperty = DependencyProperty.Register("ComTime", typeof(string), typeof(CommentPanel), null);
        public string ComTime {
            get { return GetValue(ComTimeProperty) as string; }
            set { SetValue(ComTimeProperty, value); }
        }

        public static readonly DependencyProperty ComContentProperty = DependencyProperty.Register("ComContent", typeof(string), typeof(CommentPanel), null);
        public string ComContent {
            get { return GetValue(ComContentProperty) as string; }
            set { SetValue(ComContentProperty, value); }
        }

        public CommentPanel() {
            this.InitializeComponent();
            this.DataContext = this;
        }
    }
}
