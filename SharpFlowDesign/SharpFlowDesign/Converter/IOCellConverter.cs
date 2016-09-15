using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using SharpFlowDesign.UserControls;
using static System.Windows.Media.ColorConverter;

namespace SharpFlowDesign.Converter
{

    public class IOCellSelectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool selected = (bool)value;
            if (selected)
            {

                return new SolidColorBrush(Colors.DodgerBlue);
            }

            Color color = (Color)ConvertFromString("#FF2E2E2E");
            return new SolidColorBrush(color); ;

        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}



