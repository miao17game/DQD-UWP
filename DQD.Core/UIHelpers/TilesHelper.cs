using DQD.Core.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;

namespace DQD.Core.UIHelpers {
    public class TilesHelper {
        public static async Task<SecondaryTile> PinNewSecondaryTile() {
            SecondaryTile tile = GenerateSecondaryTile("SecondaryTitle", "DQD UWP");
            await tile.RequestCreateAsync();
            return tile;
        }

        public static SecondaryTile GenerateSecondaryTile(string tileId, string displayName, Windows.UI.Color color) {
            SecondaryTile tile = new SecondaryTile(tileId, displayName, "args", new Uri("ms-appx:///Assets/Square150x150Logo.png"), TileSize.Default);
            tile.VisualElements.Square71x71Logo = new Uri("ms-appx:///Assets/Square71x71Logo.png");
            tile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/Square310x150Logo.png");
            tile.VisualElements.Square310x310Logo = new Uri("ms-appx:///Assets/Square310x310Logo.png");
            tile.VisualElements.Square44x44Logo = new Uri("ms-appx:///Assets/Square44x44Logo.png"); // Branding logo
            tile.VisualElements.BackgroundColor = color;
            return tile;
        }

        public static SecondaryTile GenerateSecondaryTile(string titleId , string displayName) {
            return GenerateSecondaryTile(titleId, displayName, Windows.UI.Colors.Transparent);
        }

        public static async Task<SecondaryTile> FindExisting(string tileId) {
            return (await SecondaryTile.FindAllAsync()).FirstOrDefault(i => i.TileId.Equals(tileId));
        }

        public static async Task<SecondaryTile> PinNewSecondaryTile(string titleId, string displayName, string xml) {
            SecondaryTile tile = GenerateSecondaryTile(titleId, displayName);
            await tile.RequestCreateAsync();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            TileUpdateManager.CreateTileUpdaterForSecondaryTile(tile.TileId).Update(new TileNotification(doc));

            return tile;
        }

        public static async Task UpdateTiles(string xml) {
            XmlDocument doc;

            try {
                doc = new XmlDocument();
                doc.LoadXml(xml);
            } catch (Exception) {
                new ToastSmooth("错误: 非法的磁贴XML模型").Show();
                return;
            }

            await UpdateTiles(doc);
        }

        public static async Task UpdateTiles(XmlDocument doc) {
            try {
                TileUpdateManager.CreateTileUpdaterForApplication().Update(new TileNotification(doc));
                await UpdateTile("Small", doc);
                await UpdateTile("Medium", doc);
                await UpdateTile("Wide", doc);
                await UpdateTile("Large", doc);
            } catch (Exception) {
                new ToastSmooth("错误: 创建磁贴失败").Show();
            }
        }

        public static async Task UpdateTile(string tileId, XmlDocument doc) {
            if (!SecondaryTile.Exists(tileId)) {
                SecondaryTile tile = GenerateSecondaryTile(tileId, tileId, Windows.UI.Colors.Transparent);
                tile.VisualElements.ShowNameOnSquare310x310Logo = true;
                await tile.RequestCreateAsync();
            }
            //await GenerateSecondaryTile(tileId, tileId).RequestCreateAsync();
            TileUpdateManager.CreateTileUpdaterForSecondaryTile(tileId).Update(new TileNotification(doc));
        }

        public static async void UpdateTitles(List<string> news) {
            if (news == null || !news.Any()) { return; }
            try {
                var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                InitLiveTitleState(updater);
                foreach (var item in news) { updater.Update(new TileNotification(CreatXMLDocument(item))); }
                foreach (var item in await SecondaryTile.FindAllAsync()) {
                    var updaterForTrans = TileUpdateManager.CreateTileUpdaterForSecondaryTile(item.TileId);
                    InitLiveTitleState(updaterForTrans);
                    foreach (var newsItem in news) { updaterForTrans.Update(new TileNotification(CreatXMLDocument(newsItem))); }
                }
            } catch (Exception) { /*do not need to report excepton. */}
        }

        private static void InitLiveTitleState(TileUpdater updater) {
            updater.EnableNotificationQueueForWide310x150(true);
            updater.EnableNotificationQueueForSquare150x150(true);
            updater.EnableNotificationQueueForSquare310x310(true);
            updater.EnableNotificationQueue(true);
            updater.Clear();
        }

        private static XmlDocument CreatXMLDocument(string newsItem) {
            var doc = new XmlDocument();
            var TileTemplateXml = $@"
                                        <tile branding='nameAndLogo' displayName='{DateTime.Now.Hour.ToString()}:{(DateTime.Now.Minute >= 10 ? DateTime.Now.Minute.ToString() : "0" + DateTime.Now.Minute.ToString())}'> 
                                             <visual version='3'>
                                                 <binding template='TileMedium'>
                                                    <text hint-wrap='true' >{newsItem}</text>
                                                 </binding>
                                                 <binding template='TileWide'>
                                                    <text hint-wrap='true' >{newsItem}</text>
                                                 </binding>
                                                 <binding template='TileLarge'>
                                                    <text hint-wrap='true' >{newsItem}</text>
                                                 </binding>
                                             </visual>
                                        </tile>";
            doc.LoadXml(TileTemplateXml, new XmlLoadSettings {
                ProhibitDtd = false,
                ValidateOnParse = false,
                ElementContentWhiteSpace = false,
                ResolveExternals = false
            });
            return doc;
        }
    }
}
