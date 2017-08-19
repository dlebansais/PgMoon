using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace PgMoon
{
    public class MushroomInfo : INotifyPropertyChanged
    {
        #region Init
        public MushroomInfo(string Name, MoonPhases? PreferredPhase)
        {
            _Name = Name;
            _SelectedMoonPhase = PreferredPhase.HasValue ? (int)PreferredPhase.Value : -1;
        }
        #endregion

        #region Properties
        public string Name
        {
            get { return _Name; }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    NotifyThisPropertyChanged();

                    if (_Name == null || _Name.Length == 0)
                        ResetSelectedMoonPhase();
                }
            }
        }
        private string _Name;

        public int SelectedMoonPhase
        {
            get { return _SelectedMoonPhase; }
            set
            {
                if (_SelectedMoonPhase != value)
                {
                    _SelectedMoonPhase = value;
                    if (_SelectedMoonPhase > (int)MoonPhases.WaningCrescentMoon)
                        ResetSelectedMoonPhase();
                }
            }
        }
        private int _SelectedMoonPhase;

        public MoonPhases? PreferredPhase { get { return (SelectedMoonPhase >= 0) ? (MoonPhases?)SelectedMoonPhase : null; } }
        #endregion

        #region Implementation
        private void ResetSelectedMoonPhase()
        {
            App CurrentApp = App.Current as App;
            MainWindow MainPopup = CurrentApp.MainPopup;
            MainPopup.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new ResetSelectedMoonPhaseHandler(OnResetSelectedMoonPhase));
        }

        private delegate void ResetSelectedMoonPhaseHandler();
        private void OnResetSelectedMoonPhase()
        {
            _SelectedMoonPhase = -1;
            NotifyThisPropertyChanged(nameof(SelectedMoonPhase));
            NotifyThisPropertyChanged(nameof(PreferredPhase));
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
