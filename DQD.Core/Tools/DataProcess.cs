using DQD.Core.Models.PageContentModels;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace DQD.Core. Tools {
    public static class DataProcess {
        private const string HomeHost = "http://www.dongqiudi.com/";

        public static Uri ConvertToUri(string str) { return !string.IsNullOrEmpty(str) ? new Uri(str) : null; }
        
        public static async Task<PageContentModel> GetPageInnerContent(string targetUri) {
            var model = new PageContentModel();
            try {
                StringBuilder urlString = new StringBuilder();
                urlString = await WebProcess.GetHtmlResources(targetUri);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(urlString.ToString());
                HtmlNode rootnode = doc.DocumentNode;
                string XPathString = "//div[@class='detail']";
                HtmlNodeCollection consDiv = rootnode.SelectNodes(XPathString);
                var detail = consDiv.ElementAt(0);
                model.Title = detail.SelectSingleNode("h1").InnerText;
                var authorDateColl = detail.SelectSingleNode("h4");
                model.Author = authorDateColl.SelectSingleNode("span[@class='name']").InnerText;
                model.Date = authorDateColl.SelectSingleNode("span[@class='time']").InnerText;
                var contents = detail.SelectSingleNode("div").SelectNodes("p");
                uint index = 0;
                model.ContentImage = new List<ContentImages>();
                model.ContentString = new List<ContentStrings>();
                foreach (var item in contents) {
                    index++;
                    if (item.SelectSingleNode("img") != null)
                        model.ContentImage.Add(new ContentImages { Image = new BitmapImage(new Uri(item.SelectSingleNode("img").Attributes["src"].Value)), Index = index });
                    else
                        model.ContentString.Add(new ContentStrings { Content = item.InnerText, Index = index });
                }
            } catch (NullReferenceException NRE) { Debug.WriteLine(NRE.Message.ToString());
            } catch (ArgumentOutOfRangeException AOORE) { Debug.WriteLine(AOORE.Message.ToString());
            } catch (ArgumentNullException ANE) { Debug.WriteLine(ANE.Message.ToString());
            } catch (FormatException FE) { Debug.WriteLine(FE.Message.ToString());
            } catch (Exception E) { Debug.WriteLine(E.Message.ToString());
            }
            return model;
        }
    }
}
