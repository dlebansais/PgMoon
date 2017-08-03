using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PgMoon
{
    public class PhaseCalculator : INotifyPropertyChanged
    {
        #region Init
        public PhaseCalculator()
        {
            CalculateBasicMoonPhase(DateTime.UtcNow);
        }
        #endregion

        #region Client Interface
        public void Update()
        {
            CalculateBasicMoonPhase(DateTime.UtcNow);
        }

        public void Update(DateTime Time)
        {
            CalculateBasicMoonPhase(Time);
        }
        #endregion

        #region Properties
        public MoonPhases Phase
        {
            get { return _Phase; }
            set
            {
                if (_Phase != value)
                {
                    _Phase = value;
                    NotifyThisPropertyChanged();
                }
            }
        }
        private MoonPhases _Phase;

        public double ProgressWithinLunation
        {
            get { return _ProgressWithinLunation; }
            set
            {
                if (_ProgressWithinLunation != value)
                {
                    _ProgressWithinLunation = value;
                    NotifyThisPropertyChanged();
                }
            }
        }
        private double _ProgressWithinLunation;

        public double ProgressWithinPhase
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
        private double _ProgressWithinPhase;

        public double ProgressToFullMoon
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
        private double _ProgressToFullMoon;

        public TimeSpan TimeToNextPhase
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
        private TimeSpan _TimeToNextPhase;

        public TimeSpan TimeToFullMoon
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
        private TimeSpan _TimeToFullMoon;
        #endregion

        #region Complicated
        private static double RAD
        {
            get { return Math.PI / 180.0; }
        }

        const double SMALL_FLOAT = 1e-12;

        // Returns the number of julian days for the specified day.
        private static double Julian(int year, int month, double day)
        {
            int a, b = 0, c, e;
            if (month < 3)
            {
                year--;
                month += 12;
            }
            if (year > 1582 || (year == 1582 && month > 10) ||
            (year == 1582 && month == 10 && day > 15))
            {
                a = year / 100;
                b = 2 - a + a / 4;
            }
            c = (int)(365.25 * year);
            e = (int)(30.6001 * (month + 1));
            return b + c + e + day + 1720994.5;
        }

        static double sun_position(double j)
        {
            double n, x, e, l, dl, v;
            //double m2;
            int i;

            n = 360 / 365.2422 * j;
            i = (int)(n / 360);
            n = n - i * 360.0;
            x = n - 3.762863;
            if (x < 0) x += 360;
            x *= RAD;
            e = x;
            do
            {
                dl = e - .016718 * Math.Sin(e) - x;
                e = e - dl / (1 - .016718 * Math.Cos(e));
            } while (Math.Abs(dl) >= SMALL_FLOAT);
            v = 360 / Math.PI * Math.Atan(1.01686011182 * Math.Tan(e / 2));
            l = v + 282.596403;
            i = (int)(l / 360);
            l = l - i * 360.0;
            return l;
        }

        static double moon_position(double j, double ls)
        {

            double ms, l, mm, n, ev, sms, ae, ec;//,z,x;//,lm,bm,ec;
                                                 //double d;
                                                 //double ds, as, dm;
            int i;

            /* ls = sun_position(j) */
            ms = 0.985647332099 * j - 3.762863;
            if (ms < 0) ms += 360.0;
            l = 13.176396 * j + 64.975464;
            i = (int)(l / 360);
            l = l - i * 360.0;
            if (l < 0) l += 360.0;
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

        public static int moon_phase2(DateTime CurrentTime)
        {
            int year = CurrentTime.Year;
            int month = CurrentTime.Month;
            int day = CurrentTime.Day;
            int hour = CurrentTime.Hour;

            /*
              Calculates more accurately than Moon_phase , the phase of the moon at
              the given epoch.
              returns the moon phase as a real number (0-1)
              */

            double j = Julian(year, month, (double)day + hour / 24.0) - 2444238.5;
            double ls = sun_position(j);
            double lm = moon_position(j, ls);

            double t = lm - ls;
            if (t < 0) t += 360;

            double Phase = (1.0 - Math.Cos((lm - ls) * RAD)) / 2;
            int PhaseIndex = (int)(Phase * 8);

            return PhaseIndex;
        }
        #endregion

        #region Basic
        public void CalculateBasicMoonPhase(DateTime Time)
        {
            DateTime RecentNewMoon = new DateTime(2017, 7, 23, 3, 0, 0, DateTimeKind.Utc);
            TimeSpan Elapsed = Time - RecentNewMoon;
            double SynodicMonth = 2551442.861088; // Seconds

            ProgressWithinLunation = (((Elapsed.TotalSeconds + (SynodicMonth / 16.0)) % SynodicMonth) / SynodicMonth);
            Phase = (MoonPhases)(ProgressWithinLunation * 8);
            ProgressWithinPhase = (ProgressWithinLunation * 8) - (int)Phase;
            TimeToNextPhase = TimeSpan.FromSeconds(((((int)Phase + 1) - (ProgressWithinLunation * 8)) / 8) * SynodicMonth);
            if (Phase < MoonPhases.FullMoon)
            {
                ProgressToFullMoon = 1.0 - ((((double)MoonPhases.FullMoon) / 8) - ProgressWithinLunation);
                TimeToFullMoon = TimeSpan.FromSeconds((((int)MoonPhases.FullMoon - (ProgressWithinLunation * 8)) / 8) * SynodicMonth);
            }
            else
            {
                ProgressToFullMoon = 1.0 - (ProgressWithinLunation - (((double)MoonPhases.FullMoon) / 8));
                TimeToFullMoon = TimeSpan.FromSeconds(SynodicMonth - (((ProgressWithinLunation * 8) - (int)MoonPhases.FullMoon) / 8) * SynodicMonth);
            }
        }
        // 01/08/17 [04.20,08:53] : FQ -> Waxing Gib
        #endregion

        #region Implementation of INotifyPropertyChanged
        /// <summary>
        ///     Implements the PropertyChanged event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        internal void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Default parameter is mandatory with [CallerMemberName]")]
        internal void NotifyThisPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
