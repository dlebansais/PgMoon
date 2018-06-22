using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Win32;
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

namespace PgMoon
{
    public partial class ShareCalendarWindow : Window, INotifyPropertyChanged
    {
        #region Constants
        public static readonly string DefaultApplicationName = "PgMoon";
        public const int DefaultUpcomingDays = 10;
        #endregion

        #region Init
        public ShareCalendarWindow(TaskbarIconHost.IPluginSettings settings)
        {
            InitializeComponent();
            DataContext = this;

            Settings = settings;

            AddEvents = Settings.GetSettingBool("AddEvents", false);
            ApplicationName = Settings.GetSettingString("EventsApplicationName", DefaultApplicationName);
            SecretFileName = Settings.GetSettingString("EventsSecretFileName", null);
            CalendarId = Settings.GetSettingString("EventsCalendarId", null);
            UpcomingDays = (uint)Settings.GetSettingInt("EventsUpcomingDays", DefaultUpcomingDays);
            WithPhaseName = Settings.GetSettingBool("EventsWithPhaseName", true);
            WithMushroomFarming = Settings.GetSettingBool("EventsWithMushroomFarming", true);
            WithMushroomFarmingComments = Settings.GetSettingBool("EventsWithMushroomFarmingComments", false);
            WithRahuBoat = Settings.GetSettingBool("EventsWithRahuBoat", true);
            WithDarkChapel = Settings.GetSettingBool("EventsWithDarkChapel", true);
            WithFreeText = Settings.GetSettingBool("EventsWithFreeText", false);
            FreeText = Settings.GetSettingString("EventsFreeText", null);

            InitCalendarList();
            InitCredential();
            InitStatus();
        }

        public TaskbarIconHost.IPluginSettings Settings { get; private set; }
        #endregion

        #region Properties
        public bool AddEvents
        {
            get { return _AddEvents; }
            set
            {
                if (_AddEvents != value)
                {
                    _AddEvents = value;
                    NotifyThisPropertyChanged();
                    UpdateStatus();
                }
            }
        }
        private bool _AddEvents;

        public string ApplicationName
        {
            get { return _ApplicationName; }
            set
            {
                string NewName = value;
                if (NewName == null)
                    NewName = DefaultApplicationName;

                if (_ApplicationName != NewName)
                {
                    _ApplicationName = NewName;
                    NotifyThisPropertyChanged();
                    UpdateStatus();
                }
            }
        }
        private string _ApplicationName;
        #endregion

        #region Calendar List
        private void InitCalendarList()
        {
            _IsListing = false;
            _IsListingCancelable = true;
            _SelectedCalendarEntry = null;
            ListTimer = new Timer(new TimerCallback(ListTimerCallback));
        }

        public bool IsListing
        {
            get { return _IsListing; }
            set
            {
                if (_IsListing != value)
                {
                    _IsListing = value;
                    NotifyThisPropertyChanged();
                }
            }
        }
        private bool _IsListing;

        public bool IsListingCancelable
        {
            get { return _IsListingCancelable; }
            set
            {
                if (_IsListingCancelable != value)
                {
                    _IsListingCancelable = value;
                    NotifyThisPropertyChanged();
                }
            }
        }
        private bool _IsListingCancelable;

        public SharedCalendarEntry SelectedCalendarEntry
        {
            get { return _SelectedCalendarEntry; }
            set
            {
                if (_SelectedCalendarEntry != value)
                {
                    _SelectedCalendarEntry = value;
                    NotifyThisPropertyChanged();
                    UpdateStatus();
                }
            }
        }
        private SharedCalendarEntry _SelectedCalendarEntry;
        private string CalendarId;

        public ObservableCollection<SharedCalendarEntry> SharedCalendarEntryList { get; } = new ObservableCollection<SharedCalendarEntry>();

        private void StartUpdatingCalendarList()
        {
            SharedCalendarEntryList.Clear();

            if (ApplicationName == null || ApplicationName.Length == 0)
                return;

            if (CredentialToken == null)
                ObtainCredentialToken(false);

            if (CredentialToken == null)
                return;

            try
            {
                // Create Google Calendar API service.
                CalendarService service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = CredentialToken,
                    ApplicationName = ApplicationName,
                });

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

        private void ListTimerCallback(object Parameter)
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

            CalendarList Result = ListTaskCancellation.IsCancellationRequested ? null : ListTask.Result;
            ListTask = null;

            ParseCalendarListResult(Result);
        }

        private void ParseCalendarListResult(CalendarList Result)
        {
            try
            {
                if (Result != null)
                {
                    SharedCalendarEntry ReselectedEntry = null;

                    foreach (CalendarListEntry Entry in Result.Items)
                    {
                        string Id = Entry.Id;
                        string Name = Entry.Summary;
                        if (Id != null && Id.Length > 0 && Name != null && Name.Length > 0)
                        {
                            bool CanWrite = ((Entry.AccessRole == "owner") || (Entry.AccessRole == "writer"));
                            SharedCalendarEntry NewEntry = new SharedCalendarEntry(Id, Name, CanWrite);
                            SharedCalendarEntryList.Add(NewEntry);

                            if (SelectedCalendarEntry != null && NewEntry.Id == SelectedCalendarEntry.Id)
                                ReselectedEntry = NewEntry;
                            else if (SelectedCalendarEntry == null && NewEntry.Id == CalendarId)
                            {
                                CalendarId = null;
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

        private Timer ListTimer;
        private static readonly TimeSpan ListTimerStart = TimeSpan.FromSeconds(3);
        private static readonly TimeSpan ListTimerInterval = TimeSpan.FromSeconds(1);
        private Task<CalendarList> ListTask;
        private CancellationTokenSource ListTaskCancellation;
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
            get { return _SecretFileName; }
            set
            {
                if (_SecretFileName != value)
                {
                    _SecretFileName = value;
                    NotifyThisPropertyChanged();
                }
            }
        }

        private string CredentialFile
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

                Result = _SecretFileName != null && 
                    ApplicationName != null && 
                    ApplicationName.Length > 0 && 
                    File.Exists(_SecretFileName) && 
                    (File.Exists(CredentialFile) || Directory.Exists(CredentialFile));

                return Result;
            }
        }
        private string _SecretFileName;

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

        private void ObtainCredentialToken(bool UpdateCalendarList)
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

            if (UpdateCalendarList)
                StartUpdatingCalendarList();
        }

        private UserCredential CredentialToken;
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

            if (ApplicationName == null || ApplicationName.Length == 0)
                StatusList.Add("Application Name is empty.");

            if (SecretFileName == null || SecretFileName.Length == 0)
                StatusList.Add("No credential file selected.");
            else if (!File.Exists(SecretFileName))
                StatusList.Add("Credential file doesn't exist.");
            else if (!IsCredentialConfirmed)
                StatusList.Add("Credential not confirmed.");

            if (SelectedCalendarEntry == null)
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
        public string FreeText { get; set; }

        public uint UpcomingDays
        {
            get { return _UpcomingDays; }
            set
            {
                if (_UpcomingDays != value)
                {
                    _UpcomingDays = value;
                    NotifyThisPropertyChanged();
                    UpdateStatus();
                }
            }
        }
        private uint _UpcomingDays;

        private void OnInfoChanged(object sender, RoutedEventArgs e)
        {
            UpdateStatus();
        }
        #endregion

        #region Save/Cancel
        private void OnSave(object sender, ExecutedRoutedEventArgs e)
        {
            Settings.SetSettingBool("AddEvents", AddEvents);
            Settings.SetSettingString("EventsApplicationName", ApplicationName);
            Settings.SetSettingString("EventsSecretFileName", SecretFileName);
            Settings.SetSettingString("EventsCalendarId", (SelectedCalendarEntry != null) ? SelectedCalendarEntry.Id : null);
            Settings.SetSettingInt("EventsUpcomingDays", (int)UpcomingDays);
            Settings.SetSettingBool("EventsWithPhaseName", WithPhaseName);
            Settings.SetSettingBool("EventsWithMushroomFarming", WithMushroomFarming);
            Settings.SetSettingBool("EventsWithMushroomFarmingComments", WithMushroomFarmingComments);
            Settings.SetSettingBool("EventsWithRahuBoat", WithRahuBoat);
            Settings.SetSettingBool("EventsWithDarkChapel", WithDarkChapel);
            Settings.SetSettingBool("EventsWithFreeText", WithFreeText);
            Settings.SetSettingString("EventsFreeText", FreeText);

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
        public void PostSharedEvents(ICollection<MushroomInfo> MushroomInfoList)
        {
            if (!IsEventActive)
                return;

            if (CredentialToken == null)
                ObtainCredentialToken(true);

            if (CredentialToken == null || SelectedCalendarEntry == null)
                return;

            List<SharedCalendarEvent> ExistingEvents = ReadExistingEvents();
            if (ExistingEvents == null)
                return;

            DateTime Now = MainWindow.Now();
            DateTime NextEventTime = ExistingEvents.Count > 0 ? ExistingEvents[ExistingEvents.Count - 1].PhaseEndTime + TimeSpan.FromHours(1) : Now;
            DateTime MaxEventTime = Now + TimeSpan.FromDays(UpcomingDays);
            List<SharedCalendarEvent> MissingEvents = new List<SharedCalendarEvent>();

            for(;;)
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
                WriteMissingEvents(MissingEvents, MushroomInfoList);
        }

        private List<SharedCalendarEvent> ReadExistingEvents()
        {
            try
            {
                List<SharedCalendarEvent> EventList = new List<SharedCalendarEvent>();

                // Create Google Calendar API service.
                CalendarService service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = CredentialToken,
                    ApplicationName = ApplicationName,
                });

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
                    return null;

                foreach (var eventItem in events.Items)
                {
                    DateTime? StartDate = eventItem.Start.DateTime;
                    DateTime? EndDate = eventItem.End.DateTime;

                    SharedCalendarEvent NewEvent;
                    if (SharedCalendarEvent.TryParse(StartDate, EndDate, out NewEvent))
                        EventList.Add(NewEvent);
                }

                return EventList;
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }

            return null;
        }

        private void WriteMissingEvents(List<SharedCalendarEvent> EventList, ICollection<MushroomInfo> MushroomInfoList)
        {
            try
            {
                // Create Google Calendar API service.
                CalendarService service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = CredentialToken,
                    ApplicationName = ApplicationName,
                });

                foreach (SharedCalendarEvent Event in EventList)
                {
                    string Description = "";

                    if (WithPhaseName)
                        Description += "This moon phase is " + Event.MoonPhase.Name;

                    if (WithMushroomFarming)
                    {
                        string MushroomList = "";

                        foreach (MushroomInfo Info in MushroomInfoList)
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

                    if (WithFreeText && FreeText != null && FreeText.Length > 0)
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
