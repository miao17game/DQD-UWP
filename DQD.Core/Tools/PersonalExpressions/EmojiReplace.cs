using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DQD.Core.Tools.PersonalExpressions {
    public static class EmojiReplace {
        public static string ToEmoji(string stringExpress) { return changeToEmoji(stringExpress); }

        static string changeToEmoji(string stringExpress) {
            var str = new Regex(@"\[.+?\]").Matches(stringExpress);
            foreach (var item in str) {
                switch ((item as Match).Value) {
                    case "[看不下去]":
                        stringExpress = new Regex(@"\[看不下去\]").Replace(stringExpress,"\ud83d\ude48");
                        break;
                    case "[加油]":
                        stringExpress = new Regex(@"\[加油\]").Replace(stringExpress, "\ud83d\udd92");
                        break;
                    case "[棒棒哒]":
                        stringExpress = new Regex(@"\[棒棒哒\]").Replace(stringExpress, "\ud83d\udc4f");
                        break;
                    case "[中国]":
                        stringExpress = new Regex(@"\[中国\]").Replace(stringExpress, "\ud83d\udc93");
                        break;
                    case "[打脸]":
                        stringExpress = new Regex(@"\[打脸\]").Replace(stringExpress, "\ud83d\ude44");
                        break;
                    case "[羞涩]":
                        stringExpress = new Regex(@"\[羞涩\]").Replace(stringExpress, "\ud83d\ude33");
                        break;
                    case "[卧槽]":
                        stringExpress = new Regex(@"\[卧槽\]").Replace(stringExpress, "\ud83d\ude31");
                        break;
                    case "[色色哒]":
                        stringExpress = new Regex(@"\[色色哒\]").Replace(stringExpress, "\ud83d\ude0d");
                        break;
                    case "[伐开心]":
                        stringExpress = new Regex(@"\[伐开心\]").Replace(stringExpress, "\ud83d\ude41");
                        break;
                    case "[搞毛]":
                        stringExpress = new Regex(@"\[搞毛\]").Replace(stringExpress, "\ud83d\ude12");
                        break;
                    case "[嘿嘿]":
                        stringExpress = new Regex(@"\[嘿嘿\]").Replace(stringExpress, "\ud83d\ude0f");
                        break;
                    case "[流鼻血]":
                        stringExpress = new Regex(@"\[流鼻血\]").Replace(stringExpress, "\ud83d\ude1a");
                        break;
                    case "[谄媚]":
                        stringExpress = new Regex(@"\[谄媚\]").Replace(stringExpress, "\ud83d\ude18");
                        break;
                    case "[伤心]":
                        stringExpress = new Regex(@"\[伤心\]").Replace(stringExpress, "\ud83d\ude30");
                        break;
                    case "[受伤]":
                        stringExpress = new Regex(@"\[受伤\]").Replace(stringExpress, "\ud83e\udd12");
                        break;
                    case "[XJBT]":
                        stringExpress = new Regex(@"\[XJBT\]").Replace(stringExpress, "\ud83c\udfd0XJB\ud83c\udfc3 ");
                        break;
                    case "[祈福]":
                        stringExpress = new Regex(@"\[祈福\]").Replace(stringExpress, "\ud83d\ude4f");
                        break;
                    case "[感动]":
                        stringExpress = new Regex(@"\[感动\]").Replace(stringExpress, "\ud83d\udc97");
                        break;
                    case "[大笑]":
                        stringExpress = new Regex(@"\[大笑\]").Replace(stringExpress, "\ud83d\ude06 \ud83e\udd12 ");
                        break;
                    case "[猴子]":
                        stringExpress = new Regex(@"\[猴子\]").Replace(stringExpress, "\ud83d\udc12");
                        break;
                    case "[学习]":
                        stringExpress = new Regex(@"\[学习\]").Replace(stringExpress, "\ud83d\udf93");
                        break;
                    case "[切]":
                        stringExpress = new Regex(@"\[切\]").Replace(stringExpress, "\ud83d\udc40");
                        break;
                    case "[红牌]":
                        stringExpress = new Regex(@"\[红牌\]").Replace(stringExpress, "\ud83d\udca3");
                        break;
                    case "[黄牌]":
                        stringExpress = new Regex(@"\[黄牌\]").Replace(stringExpress, "\ud83d\udca2");
                        break;
                    case "[愤怒]":
                        stringExpress = new Regex(@"\[愤怒\]").Replace(stringExpress, "\ud83d\ude24 \ud83d\udde1");
                        break;
                    case "[黑眼圈]":
                        stringExpress = new Regex(@"\[黑眼圈\]").Replace(stringExpress, "\ud83d\ude34");
                        break;
                    case "[金牌]":
                        stringExpress = new Regex(@"\[金牌\]").Replace(stringExpress, "\ud83e\udd47");
                        break;
                    case "[酷]":
                        stringExpress = new Regex(@"\[酷\]").Replace(stringExpress, "\ud83d\ude0e");
                        break;
                    case "[虐狗]":
                        stringExpress = new Regex(@"\[虐狗\]").Replace(stringExpress, "\ud83d\udc15");
                        break;
                    default: break;
                }
            }
            return stringExpress;
        }
    }
}
