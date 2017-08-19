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
        public static MoonPhaseToStringConverter Singleton = new MoonPhaseToStringConverter();

        public static readonly string NewMoonName = "New Moon";
        public static readonly string WaxingCrescentMoonName = "Waxing Crescent Moon";
        public static readonly string FirstQuarterMoonName = "First Quarter Moon";
        public static readonly string WaxingGibbousMoonName = "Waxing Gibbous Moon";
        public static readonly string FullMoonName = "Full Moon";
        public static readonly string WaningGibbousMoonName = "Waning Gibbous Moon";
        public static readonly string LastQuarterMoonName = "Last Quarter Moon";
        public static readonly string WaningCrescentMoonName = "Waning Crescent Moon";

        public static Dictionary<MoonPhases, string> MoonPhaseTable { get; private set; } = new Dictionary<MoonPhases, string>()
        {
            { MoonPhases.NewMoon, NewMoonName },
            { MoonPhases.WaxingCrescentMoon, WaxingCrescentMoonName },
            { MoonPhases.FirstQuarterMoon, FirstQuarterMoonName },
            { MoonPhases.WaxingGibbousMoon, WaxingGibbousMoonName },
            { MoonPhases.FullMoon, FullMoonName  },
            { MoonPhases.WaningGibbousMoon, WaningGibbousMoonName },
            { MoonPhases.LastQuarterMoon, LastQuarterMoonName },
            { MoonPhases.WaningCrescentMoon, WaningCrescentMoonName },
        };

        public static List<string> MoonPhaseList { get; private set; } = new List<string>()
        {
            NewMoonName,
            WaxingCrescentMoonName,
            FirstQuarterMoonName,
            WaxingGibbousMoonName,
            FullMoonName,
            WaningGibbousMoonName,
            LastQuarterMoonName,
            WaningCrescentMoonName,
            "(Unselect)"
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            MoonPhases MoonPhaseValue = (MoonPhases)value;
            return MoonPhaseTable[MoonPhaseValue];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
