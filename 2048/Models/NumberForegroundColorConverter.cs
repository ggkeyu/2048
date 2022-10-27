using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace _2048.Models
{
    public sealed class NumberForegroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int intValue = (int)value;

            if (intValue == 2)
            {
                return new SolidColorBrush(Color.FromArgb(255, 119,110,101));
            }
            else if (intValue == 4)
            {
                return new SolidColorBrush(Color.FromArgb(255, 119, 110, 101));
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}