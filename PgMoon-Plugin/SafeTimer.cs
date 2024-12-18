﻿namespace PgMoon;

using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;

/// <summary>
/// Execute an action regularly at a given time interval, with safeguards.
/// </summary>
public class SafeTimer : IDisposable
{
    #region Init
    /// <summary>
    /// Creates a new instance of the <see cref="SafeTimer"/> class.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="timeInterval">The time interval.</param>
    /// <param name="logger">an interface to log events asynchronously.</param>
    public static SafeTimer Create(Action action, TimeSpan timeInterval, ILogger logger) => new(action, timeInterval, logger);

    /// <summary>
    /// Initializes a new instance of the <see cref="SafeTimer"/> class.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="timeInterval">The time interval.</param>
    /// <param name="logger">an interface to log events asynchronously.</param>
    protected SafeTimer(Action action, TimeSpan timeInterval, ILogger logger)
    {
        Action = action;
        TimeInterval = timeInterval;
        Logger = logger;

        UpdateTimer = new Timer(new TimerCallback(UpdateTimerCallback));
        FullRestartTimer = new Timer(new TimerCallback(FullRestartTimerCallback));
        UpdateWatch = new Stopwatch();
        UpdateWatch.Start();

        Action();

        _ = UpdateTimer.Change(TimeInterval, TimeInterval);
        _ = FullRestartTimer.Change(FullRestartInterval, Timeout.InfiniteTimeSpan);
    }
    #endregion

    #region Cleanup
    /// <summary>
    /// Destroys an instance of the <see cref="SafeTimer"/> class.
    /// </summary>
    /// <param name="instance">The instance to destroy.</param>
    public static void Destroy(ref SafeTimer? instance) => instance = null;
    #endregion

    #region Properties
    /// <summary>
    /// Gets the action to execute.
    /// </summary>
    public Action Action { get; }

    /// <summary>
    /// Gets the time interval.
    /// </summary>
    public TimeSpan TimeInterval { get; }

    /// <summary>
    /// Gets an interface to log events asynchronously.
    /// </summary>
    public ILogger Logger { get; }
    #endregion

    #region Client Interface
    /// <summary>
    /// To call from the action callback.
    /// </summary>
    public void NotifyCallbackCalled()
    {
        int LastTimerDispatcherCount = Interlocked.Decrement(ref TimerDispatcherCount);
        UpdateWatch.Restart();

        AddLog($"Watch restarted, Elapsed = {LastTotalElapsed}, pending count = {LastTimerDispatcherCount}");
    }
    #endregion

    #region Implementation
    private void UpdateTimerCallback(object? parameter)
    {
        // Protection against reentering too many times after a sleep/wake up.
        // There must be at most two pending calls to OnUpdate in the dispatcher.
        int NewTimerDispatcherCount = Interlocked.Increment(ref TimerDispatcherCount);
        if (NewTimerDispatcherCount > 2)
        {
            _ = Interlocked.Decrement(ref TimerDispatcherCount);
            return;
        }

        // For debug purpose.
        LastTotalElapsed = Math.Round(UpdateWatch.Elapsed.TotalSeconds, 0);

        _ = System.Windows.Application.Current.Dispatcher.BeginInvoke(Action);
    }

    private void FullRestartTimerCallback(object? parameter)
    {
        if (UpdateTimer is not null)
        {
            AddLog("Restarting the timer");

            // Restart the update timer from scratch.
            _ = UpdateTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            UpdateTimer.Dispose();
            UpdateTimer = new Timer(new TimerCallback(UpdateTimerCallback));

            _ = UpdateTimer.Change(TimeInterval, TimeInterval);

            AddLog("Timer restarted");
        }
        else
        {
            AddLog("No timer to restart");
        }

        _ = FullRestartTimer?.Change(FullRestartInterval, Timeout.InfiniteTimeSpan);
        AddLog($"Next check scheduled at {DateTime.UtcNow + FullRestartInterval}");
    }

    private void AddLog(string message) => LoggerMessage.Define(LogLevel.Information, 0, message)(Logger, null);

    private readonly TimeSpan FullRestartInterval = TimeSpan.FromHours(1);
    private Timer UpdateTimer;
    private readonly Timer FullRestartTimer;
    private readonly Stopwatch UpdateWatch;
    private int TimerDispatcherCount = 1;
    private double LastTotalElapsed = double.NaN;
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
    /// Finalizes an instance of the <see cref="SafeTimer"/> class.
    /// </summary>
    ~SafeTimer()
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
        using (FullRestartTimer)
        {
        }

        using (UpdateTimer)
        {
        }
    }
    #endregion
}
