using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PgMoon
{
    public class PhaseCalculator
    {
        #region Init
        static PhaseCalculator()
        {
            Singleton = new PhaseCalculator();
        }

        private PhaseCalculator()
        {
            CalculateBasicMoonPhase(App.Now());
        }

        public static PhaseCalculator Singleton { get; private set; }
        #endregion

        #region Client Interface
        public static void Update()
        {
            Singleton.CalculateBasicMoonPhase(App.Now());
        }

        public static bool IsCurrent(int CheckMoonMonth, MoonPhase CheckMoonPhase)
        {
            return CheckMoonMonth == MoonMonth && CheckMoonPhase == MoonPhase;
        }
        #endregion

        #region Properties
        public static int MoonMonth
        {
            get { return _MoonMonth; }
            set
            {
                if (_MoonMonth != value)
                {
                    _MoonMonth = value;
                    NotifyThisPropertyChanged();
                }
            }
        }
        private static int _MoonMonth;

        public static MoonPhase MoonPhase
        {
            get { return _MoonPhase; }
            set
            {
                if (_MoonPhase != value)
                {
                    _MoonPhase = value;
                    NotifyThisPropertyChanged();

                    MoonPhase.UpdateCurrent();
                }
            }
        }
        private static MoonPhase _MoonPhase;

        public static double ProgressWithinPhase
        {
            get { return _ProgressWithinPhase; }
            set
            {
                if (_ProgressWithinPhase != value)
                {
                    _ProgressWithinPhase = value;
                    NotifyThisPropertyChanged();
                }
            }
        }
        private static double _ProgressWithinPhase;

        public static double ProgressToFullMoon
        {
            get { return _ProgressToFullMoon; }
            set
            {
                if (_ProgressToFullMoon != value)
                {
                    _ProgressToFullMoon = value;
                    NotifyThisPropertyChanged();
                }
            }
        }
        private static double _ProgressToFullMoon;

        public static TimeSpan TimeToNextPhase
        {
            get { return _TimeToNextPhase; }
            set
            {
                if (_TimeToNextPhase != value)
                {
                    _TimeToNextPhase = value;
                    NotifyThisPropertyChanged();
                }
            }
        }
        private static TimeSpan _TimeToNextPhase;

        public static TimeSpan TimeToFullMoon
        {
            get { return _TimeToFullMoon; }
            set
            {
                if (_TimeToFullMoon != value)
                {
                    _TimeToFullMoon = value;
                    NotifyThisPropertyChanged();
                }
            }
        }
        private static TimeSpan _TimeToFullMoon;
        #endregion

        #region Client Interface
        public static void DateTimeToMoonPhase(DateTime Time, out int pMoonMonth, out MoonPhase pMoonPhase, out DateTime pPhaseStartTime, out DateTime pPhaseEndTime, out double pProgressToFullMoon, out DateTime pNextFullMoonTime)
        {
            int ip;
            double p = corrected_moon_phase(Time, out ip);
            TimeSpan time_increment = TimeSpan.FromMinutes(10);

            pMoonPhase = MoonPhase.MoonPhaseList[ip];

            int previp = ip;
            DateTime PhaseStartTime = Time;
            do
            {
                PhaseStartTime -= time_increment;
                corrected_moon_phase(PhaseStartTime, out previp);
            }
            while (previp == ip);

            PhaseStartTime += time_increment;
            pPhaseStartTime = PhaseStartTime;

            pMoonMonth = (int)(to_time_t(PhaseStartTime) / 2551442.861);

            int nextip = ip;
            DateTime PhaseEndTime = Time;
            do
            {
                PhaseEndTime += time_increment;
                corrected_moon_phase(PhaseEndTime, out nextip);
            }
            while (nextip == ip);

            pPhaseEndTime = PhaseEndTime;

            if (ip != 4)
            {
                DateTime AfterFullMoonStartTime = PhaseStartTime;
                DateTime FullMoonEndTime = PhaseEndTime;

                if (previp != 4)
                {
                    do
                    {
                        AfterFullMoonStartTime -= time_increment;
                        corrected_moon_phase(AfterFullMoonStartTime, out previp);
                    }
                    while (previp != 4);

                    AfterFullMoonStartTime += time_increment;
                }

                if (nextip != 4)
                {
                    do
                    {
                        FullMoonEndTime += time_increment;
                        corrected_moon_phase(FullMoonEndTime, out nextip);
                    }
                    while (nextip != 4);
                }

                double Total = (FullMoonEndTime.Ticks - AfterFullMoonStartTime.Ticks);
                double Current = (Time.Ticks - AfterFullMoonStartTime.Ticks);

                pProgressToFullMoon = Current / Total;
                pNextFullMoonTime = FullMoonEndTime;
            }
            else
            {
                pProgressToFullMoon = 0;
                pNextFullMoonTime = DateTime.MinValue;
            }
        }


        private static double corrected_moon_phase(DateTime UtcTime, out int pip)
        {
            int Year, Month, Day, Hour, Minute;

            UtcTime -= TimeSpan.FromHours(4); // Reference is not GMT but GMT-4

            Year = UtcTime.Year;
            Month = UtcTime.Month;
            Day = UtcTime.Day;
            Hour = 0;
            Minute = 0;

            DateTime NextUtcTime = UtcTime + TimeSpan.FromMinutes(1);
            int NextYear, NextMonth, NextDay, NextHour, NextMinute;

            NextYear = NextUtcTime.Year;
            NextMonth = NextUtcTime.Month;
            NextDay = NextUtcTime.Day;
            NextHour = NextUtcTime.Hour;
            NextMinute = NextUtcTime.Minute;

            int ip, nextip;
            double p = moon_phase(Year, Month, Day, Hour, Minute, out ip);
            double nextp = moon_phase(NextYear, NextMonth, NextDay, NextHour, NextMinute, out nextip);

            switch (ip)
            {
                case 0: // NewMoon
                    if (nextp > p)
                        if (p > 0.015)
                            ip = 1; // WaxingCrescentMoon
                    break;
                case 1: // WaxingCrescentMoon
                    break;
                case 2: // FirstQuarterMoon
                    if (nextp > p)
                        if (p > 0.6)
                            ip = 3; // WaxingGibbousMoon
                    break;
                case 3: // WaxingGibbousMoon
                    break;
                case 4: // FullMoon
                    if (nextp < p && p <= 0.985)
                        ip = 5; // WaningGibbousMoon
                    break;
                case 5: // WaningGibbousMoon
                    break;
                case 6: // LastQuarterMoon
                    break;
                case 7: // WaningCrescentMoon:
                    if (nextp < p)
                        if (p < 0.05)
                            ip = 0; // NewMoon
                    break;
            }

            pip = ip;
            return p;
        }

        private static double RAD { get { return Math.PI / 180.0; } }
        private static double SMALL_FLOAT { get { return 1e-12; } }

        // Returns the number of julian days for the specified day.
        private static double Julian(int year, int month, double day)
        {
            int a, b, c, e;
            if (month < 3)
            {
                year--;
                month += 12;
            }
            if (year > 1582 || (year == 1582 && month > 10) || (year == 1582 && month == 10 && day > 15))
            {
                a = year / 100;
                b = 2 - a + a / 4;
            }
            else
                b = 0;
            c = (int)(365.25 * year);
            e = (int)(30.6001 * (month + 1));

            return b + c + e + day + 1720994.5;
        }

        private static double sun_position(double j)
        {
            double n, x, e, l, dl, v;
            int i;

            n = 360 / 365.2422 * j;
            i = (int)(n / 360);
            n = n - i * 360.0;
            x = n - 3.762863;
            if (x < 0)
                x += 360;
            x *= RAD;
            e = x;
            do
            {
                dl = e - .016718 * Math.Sin(e) - x;
                e = e - dl / (1 - .016718 * Math.Cos(e));
            }
            while (Math.Abs(dl) >= SMALL_FLOAT);
            v = 360 / Math.PI * Math.Atan(1.01686011182 * Math.Tan(e / 2));
            l = v + 282.596403;
            i = (int)(l / 360);
            l = l - i * 360.0;

            return l;
        }

        private static double moon_position(double j, double ls)
        {
            double ms, l, mm, n, ev, sms, ae, ec;
            int i;

            // ls = sun_position(j)
            ms = 0.985647332099 * j - 3.762863;
            if (ms < 0)
                ms += 360.0;
            l = 13.176396 * j + 64.975464;
            i = (int)(l / 360);
            l = l - i * 360.0;
            if (l < 0)
                l += 360.0;
            mm = l - 0.1114041 * j - 349.383063;
            i = (int)(mm / 360);
            mm -= i * 360.0;
            n = 151.950429 - 0.0529539 * j;
            i = (int)(n / 360);
            n -= i * 360.0;
            ev = 1.2739 * Math.Sin((2 * (l - ls) - mm) * RAD);
            sms = Math.Sin(ms * RAD);
            ae = 0.1858 * sms;
            mm += ev - ae - 0.37 * sms;
            ec = 6.2886 * Math.Sin(mm * RAD);
            l += ev + ec - ae + 0.214 * Math.Sin(2 * mm * RAD);
            l = 0.6583 * Math.Sin(2 * (l - ls) * RAD) + l;

            return l;
        }

        // Returns the moon phase as a real number (0-1)
        private static double moon_phase(int year, int month, int day, double hour, double minute, out int ip)
        {
            double j = Julian(year, month, day + ((hour + minute / 60.0) / 24.0)) - 2444238.5;
            double ls = sun_position(j);
            double lm = moon_position(j, ls);

            double t = lm - ls;
            if (t < 0)
                t += 360;
            ip = (int)((t + 22.5) / 45) & 0x7;

            return (1.0 - Math.Cos((lm - ls) * RAD)) / 2;
        }
        #endregion

        #region Unix Time
        private static readonly DateTime UnixReferenceTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static long to_time_t(DateTime Time)
        {
            return (long)((Time - UnixReferenceTime).TotalSeconds);
        }

        private static DateTime from_time_t(long Time_t)
        {
            return UnixReferenceTime + TimeSpan.FromSeconds(Time_t);
        }
        #endregion

        #region Calculator
        private void CalculateBasicMoonPhase(DateTime Time)
        {
            int NewMoonMonth;
            MoonPhase NewMoonPhase;
            DateTime NewPhaseStartTime;
            DateTime NewPhaseEndTime;
            double NewProgressToFullMoon;
            DateTime NewNextFullMoonTime;
            DateTimeToMoonPhase(Time, out NewMoonMonth, out NewMoonPhase, out NewPhaseStartTime, out NewPhaseEndTime, out NewProgressToFullMoon, out NewNextFullMoonTime);

            double NewProgressWithinPhase = (Time - NewPhaseStartTime).TotalSeconds / (NewPhaseEndTime - NewPhaseStartTime).TotalSeconds;
            TimeSpan NewTimeToNextPhase = NewPhaseEndTime - Time;
            TimeSpan NewTimeToFullMoon = NewNextFullMoonTime - Time;

            MoonMonth = NewMoonMonth;
            MoonPhase = NewMoonPhase;
            ProgressWithinPhase = NewProgressWithinPhase;
            TimeToNextPhase = NewTimeToNextPhase;
            ProgressToFullMoon = NewProgressToFullMoon;
            TimeToFullMoon = NewTimeToFullMoon;
        }
        #endregion

        #region Implementation of STATIC INotifyPropertyChanged
        /// <summary>
        ///     Implements the PropertyChanged event.
        /// </summary>
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        internal static void NotifyPropertyChanged(string propertyName)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Default parameter is mandatory with [CallerMemberName]")]
        internal static void NotifyThisPropertyChanged([CallerMemberName] string propertyName = "")
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
