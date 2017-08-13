using System;

namespace PgMoon
{
    public class CalendarEntry
    {
        #region Init
        public CalendarEntry(MoonPhases MoonPhase, bool IsCurrent, DateTime StartTime, DateTime EndTime)
        {
            this.MoonPhase = MoonPhase;
            this.IsCurrent = IsCurrent;
            this.StartTime = StartTime;
            this.EndTime = EndTime;
        }
        #endregion

        #region Properties
        public MoonPhases MoonPhase { get; private set; }
        public bool IsCurrent { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        #endregion

        #region Implementation
        public override string ToString()
        {
            return MoonPhase.ToString() + " - " + EndTime.ToLocalTime().ToString();
        }
        #endregion
    }
}
