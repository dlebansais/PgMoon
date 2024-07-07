namespace MoonPhaseCalculator;

using System;

public class JulianDay
{
    private const double SecondsInDay = 24 * 60 * 60;
    private const double ThirtyPlus = 30.6001;

    public JulianDay(int year, int month, double dayFraction)
    {
        Year = year;
        Month = month;
        DayFraction = dayFraction;

        SetDayHourMinuteSecond();
        SetJulianDay();
    }

    public double Value { get; private set; }

    private void SetDayHourMinuteSecond()
    {
        int SecondsInMonth = (int)Math.Round(SecondsInDay * DayFraction, MidpointRounding.AwayFromZero); // how many seconds in the day, d
        Seconds = SecondsInMonth % 60;
        int MinutesInMonth = (SecondsInMonth - Seconds) / 60;
        Minute = MinutesInMonth % 60;
        int HoursInMonth = (MinutesInMonth - Minute) / 60;
        Hour = HoursInMonth % 24;
        int DaysInMonth = (HoursInMonth - Hour) / 24;
        Day = DaysInMonth;
    }

    private void SetJulianDay()
    {
        // here we go with Jean Meeus, p. 61 of 2nd edition of Astronomical Algorithms
        switch (Month)
        {
            case 1:                 // January (don't dare say Calendar.JANUARY here, it will be wrong)
            case 2:                 // February
                Year += -1;
                Month += 12;
                break;
        }

        int b = 0;

        if (IsDateGregorian)
        {
            int a = (int)Math.Floor(Year / 100.0);
            b = 2 - a + (int)Math.Floor(a / 4.0);
        }

        // finally
        Value = Math.Floor(365.25 * (Year + 4716)) + Math.Floor(ThirtyPlus * (Month + 1)) + DayFraction + b - 1524.5;
    }

    public bool IsDateGregorian
    {
        get
        {
            // 4 October 1582
            if (Year > 1582) return true;
            if (Year < 1582) return false;

            // shit, somebody is messing with us!
            if (Month > 10) return true;
            if (Month < 10) return false;

            // now it is evident that they're fucking with us---or it is
            // my Monte-Carlo testing.  :)
            if (Day > 4) return true;
            if (Day < 4) return false;

            return false;           // this is probably the last non-Gregorian date?
        }
    }

    private int Year;
    private int Month;
    private int Day;
    private int Hour;
    private int Minute;
    private int Seconds;
    private double DayFraction;
}
