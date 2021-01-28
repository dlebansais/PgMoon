namespace PgMoon
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Input;
    using System.Windows.Threading;
    using RegistryTools;
    using ResourceTools;
    using TaskbarIconHost;
    using Tracing;

    /// <summary>
    /// Represents a plugin that automatically unblock files downloaded from the Internet.
    /// </summary>
    public class PgMoonPlugin : IPluginClient, IDisposable
    {
        #region Plugin
        /// <summary>
        /// Gets the plugin name.
        /// </summary>
        public string Name
        {
            get { return "PgMoon"; }
        }

        /// <summary>
        /// Gets the plugin unique ID.
        /// </summary>
        public Guid Guid
        {
            get { return new Guid("{AA25BA4F-9922-4018-B1B4-588A6B59CE62}"); }
        }

        /// <summary>
        /// Gets the plugin assembly name.
        /// </summary>
        public string AssemblyName { get; } = "PgMoon-Plugin";

        /// <summary>
        ///  Gets a value indicating whether the plugin require elevated (administrator) mode to operate.
        /// </summary>
        public bool RequireElevated
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the plugin want to handle clicks on the taskbar icon.
        /// </summary>
        public bool HasClickHandler
        {
            get { return false; }
        }

        /// <summary>
        /// Called once at startup, to initialize the plugin.
        /// </summary>
        /// <param name="isElevated">True if the caller is executing in administrator mode.</param>
        /// <param name="dispatcher">A dispatcher that can be used to synchronize with the UI.</param>
        /// <param name="settings">An interface to read and write settings in the registry.</param>
        /// <param name="logger">An interface to log events asynchronously.</param>
        public void Initialize(bool isElevated, Dispatcher dispatcher, Settings settings, ITracer logger)
        {
            IsElevated = isElevated;
            Dispatcher = dispatcher;
            Settings = settings;
            Logger = logger;

            MainPopup = new MainWindow(Settings);

            InitializeCommand("ShowCalendar",
                              isVisibleHandler: () => true,
                              isEnabledHandler: () => true,
                              isCheckedHandler: () => MainPopup.ShowCalendar,
                              commandHandler: OnShowCalendar);

            InitializeCommand("ShowMushrooms",
                              isVisibleHandler: () => true,
                              isEnabledHandler: () => true,
                              isCheckedHandler: () => MainPopup.ShowMushroomFarming,
                              commandHandler: OnShowMushroomFarming);

            InitializeCommand("ShowRahuBoat",
                              isVisibleHandler: () => true,
                              isEnabledHandler: () => true,
                              isCheckedHandler: () => MainPopup.ShowRahuBoat,
                              commandHandler: OnShowRahuBoat);

            InitializeCommand("ShowDarkChapel",
                              isVisibleHandler: () => true,
                              isEnabledHandler: () => true,
                              isCheckedHandler: () => MainPopup.ShowDarkChapel,
                              commandHandler: OnShowDarkChapel);

            CommandList.Add(new RoutedCommand());

            InitializeCommand("ShareCalendar",
                              isVisibleHandler: () => true,
                              isEnabledHandler: () => true,
                              isCheckedHandler: () => false,
                              commandHandler: OnSharedCalendar);
        }

        private void InitializeCommand(string header, Func<bool> isVisibleHandler, Func<bool> isEnabledHandler, Func<bool> isCheckedHandler, Action commandHandler)
        {
            string LocalizedText = Properties.Resources.ResourceManager.GetString(header, CultureInfo.CurrentCulture)!;
            ICommand Command = new RoutedUICommand(LocalizedText, header, GetType());

            CommandList.Add(Command);
            MenuHeaderTable.Add(Command, LocalizedText);
            MenuIsVisibleTable.Add(Command, isVisibleHandler);
            MenuIsEnabledTable.Add(Command, isEnabledHandler);
            MenuIsCheckedTable.Add(Command, isCheckedHandler);
            MenuHandlerTable.Add(Command, commandHandler);
        }

        /// <summary>
        /// Gets the list of commands that the plugin can receive when an item is clicked in the context menu.
        /// </summary>
        public List<ICommand> CommandList { get; private set; } = new List<ICommand>();

        /// <summary>
        /// Reads a flag indicating if the state of a menu item has changed. The flag should be reset upon return until another change occurs.
        /// </summary>
        /// <param name="beforeMenuOpening">True if this function is called right before the context menu is opened by the user; otherwise, false.</param>
        /// <returns>True if a menu item state has changed since the last call; otherwise, false.</returns>
        public bool GetIsMenuChanged(bool beforeMenuOpening)
        {
            bool Result = IsMenuChanged;
            IsMenuChanged = false;

            return Result;
        }

        /// <summary>
        /// Reads the text of a menu item associated to command.
        /// </summary>
        /// <param name="command">The command associated to the menu item.</param>
        /// <returns>The menu text.</returns>
        public string GetMenuHeader(ICommand command)
        {
            return MenuHeaderTable[command];
        }

        /// <summary>
        /// Reads the state of a menu item associated to command.
        /// </summary>
        /// <param name="command">The command associated to the menu item.</param>
        /// <returns>True if the menu item should be visible to the user, false if it should be hidden.</returns>
        public bool GetMenuIsVisible(ICommand command)
        {
            return MenuIsVisibleTable[command]();
        }

        /// <summary>
        /// Reads the state of a menu item associated to command.
        /// </summary>
        /// <param name="command">The command associated to the menu item.</param>
        /// <returns>True if the menu item should appear enabled, false if it should be disabled.</returns>
        public bool GetMenuIsEnabled(ICommand command)
        {
            return MenuIsEnabledTable[command]();
        }

        /// <summary>
        /// Reads the state of a menu item associated to command.
        /// </summary>
        /// <param name="command">The command associated to the menu item.</param>
        /// <returns>True if the menu item is checked, false otherwise.</returns>
        public bool GetMenuIsChecked(ICommand command)
        {
            return MenuIsCheckedTable[command]();
        }

        /// <summary>
        /// Reads the icon of a menu item associated to command.
        /// </summary>
        /// <param name="command">The command associated to the menu item.</param>
        /// <returns>The icon to display with the menu text, null if none.</returns>
        public Bitmap? GetMenuIcon(ICommand command)
        {
            return null;
        }

        /// <summary>
        /// This method is called before the menu is displayed, but after changes in the menu have been evaluated.
        /// </summary>
        public void OnMenuOpening()
        {
        }

        /// <summary>
        /// Requests for command to be executed.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        public void OnExecuteCommand(ICommand command)
        {
            MenuHandlerTable[command]();
        }

        /// <summary>
        /// Reads a flag indicating if the plugin icon, that might reflect the state of the plugin, has changed.
        /// </summary>
        /// <returns>True if the icon has changed since the last call, false otherwise.</returns>
        public bool GetIsIconChanged()
        {
            bool Result = IsIconChanged;
            IsIconChanged = false;

            return Result;
        }

        /// <summary>
        /// Gets the icon displayed in the taskbar.
        /// </summary>
        public Icon Icon
        {
            get
            {
                ResourceLoader.LoadIcon("Taskbar.ico", string.Empty, out Icon Result);
                return Result;
            }
        }

        /// <summary>
        /// Gets the bitmap displayed in the preferred plugin menu.
        /// </summary>
        public Bitmap SelectionBitmap
        {
            get
            {
                ResourceLoader.LoadBitmap("PgMoon.png", string.Empty, out Bitmap Result);
                return Result;
            }
        }

        /// <summary>
        /// Requests for the main plugin operation to be executed.
        /// </summary>
        public void OnIconClicked()
        {
            MainPopup?.IconClicked();
        }

        /// <summary>
        /// Reads a flag indicating if the plugin tooltip, that might reflect the state of the plugin, has changed.
        /// </summary>
        /// <returns>True if the tooltip has changed since the last call, false otherwise.</returns>
        public bool GetIsToolTipChanged()
        {
            bool Result = false;

            if (MainPopup != null)
            {
                Result = MainPopup.IsToolTipChanged || MainPopup.GetIsToolTipChanged();
                MainPopup.IsToolTipChanged = false;
            }

            return Result;
        }

        /// <summary>
        /// Gets the free text that indicate the state of the plugin.
        /// </summary>
        public string ToolTip
        {
            get
            {
                string Result = string.Empty;

                if (MainPopup != null)
                {
                    Result = PhaseCalculator.MoonPhase.Name + "\r\n" + MainPopup.TimeToNextPhaseText;
                    if (MainPopup.ShowRahuBoat)
                        Result += "\r\n" + CalendarEntry.RahuBoatDestinationShortText + ": " + PhaseCalculator.MoonPhase.RahuBoatDestination;
                    Result += "\r\n" + CalendarEntry.PortToCircleShortText + ": " + PhaseCalculator.MoonPhase.FastPortMushroomShortText;
                }

                return Result;
            }
        }

        /// <summary>
        /// Called when the taskbar is getting the application focus.
        /// </summary>
        public void OnActivated()
        {
        }

        /// <summary>
        /// Called when the taskbar is loosing the application focus.
        /// </summary>
        public void OnDeactivated()
        {
            MainPopup?.OnDeactivated();
        }

        /// <summary>
        /// Requests to close and terminate a plugin.
        /// </summary>
        /// <param name="canClose">True if no plugin called before this one has returned false, false if one of them has.</param>
        /// <returns>True if the plugin can be safely terminated, false if the request is denied.</returns>
        public bool CanClose(bool canClose)
        {
            return true;
        }

        /// <summary>
        /// Requests to begin closing the plugin.
        /// </summary>
        public void BeginClose()
        {
            if (MainPopup != null)
            {
                MainPopup.IsOpen = false;

                using (MainWindow Popup = MainPopup)
                {
                    MainPopup = null;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the plugin is closed.
        /// </summary>
        public bool IsClosed
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether the caller is executing in administrator mode.
        /// </summary>
        public bool IsElevated { get; private set; }

        /// <summary>
        /// Gets a dispatcher that can be used to synchronize with the UI.
        /// </summary>
        public Dispatcher Dispatcher { get; private set; } = null!;

        /// <summary>
        /// Gets an interface to read and write settings in the registry.
        /// </summary>
        public Settings Settings { get; private set; } = null!;

        /// <summary>
        /// Gets an interface to log events asynchronously.
        /// </summary>
        public ITracer Logger { get; private set; } = null!;

        private void AddLog(string message)
        {
            Logger.Write(Category.Information, message);
        }

        /// <summary>
        /// Gets the main window popup.
        /// </summary>
        public MainWindow? MainPopup { get; private set; }

        private Dictionary<ICommand, string> MenuHeaderTable = new Dictionary<ICommand, string>();
        private Dictionary<ICommand, Func<bool>> MenuIsVisibleTable = new Dictionary<ICommand, Func<bool>>();
        private Dictionary<ICommand, Func<bool>> MenuIsEnabledTable = new Dictionary<ICommand, Func<bool>>();
        private Dictionary<ICommand, Func<bool>> MenuIsCheckedTable = new Dictionary<ICommand, Func<bool>>();
        private Dictionary<ICommand, Action> MenuHandlerTable = new Dictionary<ICommand, Action>();
        private bool IsIconChanged;
        private bool IsMenuChanged;
        #endregion

        #region Command Handlers
        private void OnShowCalendar()
        {
            if (MainPopup != null)
            {
                MainPopup.ShowCalendar = !MainPopup.ShowCalendar;
                IsMenuChanged = true;
            }
        }

        private void OnShowMushroomFarming()
        {
            if (MainPopup != null)
            {
                MainPopup.ShowMushroomFarming = !MainPopup.ShowMushroomFarming;
                IsMenuChanged = true;
            }
        }

        private void OnShowRahuBoat()
        {
            if (MainPopup != null)
            {
                MainPopup.ShowRahuBoat = !MainPopup.ShowRahuBoat;
                IsMenuChanged = true;
            }
        }

        private void OnShowDarkChapel()
        {
            if (MainPopup != null)
            {
                MainPopup.ShowDarkChapel = !MainPopup.ShowDarkChapel;
                IsMenuChanged = true;
            }
        }

        private void OnSharedCalendar()
        {
            MainPopup?.OnSharedCalendar();
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
        /// Finalizes an instance of the <see cref="PgMoonPlugin"/> class.
        /// </summary>
        ~PgMoonPlugin()
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
            using (Settings)
            {
            }
        }
        #endregion
    }
}
