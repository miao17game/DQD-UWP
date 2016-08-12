using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using Windows . UI;
using Windows . UI . ViewManagement;
using Windows . Foundation;
using Windows . Foundation . Collections;
using Windows . System . Profile;
using Windows . UI . Core;
using Windows . UI . Xaml;
using Windows . UI . Xaml . Controls;
using Windows . UI . Xaml . Controls . Primitives;
using Windows . UI . Xaml . Data;
using Windows . UI . Xaml . Input;
using Windows . UI . Xaml . Media;
using Windows . UI . Xaml . Navigation;

namespace DQD.Core. Tools {
    /// <summary>
    /// 设置Mobile和Desktop的状态栏风格
    /// </summary>
    public static class StatusBarInit {
        /// <summary>
        /// 为准备画面初始化Desktop任务栏
        /// </summary>
        public static void InitDesktopStatusBarToPrepare ( ) {
            SetTitleBarSelfView ( 0 , 27 , 154 , 239 , Colors . White , Colors . LightGray , Colors . Gray );
            SetTitleBarButtonSelfView ( 0 , 27 , 154 , 239 , Colors . White );
            SetTitleBarButtonHPIView ( Colors . SteelBlue , Colors . White , Colors . SteelBlue , Colors . Black , Colors . DarkGray , Colors . Gray );
        }
        /// <summary>
        /// 为准备画面初始化Mobile任务栏
        /// </summary>
        public static void InitMobileStatusBarToPrepare ( ) {
            if ( Windows . Foundation . Metadata . ApiInformation . IsTypePresent ( "Windows.UI.ViewManagement.StatusBar" ) ) {
                StatusBar statusBar = StatusBar . GetForCurrentView ( );
                statusBar . BackgroundColor = Color . FromArgb ( 0 , 27 , 154 , 239 );
                statusBar . ForegroundColor = Colors . White;
                statusBar . BackgroundOpacity = 1;
            }
        }

        /// <summary>
        /// 初始化Desktop任务栏
        /// </summary>
        /// <param name="IsLightTheme">是否为亮主题</param>
        public static void InitDesktopStatusBar ( bool IsLightTheme ) {
            if ( !IsLightTheme ) {
                SetTitleBarSelfView ( 0 , 51, 187, 115, Colors . White , Colors . LightGray , Colors . Gray );
                SetTitleBarButtonSelfView ( 0 , 51, 187, 115, Colors .White);
                SetTitleBarButtonHPIView ( Colors . SteelBlue , Colors . White , Colors .SteelBlue, Colors . Black , Colors . DarkGray , Colors . Gray );
            } else {
                SetTitleBarSelfView ( 0 , 51, 187, 85 , Colors . White , Colors . LightGray , Colors . Gray );
                SetTitleBarButtonSelfView ( 0 , 51, 187, 85, Colors . White );
                SetTitleBarButtonHPIView ( Colors .SteelBlue, Colors . White , Colors .SteelBlue, Colors . Black , Colors . DarkGray , Colors . Gray );
            }
        }

        /// <summary>
        /// 初始化Mobile任务栏
        /// </summary>
        /// <param name="IsLightTheme">是否为亮主题</param>
        public static void InitMobileStatusBar ( bool IsLightTheme ) {
            if ( Windows . Foundation . Metadata . ApiInformation . IsTypePresent ( "Windows.UI.ViewManagement.StatusBar" ) ) {
                StatusBar statusBar = StatusBar . GetForCurrentView ( );
                if ( !IsLightTheme ) {
                    statusBar . BackgroundColor = Color . FromArgb ( 0 , 51, 187, 115);
                    statusBar . ForegroundColor = Colors . White;
                } else {
                    statusBar . BackgroundColor = Color . FromArgb ( 0 , 51, 187, 85);
                    statusBar . ForegroundColor = Colors . White;
                }
                statusBar . BackgroundOpacity = 1;
            }
        }

        /// <summary>
        ///  设置任务栏按钮特殊风格 ///
        ///  1）指针上方时的背景 2）前景
        ///  3）按钮按下的背景 4）前景
        ///  5）失去焦点时的背景 6）前景
        /// </summary>
        /// <param name="AppView">应用程序视图</param>
        /// <param name="HoverBack">指针上方时的背景</param>
        /// <param name="HoverFore">指针上方时的前景</param>
        /// <param name="PrsdBack">按钮按下的背景</param>
        /// <param name="PrsdFore">按钮按下的前景</param>
        /// <param name="InacBack">失去焦点时的背景</param>
        /// <param name="InacFore">失去焦点时的前景</param>
        public static void SetTitleBarButtonHPIView ( Color HoverBack , Color HoverFore , Color PrsdBack , Color PrsdFore , Color InacBack , Color InacFore ) {
            var AppView = ApplicationView . GetForCurrentView ( );
            AppView . TitleBar . ButtonHoverBackgroundColor = HoverBack;
            AppView . TitleBar . ButtonHoverForegroundColor = HoverFore;
            AppView . TitleBar . ButtonPressedBackgroundColor = PrsdBack;
            AppView . TitleBar . ButtonPressedForegroundColor = PrsdFore;
            AppView . TitleBar . ButtonInactiveBackgroundColor = InacBack;
            AppView . TitleBar . ButtonInactiveForegroundColor = InacFore;
        }

        /// <summary>
        /// 设置任务栏按钮初始风格 ///
        /// 分别设置ARGB和前景色
        /// </summary>
        /// <param name="AppView">应用试视图</param>
        /// <param name="alpha">透明度</param>
        /// <param name="R">红</param>
        /// <param name="G">绿</param>
        /// <param name="B">蓝</param>
        /// <param name="ForeColor">前景色</param>
        public static void SetTitleBarButtonSelfView ( byte alpha , byte R , byte G , byte B , Color ForeColor ) {
            var AppView = ApplicationView . GetForCurrentView ( );
            AppView . TitleBar . ButtonBackgroundColor = Color . FromArgb ( alpha , R , G , B );
            AppView . TitleBar . ButtonForegroundColor = ForeColor;
        }

        /// <summary>
        /// 设置任务栏按钮初始风格
        /// 1）分别设置ARGB和前景色
        /// 2）设置失去焦点的背景和前景
        /// </summary>
        /// <param name="AppView"></param>
        /// <param name="alpha"></param>
        /// <param name="R"></param>
        /// <param name="G"></param>
        /// <param name="B"></param>
        /// <param name="ForeColor"></param>
        /// <param name="InacBack"></param>
        /// <param name="InacFore"></param>
        public static void SetTitleBarSelfView ( byte alpha , byte R , byte G , byte B , Color ForeColor , Color InacBack , Color InacFore ) {
            var AppView = ApplicationView . GetForCurrentView ( );
            AppView . TitleBar . BackgroundColor = Color . FromArgb ( alpha , R , G , B );
            AppView . TitleBar . ForegroundColor = ForeColor;
            AppView . TitleBar . InactiveBackgroundColor = InacBack;
            AppView . TitleBar . InactiveForegroundColor = InacFore;
        }
    }
}
