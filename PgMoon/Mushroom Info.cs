using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace PgMoon
{
    public class MushroomInfo : INotifyPropertyChanged
    {
        #region Init
        public MushroomInfo(string Name, MoonPhase PreferredPhase1, MoonPhase PreferredPhase2)
        {
            _Name = Name;
            _SelectedMoonPhase1 = (PreferredPhase1 != null ? MoonPhase.MoonPhaseList.IndexOf(PreferredPhase1) : -1);
            _SelectedMoonPhase2 = (PreferredPhase2 != null ? MoonPhase.MoonPhaseList.IndexOf(PreferredPhase2) : -1);
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
                    {
                        ResetSelectedMoonPhase1();
                        ResetSelectedMoonPhase2();
                    }
                }
            }
        }
        private string _Name;

        public int SelectedMoonPhase1
        {
            get { return _SelectedMoonPhase1; }
            set
            {
                if (_SelectedMoonPhase1 != value)
                {
                    _SelectedMoonPhase1 = value;
                    if (_SelectedMoonPhase1 + 1 >= MoonPhase.MoonPhaseList.Count)
                        ResetSelectedMoonPhase1();
                    else
                        NotifyPropertyChanged(nameof(PreferredPhase1));
                }
            }
        }
        private int _SelectedMoonPhase1;

        public int SelectedMoonPhase2
        {
            get { return _SelectedMoonPhase2; }
            set
            {
                if (_SelectedMoonPhase2 != value)
                {
                    _SelectedMoonPhase2 = value;
                    if (_SelectedMoonPhase2 + 1 >= MoonPhase.MoonPhaseList.Count)
                        ResetSelectedMoonPhase2();
                    else
                        NotifyPropertyChanged(nameof(PreferredPhase2));
                }
            }
        }
        private int _SelectedMoonPhase2;

        public MoonPhase PreferredPhase1 { get { return (SelectedMoonPhase1 >= 0) ? MoonPhase.MoonPhaseList[SelectedMoonPhase1] : null; } }
        public MoonPhase PreferredPhase2 { get { return (SelectedMoonPhase2 >= 0) ? MoonPhase.MoonPhaseList[SelectedMoonPhase2] : null; } }
        #endregion

        #region Implementation
        private void ResetSelectedMoonPhase1()
        {
            App CurrentApp = App.Current as App;
            MainWindow MainPopup = CurrentApp.MainPopup;
            MainPopup.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new ResetSelectedMoonPhaseHandler(OnResetSelectedMoonPhase1));
        }

        private void ResetSelectedMoonPhase2()
        {
            App CurrentApp = App.Current as App;
            MainWindow MainPopup = CurrentApp.MainPopup;
            MainPopup.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new ResetSelectedMoonPhaseHandler(OnResetSelectedMoonPhase2));
        }

        private delegate void ResetSelectedMoonPhaseHandler();
        private void OnResetSelectedMoonPhase1()
        {
            _SelectedMoonPhase1 = -1;
            NotifyPropertyChanged(nameof(SelectedMoonPhase1));
            NotifyPropertyChanged(nameof(PreferredPhase1));
        }
        private void OnResetSelectedMoonPhase2()
        {
            _SelectedMoonPhase2 = -1;
            NotifyPropertyChanged(nameof(SelectedMoonPhase2));
            NotifyPropertyChanged(nameof(PreferredPhase2));
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
