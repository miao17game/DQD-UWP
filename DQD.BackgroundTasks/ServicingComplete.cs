using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace DQD.BackgroundTasks {
    public sealed class ServicingComplete : IBackgroundTask {

        public async void Run(IBackgroundTaskInstance taskInstance) {
            var deferral = taskInstance.GetDeferral();

            var status = await BackgroundExecutionManager.RequestAccessAsync();
            if (status == BackgroundAccessStatus.Unspecified || status == BackgroundAccessStatus.DeniedBySystemPolicy || status == BackgroundAccessStatus.DeniedByUser) { return; }

            var task = FindTask(liveTitleTask);
            if (task != null)
                task.Unregister(true);

            this.RegisterLiveTitleTask();

            deferral.Complete();
        }

        // 
        // Check for a registration of the named background task. If one
        // exists, return it.
        // 
        public static BackgroundTaskRegistration FindTask(string taskName) {
            foreach (var cur in BackgroundTaskRegistration.AllTasks) 
                if (cur.Value.Name == taskName) 
                    return (BackgroundTaskRegistration)(cur.Value);
            return null;
        }

        private const string liveTitleTask = "LIVE_TITLE_TASK";
        private void RegisterLiveTitleTask() {
            
            foreach (var item in BackgroundTaskRegistration.AllTasks)
                if (item.Value.Name == liveTitleTask)
                    item.Value.Unregister(true);

            var taskBuilder = new BackgroundTaskBuilder {
                Name = liveTitleTask,
                TaskEntryPoint = typeof(NotificationBackgroundUpdateTask).FullName
            };

            taskBuilder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
            
            taskBuilder.SetTrigger(new TimeTrigger(15, false));
            var register = taskBuilder.Register();
        }

    }
}
