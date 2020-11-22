namespace PgMoon
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Calendar.v3;
    using Google.Apis.Calendar.v3.Data;
    using Google.Apis.Services;
    using Google.Apis.Util.Store;
    using Microsoft.Win32;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1601 // Partial elements should be documented
    public partial class ShareCalendarWindow : Window, INotifyPropertyChanged, IDisposable
    {
        #region Constants
        public static readonly string DefaultApplicationName = "PgMoon";
        public const int DefaultUpcomingDays = 10;
        #endregion

        #region Init
        public ShareCalendarWindow(RegistryTools.Settings settings)
        {
            InitializeComponent();
            DataContext = this;

            Settings = settings;

            if (ResourceTools.ResourceLoader.LoadIcon("main.ico", string.Empty, out System.Windows.Media.ImageSource LoadedIcon))
                Icon = LoadedIcon;

            AddEvents = Settings.GetBool("AddEvents", false);
            ApplicationName = Settings.GetString("EventsApplicationName", DefaultApplicationName);
            SecretFileName = Settings.GetString("EventsSecretFileName", string.Empty);
            CalendarId = Settings.GetString("EventsCalendarId", string.Empty);
            UpcomingDays = (uint)Settings.GetInt("EventsUpcomingDays", DefaultUpcomingDays);
            WithPhaseName = Settings.GetBool("EventsWithPhaseName", true);
            WithMushroomFarming = Settings.GetBool("EventsWithMushroomFarming", true);
            WithMushroomFarmingComments = Settings.GetBool("EventsWithMushroomFarmingComments", false);
            WithRahuBoat = Settings.GetBool("EventsWithRahuBoat", true);
            WithDarkChapel = Settings.GetBool("EventsWithDarkChapel", true);
            WithFreeText = Settings.GetBool("EventsWithFreeText", false);
            FreeText = Settings.GetString("EventsFreeText", string.Empty);

            InitCalendarList();
            InitCredential();
            InitStatus();
        }

        public RegistryTools.Settings Settings { get; private set; }
        #endregion

        #region Properties
        public bool AddEvents
        {
            get { return AddEventsInternal; }
            set
            {
                if (AddEventsInternal != value)
                {
                    AddEventsInternal = value;
                    NotifyThisPropertyChanged();
                    UpdateStatus();
                }
            }
        }
        private bool AddEventsInternal;

        public string ApplicationName
        {
            get { return ApplicationNameInternal; }
            set
            {
                string NewName = value;
                if (NewName == null)
                    NewName = DefaultApplicationName;

                if (ApplicationNameInternal != NewName)
                {
                    ApplicationNameInternal = NewName;
                    NotifyThisPropertyChanged();
                    UpdateStatus();
                }
            }
        }
        private string ApplicationNameInternal = string.Empty;
        #endregion

        #region Calendar List
        private void InitCalendarList()
        {
            IsListingInternal = false;
            IsListingCancelableInternal = true;
            ListTimer = new Timer(new TimerCallback(ListTimerCallback));
        }

        public bool IsListing
        {
            get { return IsListingInternal; }
            set
            {
                if (IsListingInternal != value)
                {
                    IsListingInternal = value;
                    NotifyThisPropertyChanged();
                }
            }
        }
        private bool IsListingInternal;

        public bool IsListingCancelable
        {
            get { return IsListingCancelableInternal; }
            set
            {
                if (IsListingCancelableInternal != value)
                {
                    IsListingCancelableInternal = value;
                    NotifyThisPropertyChanged();
                }
            }
        }
        private bool IsListingCancelableInternal;

        public SharedCalendarEntry SelectedCalendarEntry
        {
            get { return SelectedCalendarEntryInternal; }
            set
            {
                if (SelectedCalendarEntryInternal != value)
                {
                    SelectedCalendarEntryInternal = value;
                    NotifyThisPropertyChanged();
                    UpdateStatus();
                }
            }
        }
        private SharedCalendarEntry SelectedCalendarEntryInternal = SharedCalendarEntry.None;
        private string CalendarId;

        public ObservableCollection<SharedCalendarEntry> SharedCalendarEntryList { get; } = new ObservableCollection<SharedCalendarEntry>();

        private void StartUpdatingCalendarList()
        {
            SharedCalendarEntryList.Clear();

            if (ApplicationName.Length == 0)
                return;

            if (CredentialToken == null)
                ObtainCredentialToken(false);

            if (CredentialToken == null)
                return;

            try
            {
                // Create Google Calendar API service.
                using CalendarService service = new CalendarService(new BaseClientService.Initializer() { HttpClientInitializer = CredentialToken, ApplicationName = ApplicationName });

                // Define parameters of requestCalendarList.
                CalendarListResource.ListRequest requestCalendarList = service.CalendarList.List();
                requestCalendarList.ShowDeleted = false;
                requestCalendarList.MaxResults = 10;

                // List events.
                ListTaskCancellation = new CancellationTokenSource();
                ListTask = requestCalendarList.ExecuteAsync(ListTaskCancellation.Token);
                IsListing = true;
                IsListingCancelable = true;
                ListTimer.Change(ListTimerStart, ListTimerInterval);
            }
            catch
            {
            }
        }

        private void ListTimerCallback(object? parameter)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new ListTimerHandler(OnListTimer));
        }

        private delegate void ListTimerHandler();
        private void OnListTimer()
        {
            if (ListTask == null || !ListTask.Wait(0, ListTaskCancellation.Token))
                return;

            ListTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            IsListing = false;
            IsListingCancelable = true;

            CalendarList? Result = ListTaskCancellation.IsCancellationRequested ? null : ListTask.Result;
            ListTask = null;

            ParseCalendarListResult(Result);
        }

        private void ParseCalendarListResult(CalendarList? result)
        {
            try
            {
                if (result != null)
                {
                    SharedCalendarEntry ReselectedEntry = SharedCalendarEntry.None;

                    foreach (CalendarListEntry Entry in result.Items)
                    {
                        string Id = Entry.Id;
                        string Name = Entry.Summary;
                        if (Id != null && Id.Length > 0 && Name != null && Name.Length > 0)
                        {
                            bool CanWrite = (Entry.AccessRole == "owner") || (Entry.AccessRole == "writer");
                            SharedCalendarEntry NewEntry = new SharedCalendarEntry(Id, Name, CanWrite);
                            SharedCalendarEntryList.Add(NewEntry);

                            if (SelectedCalendarEntry != SharedCalendarEntry.None && NewEntry.Id == SelectedCalendarEntry.Id)
                                ReselectedEntry = NewEntry;
                            else if (SelectedCalendarEntry == SharedCalendarEntry.None && NewEntry.Id == CalendarId)
                            {
                                CalendarId = string.Empty;
                                ReselectedEntry = NewEntry;
                            }
                        }
                    }

                    SelectedCalendarEntry = ReselectedEntry;
                }
            }
            catch
            {
            }

            UpdateStatus();
        }

        private void OnList(object sender, ExecutedRoutedEventArgs e)
        {
            StartUpdatingCalendarList();
        }

        private void OnCancelList(object sender, ExecutedRoutedEventArgs e)
        {
            ListTaskCancellation.Cancel();
            IsListingCancelable = false;
        }

        private Timer ListTimer = new Timer(new TimerCallback((object? state) => { }));
        private static readonly TimeSpan ListTimerStart = TimeSpan.FromSeconds(3);
        private static readonly TimeSpan ListTimerInterval = TimeSpan.FromSeconds(1);
        private Task<CalendarList>? ListTask;
        private CancellationTokenSource ListTaskCancellation = new CancellationTokenSource();
        #endregion

        #region Credentials
        private void InitCredential()
        {
            if (!AddEvents)
                return;

            CredentialToken = null;
            if (IsCredentialConfirmed)
                ObtainCredentialToken(true);
        }

        public string SecretFileName
        {
            get { return SecretFileNameInternal; }
            set
            {
                if (SecretFileNameInternal != value)
                {
                    SecretFileNameInternal = value;
                    NotifyThisPropertyChanged();
                }
            }
        }

        private static string CredentialFile
        {
            get
            {
                string ApplicationFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PgMoon");

                if (!Directory.Exists(ApplicationFolder))
                    Directory.CreateDirectory(ApplicationFolder);

                string CredentialFolder = Path.Combine(ApplicationFolder, ".credentials");

                if (!Directory.Exists(CredentialFolder))
                    Directory.CreateDirectory(CredentialFolder);

                return Path.Combine(CredentialFolder, "pgmoon.json");
            }
        }

        public bool IsCredentialConfirmed
        {
            get
            {
                bool Result;

                Result = SecretFileNameInternal.Length > 0 &&
                    ApplicationName.Length > 0 &&
                    File.Exists(SecretFileNameInternal) &&
                    (File.Exists(CredentialFile) || Directory.Exists(CredentialFile));

                return Result;
            }
        }

        private string SecretFileNameInternal = string.Empty;

        private void OnBrowse(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog Dlg = new OpenFileDialog();
            Dlg.FileName = SecretFileName;
            Dlg.Filter = "Any secret client file (*.json)|*.json";

            bool? Result = Dlg.ShowDialog();
            if (Result.HasValue && Result.Value)
            {
                if (File.Exists(CredentialFile))
                    File.Delete(CredentialFile);
                if (Directory.Exists(CredentialFile))
                    Directory.Delete(CredentialFile, true);

                SecretFileName = Dlg.FileName;
                ObtainCredentialToken(true);
                UpdateStatus();
            }
        }

        private void ObtainCredentialToken(bool updateCalendarList)
        {
            try
            {
                using (FileStream fs = new FileStream(SecretFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    ClientSecrets Secrets = GoogleClientSecrets.Load(fs).Secrets;
                    FileDataStore Store = new FileDataStore(CredentialFile, true);
                    string[] Scopes = { CalendarService.Scope.Calendar };

                    CredentialToken = GoogleWebAuthorizationBroker.AuthorizeAsync(Secrets, Scopes, "user", CancellationToken.None, Store).Result;

                    NotifyPropertyChanged(nameof(IsCredentialConfirmed));
                }
            }
            catch
            {
                return;
            }

            if (updateCalendarList)
                StartUpdatingCalendarList();
        }

        private UserCredential? CredentialToken;
        #endregion

        #region Status
        private void InitStatus()
        {
            UpdateStatus();
        }

        public ObservableCollection<string> StatusList { get; } = new ObservableCollection<string>();
        public bool IsEventActive { get; private set; }

        private void UpdateStatus()
        {
            StatusList.Clear();
            IsEventActive = false;

            if (!AddEvents)
            {
                StatusList.Add("Disabled.");
                return;
            }

            if (ApplicationName.Length == 0)
                StatusList.Add("Application Name is empty.");

            if (SecretFileName.Length == 0)
                StatusList.Add("No credential file selected.");
            else if (!File.Exists(SecretFileName))
                StatusList.Add("Credential file doesn't exist.");
            else if (!IsCredentialConfirmed)
                StatusList.Add("Credential not confirmed.");

            if (SelectedCalendarEntry == SharedCalendarEntry.None)
                if (SharedCalendarEntryList.Count == 0)
                    StatusList.Add("The list of calendar names is empty.");
                else
                    StatusList.Add("Calendar Name is empty.");
            else if (!SelectedCalendarEntry.CanWrite)
                StatusList.Add("The selected calendar can only be read.");

            if (!WithPhaseName && !WithMushroomFarming && !WithRahuBoat && !WithDarkChapel && !WithFreeText)
                StatusList.Add("No information shared.");

            if (StatusList.Count > 0)
                return;

            StatusList.Add("Active.");

            if (UpcomingDays == 0)
                StatusList.Add("With 0 upcoming days, only moon phase change is reported.");

            IsEventActive = true;
        }
        #endregion

        #region Event Information
        public bool WithPhaseName { get; set; }
        public bool WithMushroomFarming { get; set; }
        public bool WithMushroomFarmingComments { get; set; }
        public bool WithRahuBoat { get; set; }
        public bool WithDarkChapel { get; set; }
        public bool WithFreeText { get; set; }
        public string FreeText { get; set; } = string.Empty;

        public uint UpcomingDays
        {
            get { return UpcomingDaysInternal; }
            set
            {
                if (UpcomingDaysInternal != value)
                {
                    UpcomingDaysInternal = value;
                    NotifyThisPropertyChanged();
                    UpdateStatus();
                }
            }
        }
        private uint UpcomingDaysInternal;

        private void OnInfoChanged(object sender, RoutedEventArgs e)
        {
            UpdateStatus();
        }
        #endregion

        #region Save/Cancel
        private void OnSave(object sender, ExecutedRoutedEventArgs e)
        {
            Settings.SetBool("AddEvents", AddEvents);
            Settings.SetString("EventsApplicationName", ApplicationName);
            Settings.SetString("EventsSecretFileName", SecretFileName);
            Settings.SetString("EventsCalendarId", SelectedCalendarEntry.Id);
            Settings.SetInt("EventsUpcomingDays", (int)UpcomingDays);
            Settings.SetBool("EventsWithPhaseName", WithPhaseName);
            Settings.SetBool("EventsWithMushroomFarming", WithMushroomFarming);
            Settings.SetBool("EventsWithMushroomFarmingComments", WithMushroomFarmingComments);
            Settings.SetBool("EventsWithRahuBoat", WithRahuBoat);
            Settings.SetBool("EventsWithDarkChapel", WithDarkChapel);
            Settings.SetBool("EventsWithFreeText", WithFreeText);
            Settings.SetString("EventsFreeText", FreeText);

            DialogResult = true;
            Close();
        }

        private void OnCancel(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        #endregion

        #region Post Events
        public void PostSharedEvents(ICollection<MushroomInfo> mushroomInfoList)
        {
            if (mushroomInfoList == null)
                return;

            if (!IsEventActive)
                return;

            if (CredentialToken == null)
                ObtainCredentialToken(true);

            if (CredentialToken == null || SelectedCalendarEntry == SharedCalendarEntry.None)
                return;

            if (!ReadExistingEvents(out List<SharedCalendarEvent> ExistingEvents))
                return;

            DateTime Now = MainWindow.Now();
            DateTime NextEventTime = ExistingEvents.Count > 0 ? ExistingEvents[ExistingEvents.Count - 1].PhaseEndTime + TimeSpan.FromHours(1) : Now;
            DateTime MaxEventTime = Now + TimeSpan.FromDays(UpcomingDays);
            List<SharedCalendarEvent> MissingEvents = new List<SharedCalendarEvent>();

            for (;;)
            {
                int MoonMonth;
                MoonPhase MoonPhase;
                DateTime PhaseStartTime;
                DateTime PhaseEndTime;
                double ProgressToFullMoon;
                DateTime NextFullMoonTime;
                PhaseCalculator.DateTimeToMoonPhase(NextEventTime, out MoonMonth, out MoonPhase, out PhaseStartTime, out PhaseEndTime, out ProgressToFullMoon, out NextFullMoonTime);

                if (PhaseStartTime >= MaxEventTime)
                    break;

                PhaseStartTime = new DateTime(PhaseStartTime.Year, PhaseStartTime.Month, PhaseStartTime.Day, PhaseStartTime.Hour, 0, 0, PhaseStartTime.Kind);
                PhaseEndTime = new DateTime(PhaseEndTime.Year, PhaseEndTime.Month, PhaseEndTime.Day, PhaseEndTime.Hour, 0, 0, PhaseEndTime.Kind);

                SharedCalendarEvent NewEvent = new SharedCalendarEvent(MoonPhase, MoonMonth, PhaseStartTime, PhaseEndTime);
                MissingEvents.Add(NewEvent);

                NextEventTime = PhaseEndTime + TimeSpan.FromHours(1);
            }

            if (MissingEvents.Count > 0)
                WriteMissingEvents(MissingEvents, mushroomInfoList);
        }

        private bool ReadExistingEvents(out List<SharedCalendarEvent> existingEvents)
        {
            existingEvents = new List<SharedCalendarEvent>();

            try
            {
                // Create Google Calendar API service.
                using CalendarService service = new CalendarService(new BaseClientService.Initializer() { HttpClientInitializer = CredentialToken, ApplicationName = ApplicationName });

                // Define parameters of requestList.
                EventsResource.ListRequest requestList = service.Events.List(SelectedCalendarEntry.Id);
                requestList.TimeMin = DateTime.Now - TimeSpan.FromHours(1);
                requestList.ShowDeleted = false;
                requestList.SingleEvents = true;
                requestList.MaxResults = 100;
                requestList.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

                // List events.
                Events events = requestList.Execute();
                if (events.Items == null)
                    return false;

                foreach (var eventItem in events.Items)
                {
                    DateTime? StartDate = eventItem.Start.DateTime;
                    DateTime? EndDate = eventItem.End.DateTime;

                    if (SharedCalendarEvent.TryParse(StartDate, EndDate, out SharedCalendarEvent? NewEvent) && NewEvent != null)
                        existingEvents.Add(NewEvent);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }

            return false;
        }

        private void WriteMissingEvents(List<SharedCalendarEvent> eventList, ICollection<MushroomInfo> mushroomInfoList)
        {
            try
            {
                // Create Google Calendar API service.
                using CalendarService service = new CalendarService(new BaseClientService.Initializer() { HttpClientInitializer = CredentialToken, ApplicationName = ApplicationName });

                foreach (SharedCalendarEvent Event in eventList)
                {
                    string Description = string.Empty;

                    if (WithPhaseName)
                        Description += "This moon phase is " + Event.MoonPhase.Name;

                    if (WithMushroomFarming)
                    {
                        string MushroomList = string.Empty;

                        foreach (MushroomInfo Info in mushroomInfoList)
                            if (Event.MoonPhase == Info.RobustGrowthPhase1 || Event.MoonPhase == Info.RobustGrowthPhase2)
                            {
                                if (MushroomList.Length > 0)
                                    MushroomList += ", ";
                                MushroomList += Info.Name;

                                if (WithMushroomFarmingComments && Info.Comment != null && Info.Comment.Length > 0)
                                    MushroomList += " (" + Info.Comment + ")";
                            }

                        if (MushroomList.Length > 0)
                        {
                            if (Description.Length > 0)
                                Description += "\n\n";
                            Description += "Mushrooms growing robustly: " + MushroomList;
                        }
                    }

                    if (WithRahuBoat)
                    {
                        if (Description.Length > 0)
                            Description += "\n\n";
                        Description += "The boat at Rahu's dock is going to: " + Event.MoonPhase.RahuBoatDestination;
                    }

                    if (WithDarkChapel)
                    {
                        if (Description.Length > 0)
                            Description += "\n\n";
                        Description += "The entrance to the Dark Chapel is: " + Event.MoonPhase.DarkChapelTip;
                    }

                    if (WithFreeText && FreeText.Length > 0)
                    {
                        if (Description.Length > 0)
                            Description += "\n\n";
                        Description += FreeText;
                    }

                    Event body = new Event();
                    body.Summary = Event.MoonPhase.Name;
                    body.Start = new EventDateTime();
                    body.Start.DateTime = Event.PhaseStartTime;
                    body.End = new EventDateTime();
                    body.End.DateTime = Event.PhaseEndTime;
                    if (Description.Length > 0)
                        body.Description = Description;

                    EventsResource.InsertRequest requestInsert = service.Events.Insert(body, SelectedCalendarEntry.Id);
                    object result = requestInsert.Execute();
                    if (result == null)
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
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

        #region Implementation of IDisposable
        /// <summary>
        /// Called when an object should release its resources.
        /// </summary>
        /// <param name="isDisposing">Indicates if resources must be disposed now.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (!IsDisposed)
            {
                IsDisposed = true;

                if (isDisposing)
                    DisposeNow();
            }
        }

        /// <summary>
        /// Called when an object should release its resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ShareCalendarWindow"/> class.
        /// </summary>
        ~ShareCalendarWindow()
        {
            Dispose(false);
        }

        /// <summary>
        /// True after <see cref="Dispose(bool)"/> has been invoked.
        /// </summary>
        private bool IsDisposed;

        /// <summary>
        /// Disposes of every reference that must be cleaned up.
        /// </summary>
        private void DisposeNow()
        {
            using (ListTaskCancellation)
            {
            }

            using (ListTimer)
            {
            }
        }
        #endregion
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore SA1601 // Partial elements should be documented
}
