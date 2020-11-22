namespace Converters
{
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
            CultureInfo UsCulture = new CultureInfo("en-US");
            DateTime TimeValue = (DateTime)value;
            TimeValue = TimeValue.ToLocalTime();
            string s = TimeValue.ToString("M", UsCulture) + " " + TimeValue.Hour.ToString("D2", CultureInfo.InvariantCulture) + ":" + "00";

            // string s = TimeValue.ToString("M", UsCulture) + " " + TimeValue.Hour.ToString("D2") + ":" + TimeValue.Minute.ToString("D2");
            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null !;
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1600 // Elements should be documented
}
