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
        [System.Runtime.InteropServices.DllImport("MoonPhaseCalculator.dll")]
        public static extern void DateTimeToMoonPhase(long Time, out int MoonMonth, out int MoonPhase, out long PhaseStartTime, out long PhaseEndTime, out double ProgressToFullMoon, out long NextFullMoonTime);

        private static readonly DateTime UnixReferenceTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static long to_time_t(DateTime Time)
        {
            return (long)((Time - UnixReferenceTime).TotalSeconds);
        }

        private static DateTime from_time_t(long Time_t)
        {
            return UnixReferenceTime + TimeSpan.FromSeconds(Time_t);
        }

        public static void DateTimeToMoonPhase2(DateTime Time, out int MoonMonth, out MoonPhase MoonPhase, out DateTime PhaseStartTime, out DateTime PhaseEndTime, out double ProgressToFullMoon, out DateTime NextFullMoonTime)
        {
            long Time_t = to_time_t(Time);
            int MoonPhaseAsInt;
            long PhaseStartTime_t;
            long PhaseEndTime_t;
            long NextFullMoonTime_t;

            DateTimeToMoonPhase(Time_t, out MoonMonth, out MoonPhaseAsInt, out PhaseStartTime_t, out PhaseEndTime_t, out ProgressToFullMoon, out NextFullMoonTime_t);

            MoonPhase = MoonPhase.MoonPhaseList[MoonPhaseAsInt];
            PhaseStartTime = from_time_t(PhaseStartTime_t);
            PhaseEndTime = from_time_t(PhaseEndTime_t);
            NextFullMoonTime = from_time_t(NextFullMoonTime_t);
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
            DateTimeToMoonPhase2(Time, out NewMoonMonth, out NewMoonPhase, out NewPhaseStartTime, out NewPhaseEndTime, out NewProgressToFullMoon, out NewNextFullMoonTime);

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
