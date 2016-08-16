using DQD.Core.Models.CommentModels;
using DQD.Core.Models.MatchModels;
using DQD.Core.Models.PageContentModels;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace DQD.Core. Tools {
    public static class DataProcess {
        #region Properties and State

        private const string HomeHost = "http://www.dongqiudi.com/";
        private const string MatchHost = "http://www.dongqiudi.com/match";
        private const string DefaultImageFlagHost = "http://static1.dongqiudi.com/web-new/web/images/defaultTeam.png";
        private enum TableItemType { Round = 0, Away = 1, Home = 2, Link = 3, Vs = 4, Stat = 5, Live = 6 ,Times = 7 } 

        #endregion

        public static Uri ConvertToUri(string str) { return !string.IsNullOrEmpty(str) ? new Uri(str) : null; }
        
        public static PageContentModel GetPageInnerContent(string stringBUD) {
            var model = new PageContentModel();
            try {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(stringBUD);
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
                model.ContentGif = new List<ContentGifs>();
                model.ContentVideo = new List<ContentVideos>();
                model.ContentFlash = new List<ContentFlashs>();
                foreach (var item in contents) {
                    index++;
                    if (item.SelectSingleNode("img") != null) {
                        string Rstring = ".+.gif";
                        Regex reg = new Regex(Rstring);
                        var targetStr = item.SelectSingleNode("img").Attributes["src"].Value;
                        var coll = reg.Matches(targetStr);
                        if (coll.Count == 0) { model.ContentImage.Add(new ContentImages { Image = new BitmapImage(new Uri(targetStr)), Index = index });
                        } else { model.ContentGif.Add(new ContentGifs { ImageUri = new Uri(targetStr), Index = index }); }
                    } else if (item.SelectSingleNode("video") != null) { model.ContentVideo.Add(new ContentVideos { VideoUri = new Uri(item.SelectSingleNode("video").Attributes["src"].Value), Index = index });
                    } else if (item.SelectSingleNode("embed") != null) { model.ContentFlash.Add(new ContentFlashs { FlashUri = new Uri(item.SelectSingleNode("embed").Attributes["src"].Value), Index = index });
                    } else { model.ContentString.Add(new ContentStrings { Content = item.InnerText, Index = index }); }
                }
            } catch (NullReferenceException NRE) { Debug.WriteLine(NRE.Message.ToString());
            } catch (ArgumentOutOfRangeException AOORE) { Debug.WriteLine(AOORE.Message.ToString());
            } catch (ArgumentNullException ANE) { Debug.WriteLine(ANE.Message.ToString());
            } catch (FormatException FE) { Debug.WriteLine(FE.Message.ToString());
            } catch (Exception E) { Debug.WriteLine(E.Message.ToString());
            } return model;
        }

        public static List<CommentModel> GetPageTopComments(string stringBUD) {
            var list = new List<CommentModel>();
            try {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(stringBUD);
                HtmlNode rootnode = doc.DocumentNode;
                string XPathString = "//ol[@id='top_comment']";
                HtmlNodeCollection consDiv = rootnode.SelectNodes(XPathString);
                var wholeContentColl = consDiv.ElementAt(0).SelectNodes("li");
                foreach (var eachLi in wholeContentColl) {
                    var model = new CommentModel();
                    model.Image = new BitmapImage(new Uri(eachLi.SelectSingleNode("img").Attributes["src"].Value));
                    model.Name = eachLi.SelectSingleNode("p[@class='nameCon']").SelectSingleNode("span[@class='name']").InnerText;
                    model.Time = eachLi.SelectSingleNode("p[@class='nameCon']").SelectSingleNode("span[@class='time']").InnerText;
                    var targetStr = eachLi.SelectSingleNode("p[@class='comCon']").InnerText;
                    model.Content = targetStr.Substring(9, targetStr.Length-13);
                    list.Add(model);
                }
            } catch (NullReferenceException NRE) { Debug.WriteLine(NRE.Message.ToString());
            } catch (ArgumentOutOfRangeException AOORE) { Debug.WriteLine(AOORE.Message.ToString());
            } catch (ArgumentNullException ANE) { Debug.WriteLine(ANE.Message.ToString());
            } catch (FormatException FE) { Debug.WriteLine(FE.Message.ToString());
            } catch (Exception E) { Debug.WriteLine(E.Message.ToString());
            }
            return list;
        }

        public static async Task<List<AllCommentModel>> GetPageAllComments(string targetHost) {
            var list = new List<AllCommentModel>();
            try {
                StringBuilder urlString = new StringBuilder();
                urlString = await WebProcess.GetHtmlResources(targetHost);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(urlString.ToString());
                HtmlNode rootnode = doc.DocumentNode;
                string XPathString = "//ol[@id='all_comment']";
                HtmlNodeCollection consDiv = rootnode.SelectNodes(XPathString);
                var wholeContentColl = consDiv.ElementAt(0).SelectNodes("li");
                foreach (var eachLi in wholeContentColl) {
                    var model = new AllCommentModel();
                    model.Image = new BitmapImage(new Uri(eachLi.SelectSingleNode("img").Attributes["src"].Value));
                    model.Name = eachLi.SelectSingleNode("p[@class='nameCon']").SelectSingleNode("span[@class='name']").InnerText;
                    model.Time = eachLi.SelectSingleNode("p[@class='nameCon']").SelectSingleNode("span[@class='time']").InnerText;
                    var targetStr = eachLi.SelectSingleNode("p[@class='comCon']").InnerText;
                    model.Content = targetStr.Substring(9, targetStr.Length - 13);
                    var ReDiv = eachLi.SelectSingleNode("div[@class='recomm']");
                    if (ReDiv != null) {
                        model.ReName = ReDiv.SelectNodes("p").ElementAt(0).SelectSingleNode("span[@class='name']").InnerText;
                        model.ReTime = ReDiv.SelectNodes("p").ElementAt(0).SelectSingleNode("span[@class='time']").InnerText;
                        var targetStr2 = ReDiv.SelectNodes("p").ElementAt(1).InnerText;
                        model.ReContent = targetStr2.Substring(17, targetStr2.Length - 17);
                    }
                    list.Add(model);
                }
            } catch (NullReferenceException NRE) { Debug.WriteLine(NRE.Message.ToString());
            } catch (ArgumentOutOfRangeException AOORE) { Debug.WriteLine(AOORE.Message.ToString());
            } catch (ArgumentNullException ANE) { Debug.WriteLine(ANE.Message.ToString());
            } catch (FormatException FE) { Debug.WriteLine(FE.Message.ToString());
            } catch (Exception E) { Debug.WriteLine(E.Message.ToString());
            }
            return list;
        }

        public static List<MatchListModel> GetMatchItemsContent(string stringBUD) {
            var list = new List<MatchListModel>();
            try {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(stringBUD);
                HtmlNode rootnode = doc.DocumentNode;
                string XPathString = "//tr";
                HtmlNodeCollection consTrs = rootnode.SelectNodes(XPathString);
                string Category = default(string);
                foreach (var TrItem in consTrs) {
                    if (TrItem.SelectSingleNode("th")!= null)
                        Category = TrItem.SelectSingleNode("th").InnerText;
                    else {
                        var model = new MatchListModel();
                        model.GroupCategory = Category;
                        model.Rel = Convert.ToInt64(TrItem.Attributes["rel"].Value);
                        model.ID = TrItem.Attributes["id"].Value;
                        var TdItems = TrItem.SelectNodes("td");
                        foreach (var item in TdItems) {
                            TableItemType type =
                                item.Attributes["class"].Value.Equals("times") ? TableItemType.Times :
                                item.Attributes["class"].Value.Equals("round") ? TableItemType.Round :
                                item.Attributes["class"].Value.Equals("away") ? TableItemType.Away :
                                item.Attributes["class"].Value.Equals("home") ? TableItemType.Home :
                                item.Attributes["class"].Value.Equals("vs") ? TableItemType.Vs :
                                item.Attributes["class"].Value.Equals("stat") ? TableItemType.Stat :
                                item.Attributes["class"].Value.Equals("stat live") ? TableItemType.Live :
                                item.Attributes["class"].Value.Equals("link") ? TableItemType.Link :
                                default(TableItemType);
                            switch (type) {
                                case TableItemType.Times:
                                    model.Time = item.SelectSingleNode("p")==null? item.InnerText:"直播中";
                                    break;
                                case TableItemType.Round:
                                    model.MatchRound = item.InnerText.Substring(21, item.InnerText.Length - 38);
                                    break;
                                case TableItemType.Away:
                                    model.AwayTeam = item.InnerText.Substring(50, item.InnerText.Length-67);
                                    model.AwayImage = string.IsNullOrEmpty(new Regex(@"\/.png").Match(item.SelectSingleNode("img").Attributes["src"].Value).Value) ? 
                                        new Uri(item.SelectSingleNode("img").Attributes["src"].Value) : 
                                        new Uri(DefaultImageFlagHost);
                                    break;
                                case TableItemType.Home:
                                    model.HomeTeam = item.InnerText.Substring(25, item.InnerText.Length - 67);
                                    model.HomeImage = string.IsNullOrEmpty(new Regex(@"\/.png").Match(item.SelectSingleNode("img").Attributes["src"].Value).Value) ?
                                        new Uri(item.SelectSingleNode("img").Attributes["src"].Value) : 
                                        new Uri(DefaultImageFlagHost);
                                    break;
                                case TableItemType.Vs:
                                    model.IsOverOrNot = false;
                                    model.Score = "VS";
                                    break;
                                case TableItemType.Stat:
                                    model.IsOverOrNot = true;
                                    model.Score = item.InnerText;
                                    break;
                                case TableItemType.Live:
                                    model.IsOverOrNot = true;
                                    model.Score = item.InnerText;
                                    break;
                                case TableItemType.Link:
                                    var linkContent = item.SelectSingleNode("a");
                                    model.ArticleLink = linkContent == null ? null : new Uri(MatchHost+linkContent.Attributes["href"].Value);
                                    break;
                                default:break;
                            }
                        }
                        list.Add(model);
                    }
                }
            } catch (NullReferenceException NRE) { Debug.WriteLine(NRE.Message.ToString());
            } catch (ArgumentOutOfRangeException AOORE) { Debug.WriteLine(AOORE.Message.ToString());
            } catch (ArgumentNullException ANE) { Debug.WriteLine(ANE.Message.ToString());
            } catch (FormatException FE) { Debug.WriteLine(FE.Message.ToString());
            } catch (Exception E) { Debug.WriteLine(E.Message.ToString());
            }
            return list;
        }
    }
}
