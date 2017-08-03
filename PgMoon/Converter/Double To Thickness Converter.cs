using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Converters
{
    [ValueConversion(typeof(double), typeof(Thickness))]
    public class DoubleToThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double DoubleValue = (double)value;
            double Length;
            if (parameter is string)
                double.TryParse(parameter as string, out Length);
            else
                Length = (double)parameter;

            return new Thickness(0, 0, (1.0 - DoubleValue) * Length, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
