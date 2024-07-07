using System;

namespace MoonPhaseCalculator;

public class MoonPhaseInfo
{
    public MoonPhaseInfo(DateTime dateTime)
    {
        JulianDay j = new JulianDay(dateTime.Year, dateTime.Month, dateTime.Day);
        Moon = new Moon(j);
    }

    public Moon Moon { get; }
    public MoonPhase? Phase { get; set; }
}
