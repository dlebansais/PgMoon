namespace Converters;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented
[ValueConversion(typeof(double), typeof(Thickness))]
public class DoubleToThicknessConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        double DoubleValue = (double)value;
        double Length;

        if (parameter is string AsString)
        {
            if (!double.TryParse(AsString, out Length))
                Length = 0;
        }
        else
        {
            Length = (double)parameter;
        }

        return new Thickness(0, 0, (1.0 - DoubleValue) * Length, 0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1600 // Elements should be documented
