using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace PgMoon
{
    public class MushroomInfo : INotifyPropertyChanged
    {
        #region Init
        public MushroomInfo(Dispatcher dispatcher, string Name, string Comment, MoonPhase RobustGrowthPhase1, MoonPhase RobustGrowthPhase2)
        {
            Dispatcher = dispatcher;
            _Name = Name;
            _Comment = Comment;
            _SelectedMoonPhase1 = (RobustGrowthPhase1 != null ? MoonPhase.MoonPhaseList.IndexOf(RobustGrowthPhase1) : -1);
            _SelectedMoonPhase2 = (RobustGrowthPhase2 != null ? MoonPhase.MoonPhaseList.IndexOf(RobustGrowthPhase2) : -1);
        }

        public Dispatcher Dispatcher { get; private set; }
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

        public string Comment
        {
            get { return _Comment; }
            set
            {
                if (_Comment != value)
                {
                    _Comment = value;
                    NotifyThisPropertyChanged();
                }
            }
        }
        private string _Comment;

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
                        NotifyPropertyChanged(nameof(RobustGrowthPhase1));
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
                        NotifyPropertyChanged(nameof(RobustGrowthPhase2));
                }
            }
        }
        private int _SelectedMoonPhase2;

        public MoonPhase RobustGrowthPhase1 { get { return (SelectedMoonPhase1 >= 0) ? MoonPhase.MoonPhaseList[SelectedMoonPhase1] : null; } }
        public MoonPhase RobustGrowthPhase2 { get { return (SelectedMoonPhase2 >= 0) ? MoonPhase.MoonPhaseList[SelectedMoonPhase2] : null; } }
        #endregion

        #region Implementation
        private void ResetSelectedMoonPhase1()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new ResetSelectedMoonPhaseHandler(OnResetSelectedMoonPhase1));
        }

        private void ResetSelectedMoonPhase2()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new ResetSelectedMoonPhaseHandler(OnResetSelectedMoonPhase2));
        }

        private delegate void ResetSelectedMoonPhaseHandler();
        private void OnResetSelectedMoonPhase1()
        {
            _SelectedMoonPhase1 = -1;
            NotifyPropertyChanged(nameof(SelectedMoonPhase1));
            NotifyPropertyChanged(nameof(RobustGrowthPhase1));
        }
        private void OnResetSelectedMoonPhase2()
        {
            _SelectedMoonPhase2 = -1;
            NotifyPropertyChanged(nameof(SelectedMoonPhase2));
            NotifyPropertyChanged(nameof(RobustGrowthPhase2));
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
