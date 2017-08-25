using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PgMoon
{
    public class CalendarEntry : INotifyPropertyChanged
    {
        #region Init
        public CalendarEntry(int MoonMonth, MoonPhase MoonPhase, DateTime StartTime, DateTime EndTime)
        {
            this.MoonMonth = MoonMonth;
            this.MoonPhase = MoonPhase;
            this.StartTime = StartTime;
            this.EndTime = EndTime;
        }
        #endregion

        #region Properties
        public int MoonMonth { get; private set; }
        public MoonPhase MoonPhase { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public bool IsCurrent { get { return PhaseCalculator.IsCurrent(MoonMonth, MoonPhase); } }
        #endregion

        #region Client Interface
        public void Update()
        {
            NotifyPropertyChanged(nameof(IsCurrent));
        }
        #endregion

        #region Implementation
        public override string ToString()
        {
            return MoonPhase.ToString() + " - " + EndTime.ToLocalTime().ToString();
        }
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
