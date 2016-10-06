using DQD.Core.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.UI.Notifications;
using DQD.Core.UIHelpers;
using Windows.UI.Xaml;
using System.Diagnostics;
using Windows.UI.StartScreen;

namespace DQD.BackgroundTasks {
    public sealed class NotificationBackgroundUpdateTask : IBackgroundTask {

        public async void Run(IBackgroundTaskInstance taskInstance) {
            var deferral = taskInstance.GetDeferral();
            await GetLatestNews();
            deferral.Complete();
        }

        private IAsyncOperation<string> GetLatestNews() {
            try {
                return AsyncInfo.Run(token => GetNews());
            } catch (Exception) {
                // ignored
            }
            return null;
        }

        private async Task<string> GetNews() {
            try {
                var listfor = await DataHandler.SetHomeListResources();
                var resultList = listfor
                    .Take(5)
                    .GroupBy(i => i.Title)
                    .Select(s => s.Key)
                    .ToList();
                TilesHelper.UpdateTitles(resultList);
            } catch (Exception) {
                // ignored
            }
            return null;
        }
    }
}
