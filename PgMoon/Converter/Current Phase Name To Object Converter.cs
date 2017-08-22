using PgMoon;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Converters
{
    [ValueConversion(typeof(string), typeof(object))]
    public class CurrentPhaseNameToObjectConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string StringValue = value as string;

            bool IsCurrent;
            if (StringValue != null)
            {
                PhaseCalculator PhaseCalculator = new PhaseCalculator();
                string CurrentName = MoonPhaseToStringConverter.MoonPhaseTable[PhaseCalculator.MoonPhase];
                if (StringValue == CurrentName)
                    IsCurrent = true;
                else
                    IsCurrent = false;
            }
            else
                IsCurrent = false;

            CompositeCollection CollectionOfItems = parameter as CompositeCollection;
            return CollectionOfItems[IsCurrent ? 1 : 0];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
