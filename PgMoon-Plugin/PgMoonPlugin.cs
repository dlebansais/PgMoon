namespace PgMoon;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;
using RegistryTools;
using ResourceTools;
using TaskbarIconHost;

/// <summary>
/// Represents a plugin that automatically unblock files downloaded from the Internet.
/// </summary>
public class PgMoonPlugin : IPluginClient, IDisposable
{
    #region Plugin
    /// <inheritdoc cref="IPluginClient.Name" />
    public string Name => "PgMoon";

    /// <inheritdoc cref="IPluginClient.PluginGuid" />
    public Guid PluginGuid => new("{AA25BA4F-9922-4018-B1B4-588A6B59CE62}");

    /// <summary>
    /// Gets the plugin assembly name.
    /// </summary>
    public static string AssemblyName => "PgMoon-Plugin";

    /// <inheritdoc cref="IPluginClient.RequireElevated" />
    public bool RequireElevated => false;

    /// <inheritdoc cref="IPluginClient.HasClickHandler" />
    public bool HasClickHandler => false;

    /// <inheritdoc cref="IPluginClient.Initialize(bool, Dispatcher, Settings, ILogger)" />
    public void Initialize(bool isElevated, Dispatcher dispatcher, Settings settings, ILogger logger)
    {
        IsElevated = isElevated;
        Dispatcher = dispatcher;
        Settings = settings;
        Logger = logger;

        MainPopup?.Dispose();
        MainPopup = new MainWindow(Settings, Logger);

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

        CommandListInternal.Add(new RoutedCommand());

        InitializeCommand("ShareCalendar",
                          isVisibleHandler: () => true,
                          isEnabledHandler: () => true,
                          isCheckedHandler: () => false,
                          commandHandler: OnSharedCalendar);
    }

    private void InitializeCommand(string header, Func<bool> isVisibleHandler, Func<bool> isEnabledHandler, Func<bool> isCheckedHandler, Action commandHandler)
    {
#if NETFRAMEWORK
        string LocalizedText = Properties.Resources.ResourceManager.GetString(header, CultureInfo.CurrentCulture);
#else
        string LocalizedText = Properties.Resources.ResourceManager.GetString(header, CultureInfo.CurrentCulture)!;
#endif
        ICommand Command = new RoutedUICommand(LocalizedText, header, GetType());

        CommandListInternal.Add(Command);
        MenuHeaderTable.Add(Command, LocalizedText);
        MenuIsVisibleTable.Add(Command, isVisibleHandler);
        MenuIsEnabledTable.Add(Command, isEnabledHandler);
        MenuIsCheckedTable.Add(Command, isCheckedHandler);
        MenuHandlerTable.Add(Command, commandHandler);
    }

    /// <inheritdoc cref="IPluginClient.CommandList" />
    public IReadOnlyCollection<ICommand?> CommandList => CommandListInternal.AsReadOnly();

    private readonly List<ICommand?> CommandListInternal = [];

    /// <inheritdoc cref="IPluginClient.GetIsMenuChanged(bool)" />
    public bool GetIsMenuChanged(bool beforeMenuOpening)
    {
        bool Result = IsMenuChanged;
        IsMenuChanged = false;

        return Result;
    }

    /// <inheritdoc cref="IPluginClient.GetMenuHeader(ICommand)" />
    public string GetMenuHeader(ICommand command) => MenuHeaderTable[command];

    /// <inheritdoc cref="IPluginClient.GetMenuIsVisible(ICommand)" />
    public bool GetMenuIsVisible(ICommand command) => MenuIsVisibleTable[command]();

    /// <inheritdoc cref="IPluginClient.GetMenuIsEnabled(ICommand)" />
    public bool GetMenuIsEnabled(ICommand command) => MenuIsEnabledTable[command]();

    /// <inheritdoc cref="IPluginClient.GetMenuIsChecked(ICommand)" />
    public bool GetMenuIsChecked(ICommand command) => MenuIsCheckedTable[command]();

    /// <inheritdoc cref="IPluginClient.GetMenuIcon(ICommand)" />
    public Bitmap? GetMenuIcon(ICommand command) => null;

    /// <inheritdoc cref="IPluginClient.OnMenuOpening" />
    public void OnMenuOpening()
    {
    }

    /// <inheritdoc cref="IPluginClient.OnExecuteCommand(ICommand)" />
    public void OnExecuteCommand(ICommand command) => MenuHandlerTable[command]();

    /// <inheritdoc cref="IPluginClient.GetIsIconChanged" />
    public bool GetIsIconChanged()
    {
        bool Result = IsIconChanged;
        IsIconChanged = false;

        return Result;
    }

    /// <inheritdoc cref="IPluginClient.Icon" />
    public Icon Icon
    {
        get
        {
            _ = ResourceLoader.LoadIcon("Taskbar.ico", string.Empty, out Icon Result);
            return Result;
        }
    }

    /// <inheritdoc cref="IPluginClient.SelectionBitmap" />
    public Bitmap SelectionBitmap
    {
        get
        {
            _ = ResourceLoader.LoadBitmap("PgMoon.png", string.Empty, out Bitmap Result);
            return Result;
        }
    }

    /// <inheritdoc cref="IPluginClient.OnIconClicked" />
    public void OnIconClicked() => MainPopup?.IconClicked();

    /// <inheritdoc cref="IPluginClient.GetIsToolTipChanged" />
    public bool GetIsToolTipChanged()
    {
        bool Result = false;

        if (MainPopup is not null)
        {
            Result = MainPopup.IsToolTipChanged || MainPopup.GetIsToolTipChanged();
            MainPopup.IsToolTipChanged = false;
        }

        return Result;
    }

    /// <inheritdoc cref="IPluginClient.ToolTip" />
    public string ToolTip
    {
        get
        {
            string Result = string.Empty;

            if (MainPopup is not null)
            {
                Result = $"{PhaseCalculator.MoonPhase.Name}\r\n{MainPopup.TimeToNextPhaseText}";

                if (MainPopup.ShowRahuBoat)
                    Result += $"\r\n{CalendarEntry.RahuBoatDestinationShortText}: {PhaseCalculator.MoonPhase.RahuBoatDestination}";

                Result += $"\r\n{CalendarEntry.PortToCircleShortText}: {PhaseCalculator.MoonPhase.FastPortMushroomShortText}";
            }

            return Result;
        }
    }

    /// <inheritdoc cref="IPluginClient.OnActivated" />
    public void OnActivated()
    {
    }

    /// <inheritdoc cref="IPluginClient.OnDeactivated" />
    public void OnDeactivated() => MainPopup?.OnDeactivated();

    /// <inheritdoc cref="IPluginClient.CanClose(bool)" />
    public bool CanClose(bool canClose) => true;

    /// <inheritdoc cref="IPluginClient.BeginClose" />
    public void BeginClose()
    {
        if (MainPopup is not null)
        {
            MainPopup.IsOpen = false;
            MainPopup.Dispose();
            MainPopup = null;
        }
    }

    /// <inheritdoc cref="IPluginClient.Icon" />
    public bool IsClosed => true;

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
    public ILogger Logger { get; private set; } = null!;

    /*
    private void AddLog(string message)
    {
        LoggerMessage.Define(LogLevel.Information, 0, message)(Logger, null);
    }
    */

    /// <summary>
    /// Gets the main window popup.
    /// </summary>
    public MainWindow? MainPopup { get; private set; }

    private readonly Dictionary<ICommand, string> MenuHeaderTable = [];
    private readonly Dictionary<ICommand, Func<bool>> MenuIsVisibleTable = [];
    private readonly Dictionary<ICommand, Func<bool>> MenuIsEnabledTable = [];
    private readonly Dictionary<ICommand, Func<bool>> MenuIsCheckedTable = [];
    private readonly Dictionary<ICommand, Action> MenuHandlerTable = [];
    private bool IsIconChanged;
    private bool IsMenuChanged;
    #endregion

    #region Command Handlers
    private void OnShowCalendar()
    {
        if (MainPopup is not null)
        {
            MainPopup.ShowCalendar = !MainPopup.ShowCalendar;
            IsMenuChanged = true;
        }
    }

    private void OnShowMushroomFarming()
    {
        if (MainPopup is not null)
        {
            MainPopup.ShowMushroomFarming = !MainPopup.ShowMushroomFarming;
            IsMenuChanged = true;
        }
    }

    private void OnShowRahuBoat()
    {
        if (MainPopup is not null)
        {
            MainPopup.ShowRahuBoat = !MainPopup.ShowRahuBoat;
            IsMenuChanged = true;
        }
    }

    private void OnShowDarkChapel()
    {
        if (MainPopup is not null)
        {
            MainPopup.ShowDarkChapel = !MainPopup.ShowDarkChapel;
            IsMenuChanged = true;
        }
    }

    private void OnSharedCalendar() => MainPopup?.OnSharedCalendar();
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

    /// <inheritdoc cref="IDisposable.Dispose" />
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
    private void DisposeNow() => MainPopup?.Dispose();
    #endregion
}
