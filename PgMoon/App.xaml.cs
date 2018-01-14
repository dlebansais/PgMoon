using Microsoft.Win32;
using System;
using System.Threading;
using System.Windows;
using TaskbarTools;

namespace PgMoon
{
    public partial class App : Application, IDisposable
    {
        #region Init
        static App()
        {
            InitSettings();
        }

        public App()
        {
            // Ensure only one instance is running at a time.
            try
            {
                bool createdNew;
                InstanceEvent = new EventWaitHandle(false, EventResetMode.ManualReset, "{AA25BA4F-9922-4018-B1B4-588A6B59CE62}", out createdNew);
                if (!createdNew)
                {
                    InstanceEvent.Close();
                    InstanceEvent = null;
                    Shutdown();
                    return;
                }
            }
            catch
            {
                Shutdown();
                return;
            }

            Taskbar.UpdateLocation();
            Startup += OnStartup;
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        public MainWindow MainPopup { get; private set; }
        private EventWaitHandle InstanceEvent;
        #endregion

        #region Events
        private void OnStartup(object sender, StartupEventArgs e)
        {
            MainPopup = new MainWindow();
            Deactivated += OnDeactivated;
            Exit += OnExit;
        }

        private void OnDeactivated(object sender, EventArgs e)
        {
            MainPopup.OnDeactivated();
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            if (InstanceEvent != null)
            {
                InstanceEvent.Close();
                InstanceEvent = null;
            }

            using (MainWindow Popup = MainPopup)
            {
                MainPopup = null;
            }
        }
        #endregion

        #region Current Time
        // Use this for debugging purpose only.
        public static void IncreaseNow()
        {
            TimeOffset += TimeSpan.FromDays(1);
        }

        public static DateTime Now()
        {
            return DateTime.UtcNow + TimeOffset;
        }

        private static TimeSpan TimeOffset = TimeSpan.Zero;
        #endregion

        #region Settings
        private static void InitSettings()
        {
            try
            {
                RegistryKey Key = Registry.CurrentUser.OpenSubKey(@"Software", true);
                Key = Key.CreateSubKey("Project Gorgon Tools");
                SettingKey = Key.CreateSubKey("PgMoon");
            }
            catch
            {
            }
        }

        private static object GetSettingKey(string ValueName)
        {
            try
            {
                return SettingKey?.GetValue(ValueName);
            }
            catch
            {
                return null;
            }
        }

        private static void SetSettingKey(string ValueName, object Value, RegistryValueKind Kind)
        {
            try
            {
                SettingKey?.SetValue(ValueName, Value, Kind);
            }
            catch
            {
            }
        }

        private static void DeleteSetting(string ValueName)
        {
            try
            {
                SettingKey?.DeleteValue(ValueName, false);
            }
            catch
            {
            }
        }

        public static bool IsBoolKeySet(string ValueName)
        {
            int? Value = GetSettingKey(ValueName) as int?;
            return Value.HasValue;
        }

        public static bool GetSettingBool(string ValueName, bool Default)
        {
            int? Value = GetSettingKey(ValueName) as int?;
            return Value.HasValue ? (Value.Value != 0) : Default;
        }

        public static void SetSettingBool(string ValueName, bool Value)
        {
            SetSettingKey(ValueName, Value ? 1 : 0, RegistryValueKind.DWord);
        }

        public static int GetSettingInt(string ValueName, int Default)
        {
            int? Value = GetSettingKey(ValueName) as int?;
            return Value.HasValue ? Value.Value : Default;
        }

        public static void SetSettingInt(string ValueName, int Value)
        {
            SetSettingKey(ValueName, Value, RegistryValueKind.DWord);
        }

        public static string GetSettingString(string ValueName, string Default)
        {
            string Value = GetSettingKey(ValueName) as string;
            return Value != null ? Value : Default;
        }

        public static void SetSettingString(string ValueName, string Value)
        {
            if (Value == null)
                DeleteSetting(ValueName);
            else
                SetSettingKey(ValueName, Value, RegistryValueKind.String);
        }

        private static RegistryKey SettingKey = null;
        #endregion

        #region Implementation of IDisposable
        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
                DisposeNow();
        }

        private void DisposeNow()
        {
            if (InstanceEvent != null)
            {
                InstanceEvent.Close();
                InstanceEvent = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~App()
        {
            Dispose(false);
        }
        #endregion
    }
}
