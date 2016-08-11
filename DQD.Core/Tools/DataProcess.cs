using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQD.Core. Tools {
    public static class DataProcess {
        private const string HomeHost = "http://www.dongqiudi.com/";

        public static Uri ConvertToUri(string str) { return !string.IsNullOrEmpty(str) ? new Uri(str) : null; }
        
        public static async Task<string> GetPageInnerContent(string targetUri) {
            var result = default(string);
            try {
                StringBuilder urlString = new StringBuilder();
                urlString = await WebProcess.GetHtmlResources(targetUri);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(urlString.ToString());
                HtmlNode rootnode = doc.DocumentNode;
                string XPathString = "//div[@id='main']";
                HtmlNodeCollection hrefsDiv = rootnode.SelectNodes(XPathString);
                var hrefs = hrefsDiv.ElementAt(0).InnerText;
                result = hrefs;
            } catch (NullReferenceException NRE) { Debug.WriteLine(NRE.Message.ToString());
            } catch (ArgumentOutOfRangeException AOORE) { Debug.WriteLine(AOORE.Message.ToString());
            } catch (ArgumentNullException ANE) { Debug.WriteLine(ANE.Message.ToString());
            } catch (FormatException FE) { Debug.WriteLine(FE.Message.ToString());
            } catch (Exception E) { Debug.WriteLine(E.Message.ToString());
            }
            return result;
        }
    }
}
