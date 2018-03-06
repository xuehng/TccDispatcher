using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace renstech.NET.SupernovaDispatcher.Converter
{
    [ValueConversion(typeof(double), typeof(double))]
    public class PiexlsMinusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double? val = value as double?;
            if (val != null)
            {
                return val - 10;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
