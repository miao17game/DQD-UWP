﻿using System;
using System . Diagnostics;
using System . IO;
using System . Threading . Tasks;
using Windows . Storage;

namespace DQD.Core. Helpers {
    /// <summary>
    /// 在整个解决方案中使用的Settings value字符串常量的集合 ///
    /// 此文件向所有项目共享
    /// </summary>
    public static class SettingsHelper {
        /// <summary>
        /// 函数读取设置值，读完之后将其清除(暂时不移除)
        /// </summary>
        public static object ReadSettingsValue(string key)  {
            Debug . WriteLine ( "\n读取状态值----->【 " + key + " 】" );
            if ( !ApplicationData . Current . LocalSettings . Values . ContainsKey ( key )) {
                Debug . WriteLine ( "---->【 不存在指定键值 】" );
                return null;
            } else {
                var value = ApplicationData . Current . LocalSettings . Values [ key ];
                Debug . WriteLine ( "----> 【 " + value . ToString ( ) + " 】" );
                return value;
            }
        }

        /// <summary>
        /// 设置保存键/值对 ///
        /// 如果它不存在，则创建
        /// </summary>
        public static void SaveSettingsValue(string key, object value) {
            Debug . WriteLine ( "\n存储数据【 " + key + " 】 -----> :【 " + ( value == null ? "不存在指定键值 】" : (value . ToString ( ) + " 】") ) );
            if ( !ApplicationData . Current . LocalSettings . Values . ContainsKey ( key )) {
                ApplicationData . Current . LocalSettings . Values . Add ( key , value );
            } else {
                ApplicationData . Current . LocalSettings . Values [ key ] = value;
            }
        }
    }
}
