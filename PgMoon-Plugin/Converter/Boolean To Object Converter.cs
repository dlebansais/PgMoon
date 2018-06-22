using System;
using System.Globalization;
using System.Windows.Data;

namespace Converters
{
    [ValueConversion(typeof(bool), typeof(object))]
    public class BooleanToObjectConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int IntValue;

            if (!(value is bool))
                IntValue = 2;
            else
                IntValue = ((bool)value) ? 1 : 0;

            CompositeCollection CollectionOfItems = parameter as CompositeCollection;

            return CollectionOfItems[IntValue];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
