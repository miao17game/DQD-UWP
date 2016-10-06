using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using Windows.Storage.Search;
using Windows.Storage.FileProperties;
using Windows.UI.Core;
using System.Threading;
using System.Collections.ObjectModel;
using DQD.Core.Tools;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace DQD.Core.Models {
    /// <summary>
    /// Handle Data and resources
    /// </summary>
    public static class DataHandler {
        private const string HomeHost = "http://www.dongqiudi.com/";
        /// <summary>
        /// Handle the html resources from DQD Header Group.
        /// </summary>
        /// <returns></returns>
        public static List<HeaderModel> SetHeaderGroupResources(string stringDUB) {
            const string HomeHostInsert = "http://www.dongqiudi.com/";
            var HeaderGroup = new List<HeaderModel>();
            try {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(stringDUB);
                HtmlNode rootnode = doc.DocumentNode;
                string XPathString = "//div[@id='tab']";
                HtmlNodeCollection hrefsDiv = rootnode.SelectNodes(XPathString);
                var hrefs = hrefsDiv.ElementAt(0).SelectNodes("a");
                foreach (var item in hrefs) {
                    HeaderModel model = new HeaderModel();
                    model.pathHref = HomeHostInsert + item.Attributes["href"].Value;
                    model.Title = item.InnerText;
                    model.Number = Convert.ToInt32(item.Attributes["rel"].Value);
                    HeaderGroup.Add(model);
                }
            } catch (NullReferenceException NRE) {
                Debug.WriteLine(NRE.Message.ToString());
            } catch (ArgumentOutOfRangeException AOORE) {
                Debug.WriteLine(AOORE.Message.ToString());
            } catch (ArgumentNullException ANE) {
                Debug.WriteLine(ANE.Message.ToString());
            } catch (FormatException FE) {
                Debug.WriteLine(FE.Message.ToString());
            } catch (Exception E) {
                Debug.WriteLine(E.Message.ToString());
            }
            return HeaderGroup;
        }

        /// <summary>
        /// Handle the html resources from DQD SpecialHeader Group.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<HeaderModel>> SetSpecialHeaderGroupResources(string targetPath) {
            string HomeHostInsert = targetPath + "/";
            var HeaderGroup = new List<HeaderModel>();
            try {
                StringBuilder urlString = new StringBuilder();
                urlString = await WebProcess.GetHtmlResources(HomeHostInsert);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(urlString.ToString());
                HtmlNode rootnode = doc.DocumentNode;
                string XPathString = "//div[@id='tab']";
                HtmlNodeCollection hrefsDiv = rootnode.SelectNodes(XPathString);
                var hrefs = hrefsDiv.ElementAt(0).SelectNodes("a");
                foreach (var item in hrefs) {
                    HeaderModel model = new HeaderModel();
                    model.pathHref = HomeHostInsert + item.Attributes["href"].Value;
                    model.Title = item.InnerText;
                    model.Number = Convert.ToInt32(item.Attributes["rel"].Value);
                    HeaderGroup.Add(model);
                }
            } catch (NullReferenceException NRE) {
                Debug.WriteLine(NRE.Message.ToString());
            } catch (ArgumentOutOfRangeException AOORE) {
                Debug.WriteLine(AOORE.Message.ToString());
            } catch (ArgumentNullException ANE) {
                Debug.WriteLine(ANE.Message.ToString());
            } catch (FormatException FE) {
                Debug.WriteLine(FE.Message.ToString());
            } catch (Exception E) {
                Debug.WriteLine(E.Message.ToString());
            }
            return HeaderGroup;
        }

        /// <summary>
        /// Handle the html resources from DQD Header Group.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<HeaderModel>> SetMatchGroupResources() {
            const string HomeHostInsert = "http://www.dongqiudi.com/match/"; 
            var HeaderGroup = new List<HeaderModel>();
            try {
                StringBuilder urlString = new StringBuilder();
                urlString = await WebProcess.GetHtmlResources(HomeHostInsert);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(urlString.ToString());
                HtmlNode rootnode = doc.DocumentNode;
                string XPathString = "//div[@id='match_list']";
                HtmlNodeCollection hrefsDiv = rootnode.SelectNodes(XPathString);
                var hrefs = hrefsDiv.ElementAt(0).SelectNodes("a");
                foreach (var item in hrefs) {
                    HeaderModel model = new HeaderModel();
                    model.pathHref = HomeHostInsert + item.Attributes["href"].Value;
                    model.Title = item.InnerText.Substring(17, item.InnerText.Length - 30);
                    model.Number = Convert.ToInt32(item.Attributes["rel"].Value);
                    HeaderGroup.Add(model);
                }
            } catch (NullReferenceException NRE) {
                Debug.WriteLine(NRE.Message.ToString());
            } catch (ArgumentOutOfRangeException AOORE) {
                Debug.WriteLine(AOORE.Message.ToString());
            } catch (ArgumentNullException ANE) {
                Debug.WriteLine(ANE.Message.ToString());
            } catch (FormatException FE) {
                Debug.WriteLine(FE.Message.ToString());
            } catch (Exception E) {
                Debug.WriteLine(E.Message.ToString());
            }
            return HeaderGroup;
        }

        public static async Task<List<ContentListModel>> SetHomeListResources() {
            return await SetHomeListResourcesForBackTask("http://www.dongqiudi.com/");
        }

        /// <summary>
        /// Handle the html resources from DQD HomePage.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<ContentListModel>> SetHomeListResources(string homeHost) {
            const string HomeHostInsert = "http://www.dongqiudi.com";
            string hone = homeHost;
            List<ContentListModel> HomeList = new List<ContentListModel>();
            try {
                StringBuilder urlString = new StringBuilder();
                urlString=await WebProcess.GetHtmlResources(homeHost);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(urlString.ToString());
                HtmlNode rootnode = doc.DocumentNode;
                string XPathString = "//div[@id='news_list']";
                HtmlNodeCollection aa = rootnode.SelectNodes(XPathString);
                var li = aa.ElementAt(0).SelectNodes("ol").ElementAt(0).SelectNodes("li");
                foreach(var item in li) {
                    ContentListModel model = new ContentListModel();
                    model.Date=item.SelectSingleNode("div[@class='info']").SelectSingleNode("span[@class='time']").InnerText;
                    model.ShareNum=Convert.ToInt32(item.SelectSingleNode("div[@class='info']").SelectSingleNode("a[@class='comment']").InnerText);
                    model.Share=new Uri(HomeHostInsert+item.SelectSingleNode("div[@class='info']").SelectSingleNode("a[@class='comment']").Attributes["href"].Value);
                    var href = item.SelectSingleNode("a").Attributes["href"].Value;
                    Regex rex = new Regex(@"[0-9]{1,}");
                    var collection = rex.Match(href);
                    model.Path = new Uri(HomeHostInsert + href);
                    model.ID = Convert.ToInt32(collection.Value);
                    model.Title= Tools.PersonalExpressions.EscapeReplace.ToEscape(item.SelectSingleNode("h2").SelectNodes("a").FirstOrDefault().InnerText.ToString());
                    var coll = item.SelectNodes("a").ElementAt(0).InnerText;
                    string imgSource = !string.IsNullOrEmpty(coll) ? item.SelectNodes("a").ElementAt(0).SelectSingleNode("img").Attributes.FirstOrDefault().Value : null;
                    model.Image=new BitmapImage(new Uri(string.IsNullOrEmpty(imgSource) ? item.SelectNodes("a").ElementAt(1).SelectSingleNode("img").Attributes.FirstOrDefault().Value : imgSource));
                    HomeList.Add(model);
                }
            } catch(NullReferenceException NRE) { Debug.WriteLine(NRE.Message.ToString());
            } catch(ArgumentOutOfRangeException AOORE) { Debug.WriteLine(AOORE.Message.ToString());
            } catch(ArgumentNullException ANE) { Debug.WriteLine(ANE.Message.ToString());
            } catch(FormatException FE) { Debug.WriteLine(FE.Message.ToString());
            } catch(Exception E) { Debug.WriteLine(E.Message + "\n" + E.StackTrace);
            } return HomeList;
        }

        public static async Task<List<ContentListModel>> SetHomeListResourcesForBackTask(string homeHost) {
            const string HomeHostInsert = "http://www.dongqiudi.com";
            string hone = homeHost;
            List<ContentListModel> HomeList = new List<ContentListModel>();
            try {
                StringBuilder urlString = new StringBuilder();
                urlString = await WebProcess.GetHtmlResources(homeHost);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(urlString.ToString());
                HtmlNode rootnode = doc.DocumentNode;
                string XPathString = "//div[@id='news_list']";
                HtmlNodeCollection aa = rootnode.SelectNodes(XPathString);
                var li = aa.ElementAt(0).SelectNodes("ol").ElementAt(0).SelectNodes("li");
                foreach (var item in li) {
                    ContentListModel model = new ContentListModel();
                    model.Date = item.SelectSingleNode("div[@class='info']").SelectSingleNode("span[@class='time']").InnerText;
                    model.ShareNum = Convert.ToInt32(item.SelectSingleNode("div[@class='info']").SelectSingleNode("a[@class='comment']").InnerText);
                    model.Share = new Uri(HomeHostInsert + item.SelectSingleNode("div[@class='info']").SelectSingleNode("a[@class='comment']").Attributes["href"].Value);
                    var href = item.SelectSingleNode("a").Attributes["href"].Value;
                    Regex rex = new Regex(@"[0-9]{1,}");
                    var collection = rex.Match(href);
                    model.Path = new Uri(HomeHostInsert + href);
                    model.ID = Convert.ToInt32(collection.Value);
                    model.Title = Tools.PersonalExpressions.EscapeReplace.ToEscape(item.SelectSingleNode("h2").SelectNodes("a").FirstOrDefault().InnerText.ToString());
                    var coll = item.SelectNodes("a").ElementAt(0).InnerText;
                    string imgSource = !string.IsNullOrEmpty(coll) ? item.SelectNodes("a").ElementAt(0).SelectSingleNode("img").Attributes.FirstOrDefault().Value : null;
                    model.ImageSource = new Uri(string.IsNullOrEmpty(imgSource) ? item.SelectNodes("a").ElementAt(1).SelectSingleNode("img").Attributes.FirstOrDefault().Value : imgSource);
                    HomeList.Add(model);
                }
            } catch (NullReferenceException NRE) { Debug.WriteLine(NRE.Message.ToString());
            } catch (ArgumentOutOfRangeException AOORE) { Debug.WriteLine(AOORE.Message.ToString());
            } catch (ArgumentNullException ANE) { Debug.WriteLine(ANE.Message.ToString());
            } catch (FormatException FE) { Debug.WriteLine(FE.Message.ToString());
            } catch (Exception E) { Debug.WriteLine(E.Message + "\n" + E.StackTrace);
            } return HomeList;
        }

        //// 需要确保只有一个请求正在进行一次
        //private static SemaphoreSlim gettingFileProperties = new SemaphoreSlim(1);

        //// 获取指定文件的所有数据
        //public async static Task<HomeListModel> fromHomeListResources ( ) {
        //    HomeListModel item = new HomeListModel( );
        //    item . Filename = file . DisplayName;

        //    // 块来确保我们只有一个请求优秀
        //    await gettingFileProperties . WaitAsync ( );

        //    BasicProperties basicProperties = null;
        //    try {
        //        basicProperties = await file . GetBasicPropertiesAsync ( ) . AsTask ( token );
        //    } catch ( Exception ) { 
        //    } finally { gettingFileProperties . Release ( ); }

        //    token . ThrowIfCancellationRequested ( );

        //    item . Size = ( int ) basicProperties . Size;
        //    item . Key = file . FolderRelativeId;

        //    StorageItemThumbnail thumb = await file . GetThumbnailAsync ( ThumbnailMode . SingleItem ) . AsTask ( token );
        //    token . ThrowIfCancellationRequested ( );
        //    BitmapImage img = new BitmapImage ( );
        //    await img . SetSourceAsync ( thumb ) . AsTask ( token );

        //    item . ImageData = img;
        //    return item;
        //}
    }
}
