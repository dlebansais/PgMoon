namespace PgMoon
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Threading;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented
    public class MushroomInfo : INotifyPropertyChanged
    {
        #region Init
        public MushroomInfo(Dispatcher dispatcher, string name, string comment, MoonPhase? robustGrowthPhase1, MoonPhase? robustGrowthPhase2)
        {
            Dispatcher = dispatcher;
            NameInternal = name;
            CommentInternal = comment;
            SelectedMoonPhase1Internal = robustGrowthPhase1 != null ? MoonPhase.MoonPhaseList.IndexOf(robustGrowthPhase1) : -1;
            SelectedMoonPhase2Internal = robustGrowthPhase2 != null ? MoonPhase.MoonPhaseList.IndexOf(robustGrowthPhase2) : -1;
        }

        public Dispatcher Dispatcher { get; private set; }
        #endregion

        #region Properties
        public string Name
        {
            get { return NameInternal; }
            set
            {
                if (NameInternal != value)
                {
                    NameInternal = value;
                    NotifyThisPropertyChanged();

                    if (NameInternal == null || NameInternal.Length == 0)
                    {
                        ResetSelectedMoonPhase1();
                        ResetSelectedMoonPhase2();
                    }
                }
            }
        }

        private string NameInternal;

        public string Comment
        {
            get { return CommentInternal; }
            set
            {
                if (CommentInternal != value)
                {
                    CommentInternal = value;
                    NotifyThisPropertyChanged();
                }
            }
        }

        private string CommentInternal;

        public int SelectedMoonPhase1
        {
            get { return SelectedMoonPhase1Internal; }
            set
            {
                if (SelectedMoonPhase1Internal != value)
                {
                    SelectedMoonPhase1Internal = value;
                    if (SelectedMoonPhase1Internal + 1 >= MoonPhase.MoonPhaseList.Count)
                        ResetSelectedMoonPhase1();
                    else
                        NotifyPropertyChanged(nameof(RobustGrowthPhase1));
                }
            }
        }

        private int SelectedMoonPhase1Internal;

        public int SelectedMoonPhase2
        {
            get { return SelectedMoonPhase2Internal; }
            set
            {
                if (SelectedMoonPhase2Internal != value)
                {
                    SelectedMoonPhase2Internal = value;
                    if (SelectedMoonPhase2Internal + 1 >= MoonPhase.MoonPhaseList.Count)
                        ResetSelectedMoonPhase2();
                    else
                        NotifyPropertyChanged(nameof(RobustGrowthPhase2));
                }
            }
        }

        private int SelectedMoonPhase2Internal;

        public MoonPhase? RobustGrowthPhase1 { get { return (SelectedMoonPhase1 >= 0) ? MoonPhase.MoonPhaseList[SelectedMoonPhase1] : null; } }
        public MoonPhase? RobustGrowthPhase2 { get { return (SelectedMoonPhase2 >= 0) ? MoonPhase.MoonPhaseList[SelectedMoonPhase2] : null; } }
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
            SelectedMoonPhase1Internal = -1;
            NotifyPropertyChanged(nameof(SelectedMoonPhase1));
            NotifyPropertyChanged(nameof(RobustGrowthPhase1));
        }

        private void OnResetSelectedMoonPhase2()
        {
            SelectedMoonPhase2Internal = -1;
            NotifyPropertyChanged(nameof(SelectedMoonPhase2));
            NotifyPropertyChanged(nameof(RobustGrowthPhase2));
        }
        #endregion

        #region Implementation of INotifyPropertyChanged
        /// <summary>
        /// Implements the PropertyChanged event.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Invoke handlers of the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Invoke handlers of the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected void NotifyThisPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1600 // Elements should be documented
}
