using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace renstech.NET.SupernovaDispatcher.Converter
{
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class String2VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(value as string))
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
