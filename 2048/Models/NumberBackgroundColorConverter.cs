using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace _2048.Models
{
    public sealed class NumberBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int intValue = (int)value;

            if(intValue == 0)
            {
                return new SolidColorBrush(Color.FromArgb(255, 205, 191, 178));
            }
            else if(intValue == 2)
            {
                return new SolidColorBrush(Color.FromArgb(255, 238, 228, 218));
            }
            else if (intValue == 4)
            {
                return new SolidColorBrush(Color.FromArgb(255, 237, 224, 200));
            }
            else if (intValue == 8)
            {
                return new SolidColorBrush(Color.FromArgb(255, 244, 177, 121));
            }
            else if (intValue == 16)
            {
                return new SolidColorBrush(Color.FromArgb(255, 245, 149, 99));
            }
            else if(intValue >= 2048)
            {
                return new SolidColorBrush(Colors.Gold);
            }
            else
            {
                return new SolidColorBrush(Color.FromArgb(255, 246, 124, 95));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}