using System;
using System.Globalization;
using System.Windows.Data;

namespace Converters
{
    [ValueConversion(typeof(DateTime), typeof(object))]
    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CultureInfo UsCulture = new CultureInfo("en-US");
            DateTime TimeValue = (DateTime)value;
            TimeValue = TimeValue.ToLocalTime();
            string s = TimeValue.ToString("M", UsCulture) + " " + TimeValue.Hour.ToString("D2") + ":" + TimeValue.Minute.ToString("D2");

            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
