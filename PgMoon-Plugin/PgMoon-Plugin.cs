using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;

namespace PgMoon
{
    public class PgMoonPlugin : MarshalByRefObject, TaskbarIconHost.IPluginClient
    {
        #region Plugin
        public string Name
        {
            get { return "PgMoon"; }
        }

        public Guid Guid
        {
            get { return new Guid("{AA25BA4F-9922-4018-B1B4-588A6B59CE62}"); }
        }

        public bool RequireElevated
        {
            get { return false; }
        }

        public void Initialize(bool isElevated, Dispatcher dispatcher, TaskbarIconHost.IPluginSettings settings, TaskbarIconHost.IPluginLogger logger)
        {
            IsElevated = isElevated;
            Dispatcher = dispatcher;
            Settings = settings;
            Logger = logger;

            MainPopup = new MainWindow(Settings);

            InitializeCommand("Show Calendar",
                              isVisibleHandler: () => true,
                              isEnabledHandler: () => true,
                              isCheckedHandler: () => MainPopup.ShowCalendar,
                              commandHandler: OnShowCalendar);

            InitializeCommand("Show Mushrooms",
                              isVisibleHandler: () => true,
                              isEnabledHandler: () => true,
                              isCheckedHandler: () => MainPopup.ShowMushroomFarming,
                              commandHandler: OnShowMushroomFarming);

            InitializeCommand("Show Rahu Boat",
                              isVisibleHandler: () => true,
                              isEnabledHandler: () => true,
                              isCheckedHandler: () => MainPopup.ShowRahuBoat,
                              commandHandler: OnShowRahuBoat);

            InitializeCommand("Show Dark Chapel",
                              isVisibleHandler: () => true,
                              isEnabledHandler: () => true,
                              isCheckedHandler: () => MainPopup.ShowDarkChapel,
                              commandHandler: OnShowDarkChapel);

            InitializeCommand("Share the calendar...",
                              isVisibleHandler: () => true,
                              isEnabledHandler: () => true,
                              isCheckedHandler: () => true,
                              commandHandler: OnSharedCalendar);
        }

        private void InitializeCommand(string header, Func<bool> isVisibleHandler, Func<bool> isEnabledHandler, Func<bool> isCheckedHandler, Action commandHandler)
        {
            ICommand Command = new RoutedUICommand();
            CommandList.Add(Command);
            MenuHeaderTable.Add(Command, header);
            MenuIsVisibleTable.Add(Command, isVisibleHandler);
            MenuIsEnabledTable.Add(Command, isEnabledHandler);
            MenuIsCheckedTable.Add(Command, isCheckedHandler);
            MenuHandlerTable.Add(Command, commandHandler);
        }

        public List<ICommand> CommandList { get; private set; } = new List<ICommand>();

        public bool GetIsMenuChanged()
        {
            bool Result = IsMenuChanged;
            IsMenuChanged = false;

            return Result;
        }

        public string GetMenuHeader(ICommand Command)
        {
            return MenuHeaderTable[Command];
        }

        public bool GetMenuIsVisible(ICommand Command)
        {
            return MenuIsVisibleTable[Command]();
        }

        public bool GetMenuIsEnabled(ICommand Command)
        {
            return MenuIsEnabledTable[Command]();
        }

        public bool GetMenuIsChecked(ICommand Command)
        {
            return MenuIsCheckedTable[Command]();
        }

        public Bitmap GetMenuIcon(ICommand Command)
        {
            return null;
        }

        public void OnMenuOpening()
        {
        }

        public void ExecuteCommandHandler(ICommand Command)
        {
            MenuHandlerTable[Command]();
        }

        public bool GetIsIconChanged()
        {
            return false;
        }

        public Icon Icon { get { return LoadEmbeddedResource<Icon>("Taskbar.ico"); } }

        public bool GetIsToolTipChanged()
        {
            bool Result = IsToolTipChanged || MainPopup.GetIsToolTipChanged();
            IsToolTipChanged = false;

            return Result;
        }

        public string ToolTip
        {
            get
            {
                string Result = PhaseCalculator.MoonPhase.Name + "\r\n" + MainPopup.TimeToNextPhaseText;
                if (MainPopup.ShowRahuBoat)
                    Result += "\r\n" + CalendarEntry.RahuBoatDestinationShortText + ": " + PhaseCalculator.MoonPhase.RahuBoatDestination;
                Result += "\r\n" + CalendarEntry.PortToCircleShortText + ": " + PhaseCalculator.MoonPhase.FastPortMushroomShortText;

                return Result;
            }
        }

        public bool CanClose(bool canClose)
        {
            return true;
        }

        public void BeginClose()
        {
            MainPopup.IsOpen = false;

            using (MainWindow Popup = MainPopup)
            {
                MainPopup = null;
            }
        }

        public bool IsClosed
        {
            get { return true; }
        }

        public bool IsElevated { get; private set; }
        public Dispatcher Dispatcher { get; private set; }
        public TaskbarIconHost.IPluginSettings Settings { get; private set; }
        public TaskbarIconHost.IPluginLogger Logger { get; private set; }

        private T LoadEmbeddedResource<T>(string resourceName)
        {
            // Loads an "Embedded Resource" of type T (ex: Bitmap for a PNG file).
            foreach (string ResourceName in Assembly.GetExecutingAssembly().GetManifestResourceNames())
                if (ResourceName.EndsWith(resourceName))
                {
                    using (Stream rs = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName))
                    {
                        T Result = (T)Activator.CreateInstance(typeof(T), rs);
                        Logger.AddLog($"Resource {resourceName} loaded");

                        return Result;
                    }
                }

            Logger.AddLog($"Resource {resourceName} not found");
            return default(T);
        }

        public MainWindow MainPopup { get; private set; }
        private Dictionary<ICommand, string> MenuHeaderTable = new Dictionary<ICommand, string>();
        private Dictionary<ICommand, Func<bool>> MenuIsVisibleTable = new Dictionary<ICommand, Func<bool>>();
        private Dictionary<ICommand, Func<bool>> MenuIsEnabledTable = new Dictionary<ICommand, Func<bool>>();
        private Dictionary<ICommand, Func<bool>> MenuIsCheckedTable = new Dictionary<ICommand, Func<bool>>();
        private Dictionary<ICommand, Action> MenuHandlerTable = new Dictionary<ICommand, Action>();
        private bool IsMenuChanged;
        private bool IsToolTipChanged;
        #endregion

        #region Command Handlers
        private void OnShowCalendar()
        {
            MainPopup.ShowCalendar = !MainPopup.ShowCalendar;
            IsMenuChanged = true;
        }

        private void OnShowMushroomFarming()
        {
            MainPopup.ShowMushroomFarming = !MainPopup.ShowMushroomFarming;
            IsMenuChanged = true;
        }

        private void OnShowRahuBoat()
        {
            MainPopup.ShowRahuBoat = !MainPopup.ShowRahuBoat;
            IsMenuChanged = true;
        }

        private void OnShowDarkChapel()
        {
            MainPopup.ShowDarkChapel = !MainPopup.ShowDarkChapel;
            IsMenuChanged = true;
        }

        private void OnSharedCalendar()
        {
            MainPopup.OnSharedCalendar();
        }
        
        #endregion
    }
}
