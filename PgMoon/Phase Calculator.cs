using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PgMoon
{
    public class PhaseCalculator : INotifyPropertyChanged
    {
        #region Init
        static PhaseCalculator()
        {
            InitPhaseTable();
        }

        public PhaseCalculator()
        {
            CalculateBasicMoonPhase(DateTime.UtcNow);
        }

        private static readonly TimeSpan SynodicMonth = TimeSpan.FromSeconds(2551442.861);
        private static readonly TimeSpan MainPhaseDuration = TimeSpan.FromDays(3);
        private static readonly TimeSpan IntermediatePhaseDuration = TimeSpan.FromSeconds((SynodicMonth.TotalSeconds - (MainPhaseDuration.TotalSeconds * 4)) / 4);
        //private static readonly DateTime RecentNewMoon = new DateTime(2017, 8, 9, 4, 0, 5, DateTimeKind.Utc);
        private static readonly DateTime RecentNewMoon = new DateTime(2017, 7, 22, 9, 38, 0, DateTimeKind.Utc);
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
        public int MoonMonth
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
        private int _MoonMonth;

        public MoonPhases MoonPhase
        {
            get { return _MoonPhase; }
            set
            {
                if (_MoonPhase != value)
                {
                    _MoonPhase = value;
                    NotifyThisPropertyChanged();
                }
            }
        }
        private MoonPhases _MoonPhase;

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

        public static void DateTimeToMoonPhase2(DateTime Time, out int MoonMonth, out MoonPhases MoonPhase, out DateTime PhaseStartTime, out DateTime PhaseEndTime, out double ProgressToFullMoon, out DateTime NextFullMoonTime)
        {
            long Time_t = to_time_t(Time);
            int MoonPhaseAsInt;
            long PhaseStartTime_t;
            long PhaseEndTime_t;
            long NextFullMoonTime_t;

            DateTimeToMoonPhase(Time_t, out MoonMonth, out MoonPhaseAsInt, out PhaseStartTime_t, out PhaseEndTime_t, out ProgressToFullMoon, out NextFullMoonTime_t);

            MoonPhase = (MoonPhases)MoonPhaseAsInt;
            PhaseStartTime = from_time_t(PhaseStartTime_t);
            PhaseEndTime = from_time_t(PhaseEndTime_t);
            NextFullMoonTime = from_time_t(NextFullMoonTime_t);
        }

        /*
        private static void DateTimeToMoonPhase(DateTime Time, out int MoonMonth, out MoonPhases MoonPhase, out DateTime PhaseStartTime, out DateTime PhaseEndTime, out double ProgressToFullMoon, out DateTime NextFullMoonTime)
        {
            TimeSpan Elapsed = Time - RecentNewMoon;

            MoonMonth = (int)(Elapsed.TotalSeconds / SynodicMonth.TotalSeconds);
            DateTime MonthStartTime = RecentNewMoon + TimeSpan.FromSeconds(SynodicMonth.TotalSeconds * MoonMonth);
            TimeSpan PositionInMonth = TimeSpan.FromSeconds(Elapsed.TotalSeconds - (MoonMonth * SynodicMonth.TotalSeconds));

            //DateTime RealStart = RecentNewMoon - MainPhaseDuration - IntermediatePhaseDuration - MainPhaseDuration - IntermediatePhaseDuration - MainPhaseDuration;

            int i;
            for (i = 0; i + 1 < PhaseTable.Length; i++)
                if (PositionInMonth < PhaseTable[i + 1])
                    break;

            MoonPhase = (MoonPhases)i;
            double ProgressWithinLunation = PositionInMonth.TotalSeconds / SynodicMonth.TotalSeconds;
            PhaseStartTime = MonthStartTime + PhaseTable[i];
            PhaseEndTime = ((i + 1 < PhaseTable.Length) ? (MonthStartTime + PhaseTable[i + 1]) : MonthStartTime + SynodicMonth);
            if (MoonPhase < MoonPhases.FullMoon)
            {
                ProgressToFullMoon = 1.0 - ((((double)MoonPhases.FullMoon) / 8) - ProgressWithinLunation);
                TimeSpan TimeToFullMoon = TimeSpan.FromSeconds((((int)MoonPhases.FullMoon - (ProgressWithinLunation * 8)) / 8) * SynodicMonth.TotalSeconds);
                NextFullMoonTime = Time + TimeToFullMoon;
            }
            else
            {
                ProgressToFullMoon = 1.0 - (ProgressWithinLunation - (((double)MoonPhases.FullMoon) / 8));
                TimeSpan TimeToFullMoon = TimeSpan.FromSeconds(SynodicMonth.TotalSeconds - (((ProgressWithinLunation * 8) - (int)MoonPhases.FullMoon) / 8) * SynodicMonth.TotalSeconds);
                NextFullMoonTime = Time + TimeToFullMoon;
            }
        }*/
        #endregion

        #region Calculator
        private static void InitPhaseTable()
        {
            PhaseTable = new TimeSpan[8];
            TimeSpan PhaseStart = TimeSpan.Zero;
            for (int PhaseIndex = 0; PhaseIndex < PhaseTable.Length; PhaseIndex += 2)
            {
                PhaseTable[PhaseIndex + 0] = PhaseStart;
                PhaseStart += MainPhaseDuration;

                PhaseTable[PhaseIndex + 1] = PhaseStart;
                PhaseStart += IntermediatePhaseDuration;
            }
        }

        private void CalculateBasicMoonPhase(DateTime Time)
        {
            int MoonMonth;
            MoonPhases MoonPhase;
            DateTime PhaseStartTime;
            DateTime PhaseEndTime;
            double ProgressToFullMoon;
            DateTime NextFullMoonTime;
            DateTimeToMoonPhase2(Time, out MoonMonth, out MoonPhase, out PhaseStartTime, out PhaseEndTime, out ProgressToFullMoon, out NextFullMoonTime);

            double ProgressWithinPhase = (Time - PhaseStartTime).TotalSeconds / (PhaseEndTime - PhaseStartTime).TotalSeconds;
            TimeSpan TimeToNextPhase = PhaseEndTime - Time;
            TimeSpan TimeToFullMoon = NextFullMoonTime - Time;

            this.MoonMonth = MoonMonth;
            this.MoonPhase = MoonPhase;
            this.ProgressWithinPhase = ProgressWithinPhase;
            this.TimeToNextPhase = TimeToNextPhase;
            this.ProgressToFullMoon = ProgressToFullMoon;
            this.TimeToFullMoon = TimeToFullMoon;
        }

        private static TimeSpan[] PhaseTable;

        // 01/08/17 [04.20,08:53] : FQ -> Waxing Gib
        // 06/08/17 [01.30,08:09] : Waxing Gib -> Full (06:00?)
        // 09/08/17 [06.04,06:06] : Full -> Waning Gibbous
        // 14/08/17 [05:17,09:08] : Waning Gibbous -> Last Quarter
        // 17/08/17 [05:47,07:04] : Last Quarter -> Waning Crescent
        // 20/08/17 [04:18,09:12] : Waning Crescent -> New Moon
        // 23/08/17 [06:05,06:09] : New Moon -> Waxing Crescent
        //06:05,06:09 
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
