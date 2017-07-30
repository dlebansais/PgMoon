using PgMoon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Converters
{
    [ValueConversion(typeof(MoonPhases), typeof(object))]
    public class MoonPhaseToStringConverter : IValueConverter
    {
        public static Dictionary<MoonPhases, string> MoonPhaseTable = new Dictionary<MoonPhases, string>()
        {
            { MoonPhases.NewMoon, "New Moon" },
            { MoonPhases.WaxingCrescentMoon, "Waxing Crescent Moon" },
            { MoonPhases.FirstQuarterMoon, "First Quarter Moon" },
            { MoonPhases.WaxingGibbousMoon, "Waxing Gibbous Moon" },
            { MoonPhases.FullMoon, "Full Moon" },
            { MoonPhases.WaningGibbousMoon, "Waning Gibbous Moon" },
            { MoonPhases.LastQuarterMoon, "Last Quarter Moon" },
            { MoonPhases.WaningCrescentMoon, "Waning Crescent Moon" },
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            MoonPhases MoonPhaseValue = (MoonPhases)value;
            return MoonPhaseTable[MoonPhaseValue];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
