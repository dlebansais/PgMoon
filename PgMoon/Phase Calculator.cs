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

        #region Client Interface
        public static void DateTimeToMoonPhase(DateTime Time, out int MoonMonth, out MoonPhases MoonPhase, out double ProgressWithinLunation, out double ProgressWithinPhase, out DateTime PhaseStartTime, out DateTime PhaseEndTime, out TimeSpan TimeToNextPhase, out double ProgressToFullMoon, out TimeSpan TimeToFullMoon)
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
            ProgressWithinLunation = PositionInMonth.TotalSeconds / SynodicMonth.TotalSeconds;
            ProgressWithinPhase = (ProgressWithinLunation * PhaseTable.Length) - (int)MoonPhase;
            PhaseStartTime = MonthStartTime + PhaseTable[i];
            PhaseEndTime = ((i + 1 < PhaseTable.Length) ? (MonthStartTime + PhaseTable[i + 1]) : MonthStartTime + SynodicMonth);
            TimeToNextPhase = TimeSpan.FromSeconds(((((int)MoonPhase + 1) - (ProgressWithinLunation * PhaseTable.Length)) / PhaseTable.Length) * SynodicMonth.TotalSeconds);
            if (MoonPhase < MoonPhases.FullMoon)
            {
                ProgressToFullMoon = 1.0 - ((((double)MoonPhases.FullMoon) / 8) - ProgressWithinLunation);
                TimeToFullMoon = TimeSpan.FromSeconds((((int)MoonPhases.FullMoon - (ProgressWithinLunation * 8)) / 8) * SynodicMonth.TotalSeconds);
            }
            else
            {
                ProgressToFullMoon = 1.0 - (ProgressWithinLunation - (((double)MoonPhases.FullMoon) / 8));
                TimeToFullMoon = TimeSpan.FromSeconds(SynodicMonth.TotalSeconds - (((ProgressWithinLunation * 8) - (int)MoonPhases.FullMoon) / 8) * SynodicMonth.TotalSeconds);
            }
        }
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
            double ProgressWithinLunation;
            double ProgressWithinPhase;
            DateTime PhaseStartTime;
            DateTime PhaseEndTime;
            TimeSpan TimeToNextPhase;
            double ProgressToFullMoon;
            TimeSpan TimeToFullMoon;
            DateTimeToMoonPhase(Time, out MoonMonth, out MoonPhase, out ProgressWithinLunation, out ProgressWithinPhase, out PhaseStartTime, out PhaseEndTime, out TimeToNextPhase, out ProgressToFullMoon, out TimeToFullMoon);

            this.MoonMonth = MoonMonth;
            this.MoonPhase = MoonPhase;
            this.ProgressWithinLunation = ProgressWithinLunation;
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
        // 19/08/17 [16:00] : Waning Crescent -> New Moon
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
