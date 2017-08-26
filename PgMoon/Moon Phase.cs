using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PgMoon
{
    public class MoonPhase : INotifyPropertyChanged
    {
        #region Init
        public static MoonPhase NewMoon = new MoonPhase("New Moon");
        public static MoonPhase WaxingCrescentMoon = new MoonPhase("Waxing Crescent Moon");
        public static MoonPhase FirstQuarterMoon = new MoonPhase("First Quarter Moon");
        public static MoonPhase WaxingGibbousMoon = new MoonPhase("Waxing Gibbous Moon");
        public static MoonPhase FullMoon = new MoonPhase("Full Moon");
        public static MoonPhase WaningGibbousMoon = new MoonPhase("Waning Gibbous Moon");
        public static MoonPhase LastQuarterMoon = new MoonPhase("Last Quarter Moon");
        public static MoonPhase WaningCrescentMoon = new MoonPhase("Waning Crescent Moon");
        private static MoonPhase NullMoonPhase = new MoonPhase("(Unselect)");

        public static List<MoonPhase> MoonPhaseList = new List<MoonPhase>()
        {
            NewMoon,
            WaxingCrescentMoon,
            FirstQuarterMoon,
            WaxingGibbousMoon,
            FullMoon,
            WaningGibbousMoon,
            LastQuarterMoon,
            WaningCrescentMoon,
            NullMoonPhase,
        };

        public MoonPhase(string Name)
        {
            this.Name = Name;
        }
        #endregion

        #region Properties
        public string Name { get; private set; }
        public bool IsCurrent { get { return PhaseCalculator.MoonPhase == this; } }
        #endregion

        #region Client Interface
        public static void UpdateCurrent()
        {
            foreach (MoonPhase Item in MoonPhaseList)
                Item.Update();
        }

        private void Update()
        {
            NotifyPropertyChanged(nameof(IsCurrent));
        }
        #endregion

        #region Implementation of INotifyPropertyChanged
        /// <summary>
        ///     Implements the PropertyChanged event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        internal void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Default parameter is mandatory with [CallerMemberName]")]
        internal void NotifyThisPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
