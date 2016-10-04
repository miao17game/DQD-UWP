using DQD.Core.Controls;
using DQD.Core.Models;
using DQD.Core.Models.CommentModels;
using DQD.Core.Models.MatchModels;
using DQD.Core.Models.PageContentModels;
using DQD.Core.Models.TeamModels;
using DQD.Core.Tools.PersonalExpressions;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace DQD.Core.Tools {
    public static class DataProcess {
        #region Properties and State

        private const string HomeHost = "http://www.dongqiudi.com/";
        private const string HomeHostInsert = "http://www.dongqiudi.com";
        private const string MatchHost = "http://www.dongqiudi.com/match";
        private const string ArticleHost = "http://dongqiudi.com/article/";
        private const string DefaultImageFlagHost = "http://static1.dongqiudi.com/web-new/web/images/defaultTeam.png";
        private const string DefaultMenberHost = "http://static1.dongqiudi.com/web-new/web/images/defaultPlayer.png";
        private enum TableItemType { Round = 0, Away = 1, Home = 2, Link = 3, Vs = 4, Stat = 5, Live = 6 ,Times = 7 }

        #endregion

        public static async void ReportError(string erroeMessage) {
            await Window.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                new ToastSmooth("获取数据发生错误\n"+erroeMessage).Show();
            });
        }

        public static async void ReportException(string erroeMessage) {
            await Window.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                new ToastSmooth(erroeMessage).Show();
            });
        }

        public static Uri ConvertToUri(string str) { return !string.IsNullOrEmpty(str) ? new Uri(str) : null; }

        public static List<ContentListModel> GetFlipViewContent(string stringBUD) {
            var list = new List<ContentListModel>();
            try {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(stringBUD);
                HtmlNode rootnode = doc.DocumentNode;
                string XPathString = "//div[@id='show']";
                HtmlNodeCollection consDiv = rootnode.SelectNodes(XPathString);
                var wholeContentColl = consDiv.ElementAt(0).SelectNodes("ul/li");
                foreach (var eachLi in wholeContentColl) {
                    try {
                        var model = new ContentListModel();
                        model.Image = new BitmapImage(new Uri(eachLi.SelectSingleNode("a/img").Attributes["src"].Value));
                        model.Title = EscapeReplace.ToEscape(eachLi.SelectSingleNode("a/h3").InnerText);
                        model.Path = new Uri(HomeHostInsert + eachLi.SelectSingleNode("a").Attributes["href"].Value);
                        model.ID = Convert.ToInt32( new Regex(@"\d{6,}").Match(eachLi.SelectSingleNode("a").Attributes["href"].Value).Value);
                        list.Add(model);
                    } catch (NullReferenceException ) { ReportException("读取数据异常");
                    } catch (ArgumentOutOfRangeException AOORE) { ReportError(AOORE.Message.ToString());
                    } catch (ArgumentNullException ANE) { ReportError(ANE.Message.ToString());
                    } catch (FormatException FE) { ReportError(FE.Message.ToString());
                    } catch (Exception E) { ReportError(E.Message.ToString());
                    }
                }
            } catch (Exception E) { ReportError(E.Message.ToString()); }
            return list;
        }

        public static PageContentModel GetPageInnerContent(string stringBUD) {
            var model = new PageContentModel();
            try {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(stringBUD);
                HtmlNode rootnode = doc.DocumentNode;
                string XPathString = "//div[@class='detail']";
                HtmlNodeCollection consDiv = rootnode.SelectNodes(XPathString);
                var detail = consDiv.ElementAt(0);
                model.Title = EscapeReplace.ToEscape(detail.SelectSingleNode("h1").InnerText);
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
                model.ContentSelfUri = new List<ContentSelfUris>();
                foreach (var item in contents) {
                    try {
                        index++;
                        if (item.SelectSingleNode("img") != null) {
                            var targetStr = item.SelectSingleNode("img").Attributes["src"].Value;
                            var coll = new Regex(".+.gif").Matches(targetStr);
                            if (coll.Count == 0) {
                                model.ContentImage.Add(new ContentImages {
                                    Image = new BitmapImage(new Uri(targetStr)),
                                    Index = index
                                });
                            } else { model.ContentGif.Add(new ContentGifs {
                                ImageUri = new Uri(targetStr),
                                Index = index
                            }); }
                        } else if (item.SelectSingleNode("video") != null) {
                            model.ContentVideo.Add(new ContentVideos {
                                VideoUri = new Uri(item.SelectSingleNode("video").Attributes["src"].Value),
                                Index = index
                            });
                        } else if (item.SelectSingleNode("embed") != null) {
                            model.ContentFlash.Add(new ContentFlashs {
                                FlashUri = new Uri(item.SelectSingleNode("embed").Attributes["src"].Value),
                                Index = index
                            });
                        } else if ((item.SelectSingleNode("a") != null)) {
                            var targetStr = item.SelectSingleNode("a").Attributes["href"].Value;
                            var match = new Regex(@"dongqiudi.+\d{6,}").Match(targetStr);
                            if (!string.IsNullOrEmpty(match.Value)) {
                                var contentNumber = new Regex(@"\d{6,}").Match(match.Value).Value;
                                model.ContentSelfUri.Add(new ContentSelfUris {
                                    Uri = new Uri(ArticleHost + contentNumber),
                                    Number = Convert.ToInt32(contentNumber),
                                    Title = item.SelectSingleNode("a").InnerText,
                                    Index = index
                                });
                            } else { model.ContentString.Add(new ContentStrings {
                                Content = EscapeReplace.ToEscape(item.InnerText),
                                Index = index
                            }); }
                        } else { model.ContentString.Add(new ContentStrings {
                            Content = EscapeReplace.ToEscape(item.InnerText),
                            Index = index
                        }); }
                    } catch (NullReferenceException ) { ReportException("部分内容暂不支持显示");
                    } catch (ArgumentOutOfRangeException AOORE) { ReportError(AOORE.Message.ToString());
                    } catch (ArgumentNullException ) { 
                    } catch (FormatException FE) { ReportError(FE.Message.ToString());
                    } catch (Exception E) { ReportError(E.Message.ToString());
                    }
                }
            } catch (ArgumentOutOfRangeException AOORE) { ReportError(AOORE.Message.ToString());
            } catch (ArgumentNullException) {
            } catch (Exception E) { ReportError(E.Message.ToString());
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
                    try {
                        var model = new CommentModel();
                        model.Image = new BitmapImage(new Uri(eachLi.SelectSingleNode("img").Attributes["src"].Value));
                        model.Name = eachLi.SelectSingleNode("p[@class='nameCon']").SelectSingleNode("span[@class='name']").InnerText;
                        model.Time = eachLi.SelectSingleNode("p[@class='nameCon']").SelectSingleNode("span[@class='time']").InnerText;
                        var targetStr = EmojiReplace.ToEmoji(eachLi.SelectSingleNode("p[@class='comCon']").InnerText);
                        model.Content = EscapeReplace.ToEscape(targetStr.Substring(9, targetStr.Length - 13));
                        model.Zan = eachLi.SelectSingleNode("div[@class='ctr']").SelectSingleNode("a[@class='zan']").InnerText;
                        list.Add(model);
                    } catch (NullReferenceException ) { 
                    } catch (ArgumentOutOfRangeException AOORE) { ReportError(AOORE.Message.ToString());
                    } catch (ArgumentNullException) {
                    } catch (FormatException FE) { ReportError(FE.Message.ToString());
                    }
                }
            } catch (ArgumentOutOfRangeException AOORE) { ReportError(AOORE.Message.ToString());
            } catch (ArgumentNullException) {
            } catch (Exception E) { ReportError(E.Message.ToString());
            }
            return list;
        }

        public static async Task<ObservableCollection<AllCommentModel>> GetPageAllComments(string targetHost) {
            var list = new ObservableCollection<AllCommentModel>();
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
                    try {
                        var model = new AllCommentModel();
                        model.Image = new BitmapImage(new Uri(eachLi.SelectSingleNode("img").Attributes["src"].Value));
                        model.Name = eachLi.SelectSingleNode("p[@class='nameCon']").SelectSingleNode("span[@class='name']").InnerText;
                        model.Time = eachLi.SelectSingleNode("p[@class='nameCon']").SelectSingleNode("span[@class='time']").InnerText;
                        var targetStr = EmojiReplace.ToEmoji(eachLi.SelectSingleNode("p[@class='comCon']").InnerText);
                        model.Content = EscapeReplace.ToEscape(targetStr.Substring(9, targetStr.Length - 13));
                        var ReDiv = eachLi.SelectSingleNode("div[@class='recomm']");
                        if (ReDiv != null) {
                            model.ReName = ReDiv.SelectNodes("p").ElementAt(0).SelectSingleNode("span[@class='name']").InnerText;
                            model.ReTime = ReDiv.SelectNodes("p").ElementAt(0).SelectSingleNode("span[@class='time']").InnerText;
                            var targetStr2 = EmojiReplace.ToEmoji(ReDiv.SelectNodes("p").ElementAt(1).InnerText);
                            model.ReContent = targetStr2.Substring(17, targetStr2.Length - 17);
                        } list.Add(model);
                    } catch (NullReferenceException ) { ReportException("抱歉，目前还没有用户评论");
                    } catch (ArgumentOutOfRangeException AOORE) { ReportError(AOORE.Message.ToString());
                    } catch (ArgumentNullException ) { 
                    } catch (FormatException FE) { ReportError(FE.Message.ToString());
                    }
                }
            } catch (ArgumentOutOfRangeException AOORE) { ReportError(AOORE.Message.ToString());
            } catch (ArgumentNullException) {
            } catch (Exception E) { ReportError(E.Message.ToString());
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
                            try {
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
                                        model.Time = item.SelectSingleNode("p") == null ? item.InnerText : "直播中";
                                        break;
                                    case TableItemType.Round:
                                        model.MatchRound = item.InnerText.Substring(21, item.InnerText.Length - 38);
                                        break;
                                    case TableItemType.Away:
                                        model.AwayTeam = item.InnerText.Substring(50, item.InnerText.Length - 67);
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
                                        model.ArticleLink = 
                                            linkContent == null ? null :
                                            !string.IsNullOrEmpty(new Regex(@"article").Match(linkContent.Attributes["href"].Value).Value) ? 
                                            new Uri(HomeHostInsert + linkContent.Attributes["href"].Value) :
                                            new Uri(linkContent.Attributes["href"].Value);
                                        model.ArticleID =
                                            linkContent != null && !string.IsNullOrEmpty(new Regex(@"article").Match(linkContent.Attributes["href"].Value).Value) ?
                                            (int?)Convert.ToInt32(new Regex(@"\d{6,}").Match(linkContent.Attributes["href"].Value).Value) :
                                            null;
                                        model.MatchType =
                                            linkContent == null ? MatchListType.None :
                                            !string.IsNullOrEmpty(new Regex(@"article").Match(linkContent.Attributes["href"].Value).Value) ? MatchListType.Review :
                                            MatchListType.Live;
                                        break;
                                    default: break;
                                }
                            } catch (NullReferenceException ) { ReportException("没有获取到相应的信息");
                            } catch (ArgumentOutOfRangeException AOORE) { ReportError(AOORE.Message.ToString());
                            } catch (ArgumentNullException ) { 
                            } catch (FormatException FE) { ReportError(FE.Message.ToString());
                            }
                        } list.Add(model);
                    }
                }
            } catch (ArgumentOutOfRangeException AOORE) { ReportError(AOORE.Message.ToString());
            } catch (ArgumentNullException) {
            } catch (Exception E) { ReportError(E.Message.ToString());
            }
            return list;
        }

        public static List<LeagueModel> GetLeagueContent(string stringBUD) {
            var list = new List<LeagueModel>();
            try {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(stringBUD);
                HtmlNode rootnode = doc.DocumentNode;
                string XPathString = "//div[@id='stat_list']";
                HtmlNodeCollection startListA = rootnode.SelectNodes(XPathString).ElementAt(0).SelectNodes("a");
                foreach (var listItemA in startListA) {
                    try {
                        var model = new LeagueModel();
                        model.Href = new Uri(HomeHost + listItemA.Attributes["href"].Value);
                        model.LeagueName =
                            listItemA.Attributes["rel"] != null ?
                            listItemA.InnerText.Substring(25, listItemA.InnerText.Length - 25) :
                            listItemA.InnerText.Substring(25, listItemA.InnerText.Length - 25);
                        list.Add(model);
                    } catch (NullReferenceException ) { ReportException("没有获取到相应的信息");
                    } catch (ArgumentOutOfRangeException AOORE) { ReportError(AOORE.Message.ToString());
                    } catch (ArgumentNullException ) { 
                    } catch (FormatException FE) { ReportError(FE.Message.ToString());
                    }
                }
            } catch (ArgumentOutOfRangeException AOORE) { ReportError(AOORE.Message.ToString());
            } catch (ArgumentNullException) {
            } catch (Exception E) { ReportError(E.Message.ToString());
            }
            return list;
        }

        public static List<TeamLeagueModel> GetLeagueTeamsContent(string stringBUD) {
            var list = new List<TeamLeagueModel>();
            try {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(stringBUD);
                HtmlNode rootnode = doc.DocumentNode;
                string XPathString = "//table[@class='list_1']";
                HtmlNodeCollection startListTrs = rootnode.SelectNodes(XPathString);
                foreach (var table in startListTrs) {
                    try {
                        var trs = table.SelectNodes("tr");
                        foreach (var listItem in trs) {
                            var model = new TeamLeagueModel();
                            var ths = listItem.SelectNodes("th");
                            var tds = listItem.SelectNodes("td");
                            TeamLeagueModel.TeamModelType type =
                                tds != null && (tds.ElementAt(0).Attributes["class"] == null && tds.Count == 10) ? TeamLeagueModel.TeamModelType.LeagueTeam :
                                tds != null && (tds.ElementAt(0).Attributes["class"] != null || tds.Count != 10) ? TeamLeagueModel.TeamModelType.CupModel :
                                ths.Count == 1 ? TeamLeagueModel.TeamModelType.ListTitle :
                                TeamLeagueModel.TeamModelType.LeaTeamHeader;
                            switch (type) {
                                case TeamLeagueModel.TeamModelType.ListTitle:
                                    model.ModelType = TeamLeagueModel.TeamModelType.ListTitle;
                                    model.ListTitle = listItem.SelectSingleNode("th").InnerText;
                                    break;
                                case TeamLeagueModel.TeamModelType.LeagueTeam:
                                    model.ModelType = TeamLeagueModel.TeamModelType.LeagueTeam;
                                    InsertLeagueTeamModel(listItem, model);
                                    break;
                                case TeamLeagueModel.TeamModelType.LeaTeamHeader:
                                    model.ModelType = TeamLeagueModel.TeamModelType.LeaTeamHeader;
                                    InsertLeaTeamHeader(model, ths);
                                    break;
                                case TeamLeagueModel.TeamModelType.CupModel:
                                    model.ModelType = TeamLeagueModel.TeamModelType.CupModel;
                                    InsertCupTeamModel(listItem, model);
                                    break;
                            } list.Add(model);
                        }
                    } catch (NullReferenceException ) { ReportException("没有获取到相应的信息");
                    } catch (ArgumentOutOfRangeException AOORE) { ReportError(AOORE.Message.ToString());
                    } catch (ArgumentNullException ) { 
                    } catch (FormatException FE) { ReportError(FE.Message.ToString());
                    }
                }
            } catch (ArgumentOutOfRangeException AOORE) { ReportError(AOORE.Message.ToString());
            } catch (ArgumentNullException) {
            } catch (Exception E) {ReportError(E.Message.ToString());
            }
            return list;
        }

        public static List<SoccerMemberModel> GetSoccerMemberContent(string stringBUD) {
            var list = new List<SoccerMemberModel>();
            try {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(stringBUD);
                HtmlNode rootnode = doc.DocumentNode;
                string XPathString = "//table[@class='list_1']";
                HtmlNodeCollection startListTrs = rootnode.SelectNodes(XPathString);
                foreach (var table in startListTrs) {
                    var trs = table.SelectNodes("tr");
                    foreach (var listItem in trs) {
                        try {
                            var model = new SoccerMemberModel();
                            var ths = listItem.SelectNodes("th");
                            var tds = listItem.SelectNodes("td");
                            SoccerMemberModel.SocMenType type =
                                ths == null || ths.Count == 0 ? SoccerMemberModel.SocMenType.Content :
                                SoccerMemberModel.SocMenType.Header;
                            switch (type) {
                                case SoccerMemberModel.SocMenType.Content:
                                    model.ModelType = SoccerMemberModel.SocMenType.Content;
                                    InsertSoccerMemberModel(listItem, model);
                                    break;
                                case SoccerMemberModel.SocMenType.Header:
                                    model.ModelType = SoccerMemberModel.SocMenType.Header;
                                    InsertSocMemHeaderModel(model, ths);
                                    break;
                            } list.Add(model);
                        } catch (NullReferenceException) { ReportException("没有获取到相应的信息");
                        } catch (ArgumentOutOfRangeException AOORE) { ReportError(AOORE.Message.ToString());
                        } catch (ArgumentNullException ) { 
                        } catch (FormatException FE) { ReportError(FE.Message.ToString());
                        }
                    }
                }
            } catch (ArgumentOutOfRangeException AOORE) { ReportError(AOORE.Message.ToString());
            } catch (ArgumentNullException) {
            } catch (Exception E) { ReportError(E.Message.ToString());
            }
            return list;
        }

        public static List<LeagueScheduleModel> GetScheduleTeamsContent(string stringBUD) {
            var list = new List<LeagueScheduleModel>();
            try {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(stringBUD);
                HtmlNode rootnode = doc.DocumentNode;
                string XPathString = "//table[@class='list_2']";
                HtmlNodeCollection startListTrs = rootnode.SelectNodes(XPathString);
                var theadTrs = startListTrs.ElementAt(0).SelectSingleNode("thead/tr");
                var titleModel = new LeagueScheduleModel();
                titleModel.ModelType = LeagueScheduleModel.ScheduleType.Title;
                if (theadTrs.SelectSingleNode("th[@class='prev']/a") != null) {
                    titleModel.PreTitleUri = new Uri(HomeHostInsert + theadTrs.SelectSingleNode("th[@class='prev']/a").Attributes["href"].Value);
                    titleModel.PreTitle = theadTrs.SelectSingleNode("th[@class='prev']/a").InnerText.Substring(5, theadTrs.SelectSingleNode("th[@class='prev']/a").InnerText.Length - 5);}
                if (theadTrs.SelectSingleNode("th[@class='next']/a") != null) {
                    titleModel.NextTitleUri = new Uri(HomeHostInsert + theadTrs.SelectSingleNode("th[@class='next']/a").Attributes["href"].Value);
                    titleModel.NextTitle = theadTrs.SelectSingleNode("th[@class='next']/a").InnerText.Substring(0, theadTrs.SelectSingleNode("th[@class='next']/a").InnerText.Length - 5);}
                titleModel.ScheduleTitle = theadTrs.SelectSingleNode("th[@id='schedule_title']").InnerText;
                list.Add(titleModel);
                var insideTds = startListTrs.ElementAt(0).SelectSingleNode("tbody[@id='schduleContent']").SelectNodes("tr");
                foreach (var item in insideTds) {
                    try {
                        var model = new LeagueScheduleModel();
                        model.ModelType = LeagueScheduleModel.ScheduleType.Content;
                        model.Time = item.SelectSingleNode("td[@class='time']").Attributes["utc"] != null && !item.SelectSingleNode("td[@class='time']").Attributes["utc"].Value.Equals("0") ?
                            new DateTime(1970, 1, 1, 8, 0, 0).AddSeconds(Convert.ToInt64(item.SelectSingleNode("td[@class='time']").Attributes["utc"].Value)).ToString("yy-MM-dd HH:mm") :
                            item.SelectSingleNode("td[@class='time']").Attributes["utc"] != null && item.SelectSingleNode("td[@class='time']").Attributes["utc"].Value.Equals("0") ?
                            "待定" : "待定";
                        model.AwayTeam = item.SelectSingleNode("td[@class='away']").InnerText;
                        model.HomeTeam = item.SelectSingleNode("td[@class='home']").InnerText;
                        model.AwayTeamIcon =
                            string.IsNullOrEmpty(new Regex(@"\/.png").Match(item.SelectSingleNode("td[@class='away']/img").Attributes["src"].Value).Value) ?
                            new Uri(item.SelectSingleNode("td[@class='away']/img").Attributes["src"].Value) :
                            new Uri(DefaultImageFlagHost);
                        model.HomeTeamIcon =
                            string.IsNullOrEmpty(new Regex(@"\/.png").Match(item.SelectSingleNode("td[@class='home']/img").Attributes["src"].Value).Value) ?
                            new Uri(item.SelectSingleNode("td[@class='home']/img").Attributes["src"].Value) :
                            new Uri(DefaultImageFlagHost);
                        model.Score= item.SelectSingleNode("td[@class='status']").InnerText;
                        list.Add(model);
                    } catch (NullReferenceException) { ReportException("没有获取到相应的信息");
                    } catch (ArgumentOutOfRangeException AOORE) { ReportError(AOORE.Message.ToString());
                    } catch (ArgumentNullException ) { 
                    } catch (FormatException FE) { ReportError(FE.Message.ToString());
                    }
                } 
            } catch (ArgumentOutOfRangeException AOORE) { ReportError(AOORE.Message.ToString());
            } catch (ArgumentNullException) {
            } catch (Exception E) { ReportError(E.Message.ToString());
            }
            return list;
        }

        #region Methods inside
        private static void InsertLeaTeamHeader(TeamLeagueModel model, HtmlNodeCollection ths) {
            model.RankHeader = ths.ElementAt(0).InnerText;
            model.TeamHeader = ths.ElementAt(1).InnerText;
            model.AmountHeader = ths.ElementAt(2).InnerText;
            model.WinHeader = ths.ElementAt(3).InnerText;
            model.DrawHeader = ths.ElementAt(4).InnerText;
            model.LoseHeader = ths.ElementAt(5).InnerText;
            model.ScoreBallHeader = ths.ElementAt(6).InnerText;
            model.LostBallHeader = ths.ElementAt(7).InnerText;
            model.NetBallHeader = ths.ElementAt(8).InnerText;
            model.IntegralHeader = ths.ElementAt(9).InnerText;
        }

        private static void InsertLeagueTeamModel(HtmlNode listItem, TeamLeagueModel model) {
            var tds = listItem.SelectNodes("td");
            model.Rank = Convert.ToUInt32(tds.ElementAt(0).InnerText);
            model.Team = tds.ElementAt(1).InnerText;
            model.Amount = Convert.ToUInt32(tds.ElementAt(2).InnerText);
            model.Win = Convert.ToUInt32(tds.ElementAt(3).InnerText);
            model.Draw = Convert.ToUInt32(tds.ElementAt(4).InnerText);
            model.Lose = Convert.ToUInt32(tds.ElementAt(5).InnerText);
            model.ScoreBall = Convert.ToUInt32(tds.ElementAt(6).InnerText);
            model.LostBall = Convert.ToUInt32(tds.ElementAt(7).InnerText);
            model.NetBall = Convert.ToInt32(tds.ElementAt(8).InnerText);
            model.Integral = Convert.ToUInt32(tds.ElementAt(9).InnerText);
            model.UpOrDown =
                listItem.Attributes["class"]==null? TeamLeagueModel.TopOrBottom.None:
                listItem.Attributes["class"].Value.Equals("top_rank") ? TeamLeagueModel.TopOrBottom.Top :
                listItem.Attributes["class"].Value.Equals("bottom_rank") ? TeamLeagueModel.TopOrBottom.Bottom :
                TeamLeagueModel.TopOrBottom.None;
            model.TeamIcon =
                string.IsNullOrEmpty(new Regex(@"\/.png").Match(tds.ElementAt(1).SelectSingleNode("img").Attributes["src"].Value).Value) ?
                new Uri(tds.ElementAt(1).SelectSingleNode("img").Attributes["src"].Value) :
                new Uri(DefaultImageFlagHost);
        }

        private static void InsertCupTeamModel(HtmlNode listItem, TeamLeagueModel model) {
            var ctds = listItem.SelectNodes("td");
            model.TopRankOrNot = 
                listItem.Attributes["class"]==null? TeamLeagueModel.TopOrBottom.None:
                listItem.Attributes["class"].Value.Equals("top_rank") ? TeamLeagueModel.TopOrBottom.Top :
                TeamLeagueModel.TopOrBottom.None;
            model.Time = ctds.ElementAt(0).Attributes["utc"] != null && !ctds.ElementAt(0).Attributes["utc"].Value .Equals("0")?
                new DateTime(1970, 1, 1, 8, 0, 0).AddSeconds(Convert.ToInt64(ctds.ElementAt(0).Attributes["utc"].Value)).ToString("yy-MM-dd HH:mm") :
                ctds.ElementAt(0).Attributes["utc"] != null && ctds.ElementAt(0).Attributes["utc"].Value.Equals("0")?
                "待定":"总分";
            model.Score = ctds.ElementAt(2).InnerText;
            model.AwayTeam = ctds.ElementAt(1).InnerText;
            model.HomeTeam = ctds.ElementAt(3).InnerText;
            model.AwayTeamIcon =
                string.IsNullOrEmpty(new Regex(@"\/.png").Match(ctds.ElementAt(1).SelectSingleNode("img").Attributes["src"].Value).Value) ?
                new Uri(ctds.ElementAt(1).SelectSingleNode("img").Attributes["src"].Value) :
                new Uri(DefaultImageFlagHost);
            model.HomeTeamIcon =
                string.IsNullOrEmpty(new Regex(@"\/.png").Match(ctds.ElementAt(3).SelectSingleNode("img").Attributes["src"].Value).Value) ?
                new Uri(ctds.ElementAt(3).SelectSingleNode("img").Attributes["src"].Value) :
                new Uri(DefaultImageFlagHost);
        }

        private static void InsertSocMemHeaderModel(SoccerMemberModel model, HtmlNodeCollection ths) {
            model.RankHeader = ths.ElementAt(0).InnerText;
            model.TeamHeader = ths.ElementAt(2).InnerText;
            model.MemberHeader = ths.ElementAt(1).InnerText;
            model.StatHeader = ths.ElementAt(3).InnerText;
        }

        private static void InsertSoccerMemberModel(HtmlNode listItem, SoccerMemberModel model) {
            var tds = listItem.SelectNodes("td");
            model.Rank = Convert.ToUInt32(tds.ElementAt(0).InnerText);
            model.Team = tds.ElementAt(2).InnerText;
            model.Member = tds.ElementAt(1).InnerText;
            model.Stat = Convert.ToUInt32(tds.ElementAt(3).InnerText);
            model.TeamIcon =
                 string.IsNullOrEmpty(new Regex(@"\/.png").Match(tds.ElementAt(2).SelectSingleNode("img").Attributes["src"].Value).Value) ?
                 new Uri(tds.ElementAt(2).SelectSingleNode("img").Attributes["src"].Value) :
                 new Uri(DefaultImageFlagHost);
            model.MemberIcon =
                 string.IsNullOrEmpty(new Regex(@"\/.png").Match(tds.ElementAt(1).SelectSingleNode("img").Attributes["src"].Value).Value) ?
                 new Uri(tds.ElementAt(1).SelectSingleNode("img").Attributes["src"].Value) :
                 new Uri(DefaultMenberHost);
        }
        #endregion

    }
}
