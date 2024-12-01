namespace Converters;

using System;
using System.Globalization;
using System.Windows.Data;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented
[ValueConversion(typeof(DateTime), typeof(object))]
public class DateTimeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        CultureInfo UsCulture = new("en-US");
        DateTime TimeValue = (DateTime)value;
        TimeValue = TimeValue.ToLocalTime();
        string s = TimeValue.ToString("M", UsCulture) + " " + TimeValue.Hour.ToString("D2", CultureInfo.InvariantCulture) + ":" + "00";
        return s;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1600 // Elements should be documented
