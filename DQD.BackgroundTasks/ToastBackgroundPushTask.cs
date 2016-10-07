using DQD.BackgroundTasks.Helpers;
using DQD.Core.Helpers;
using DQD.Core.Models;
using DQD.Core.Models.PageContentModels;
using DQD.Core.Tools;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace DQD.BackgroundTasks {
    public sealed class ToastBackgroundPushTask : IBackgroundTask {

        public async void Run(IBackgroundTaskInstance taskInstance) {
            var deferral = taskInstance.GetDeferral();
            await GetNewsAndPushToast();
            deferral.Complete();
        }

        private static async Task GetNewsAndPushToast() {
            foreach (var item in (
                await DataHandler.SetHomeListResources())
                .Skip(4)
                .Take(2)) {
                var resultHtml = await WebProcess.GetHtmlResources(item.Path.ToString());
                try {
                    ToastHelper.PopToast(
                        item.Title,
                        GetPageContent(resultHtml.ToString()),
                        item.ImageSource.ToString(),
                        item.ID.ToString());
                    await Task.Delay(1500);
                } catch { /* don not need to check. */}
            }
        }

        private static string GetPageContent(string stringBUD) {
            var result = default(string);
            try {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(stringBUD);
                result = Core.Tools.PersonalExpressions.EscapeReplace.ToEscape(
                    doc.DocumentNode.SelectNodes("//div[@class='detail']")
                    .ElementAt(0)
                    .SelectSingleNode("div")
                    .InnerText);
            } catch (Exception) {
            } return result;
        }
    }
}
