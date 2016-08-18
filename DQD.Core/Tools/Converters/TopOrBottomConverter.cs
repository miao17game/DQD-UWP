using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace DQD.Core.Tools.Converters {
    public class TopOrBottomConverter: IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            return ConvertToVisibility((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }

        private Brush ConvertToVisibility(string value) {
            return value.Equals("Top") ? Application.Current.Resources["DQDListTopColor"] as Brush :
                value.Equals("Bottom") ? Application.Current.Resources["DQDListBottomColor"] as Brush :
                null;
        }
    }
}
