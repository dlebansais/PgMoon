using System;
using System.Threading;
using System.Windows;

namespace PgMoon
{
    public partial class App : Application
    {
        public App()
        {
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
            MainPopup = new MainWindow();
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

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

        private MainWindow MainPopup;
        private EventWaitHandle InstanceEvent;
    }
}
