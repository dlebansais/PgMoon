using Microsoft.Win32.TaskScheduler;
using System.IO;

namespace SchedulerTools
{
    public static class Scheduler
    {
        #region Client Interface
        public static bool AddTask(string TaskName, string ExeName)
        {
            try
            {
                char[] InvalidChars = Path.GetInvalidFileNameChars();
                foreach (char InvalidChar in InvalidChars)
                    TaskName = TaskName.Replace(InvalidChar, ' ');

                TaskService Scheduler = new TaskService();
                Trigger LogonTrigger = Trigger.CreateTrigger(TaskTriggerType.Logon);
                ExecAction RunAction = Action.CreateAction(TaskActionType.Execute) as ExecAction;
                RunAction.Path = ExeName;

                Task task;
                try
                {
                    // Try with a task name (mandatory on new versions of Windows)
                    task = Scheduler.AddTask(TaskName, LogonTrigger, RunAction);
                    return true;
                }
                catch
                {
                }

                try
                {
                    // Try without a task name (mandatory on old versions of Windows)
                    task = Scheduler.AddTask(null, LogonTrigger, RunAction);
                    return true;
                }
                catch
                {
                }
            }
            catch
            {
            }

            return false;
        }

        public static bool IsTaskActive(string ExeName)
        {
            bool IsFound = false;
            EnumTasks(ExeName, OnList, ref IsFound);
            return IsFound;
        }

        public static void RemoveTask(string ExeName)
        {
            bool IsFound = false;
            EnumTasks(ExeName, OnRemove, ref IsFound);
        }
        #endregion

        #region Implementation
        private delegate void EnumTaskHandler(Task Task, ref bool ReturnValue);

        private static void OnList(Task Task, ref bool ReturnValue)
        {
            Trigger LogonTrigger = Task.Definition.Triggers[0];
            if (LogonTrigger.Enabled)
                ReturnValue = true;
        }

        private static void OnRemove(Task Task, ref bool ReturnValue)
        {
            TaskService Scheduler = Task.TaskService;
            TaskFolder RootFolder = Scheduler.RootFolder;
            RootFolder.DeleteTask(Task.Name, false);
        }

        private static void EnumTasks(string ExeName, EnumTaskHandler Handler, ref bool ReturnValue)
        {
            string ProgramName = Path.GetFileName(ExeName);

            try
            {
                TaskService Scheduler = new TaskService();

                foreach (Task t in Scheduler.AllTasks)
                {
                    try
                    {
                        TaskDefinition Definition = t.Definition;
                        if (Definition.Actions.Count != 1 || Definition.Triggers.Count != 1)
                            continue;

                        ExecAction AsExecAction;
                        if ((AsExecAction = Definition.Actions[0] as ExecAction) == null)
                            continue;

                        if (!AsExecAction.Path.EndsWith(ProgramName) || Path.GetFileName(AsExecAction.Path) != ProgramName)
                            continue;

                        Handler(t, ref ReturnValue);
                        return;
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }
        #endregion
    }
}
