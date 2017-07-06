using ElectricAnalysis.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace ElectricAnalysis.Converter
{
    public class ComponentColorConvertor : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ComponentState state = (ComponentState)value;
            if (state == ComponentState.Connected)
                return Brushes.Red;
            else if (state == ComponentState.UnConnected)
                return Brushes.Black;
            else if (state == ComponentState.Fault0)
                return new SolidColorBrush(Color.FromArgb(255, 13, 209, 22));
            else if (state == ComponentState.Fault1)
                return new SolidColorBrush(Color.FromArgb(255, 241, 21, 241));
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
