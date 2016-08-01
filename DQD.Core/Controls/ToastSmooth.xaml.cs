using System;
using System . Collections . Generic;
using System . IO;
using System . Linq;
using System . Runtime . InteropServices . WindowsRuntime;
using Windows . Foundation;
using Windows . Foundation . Collections;
using Windows . UI . Xaml;
using Windows . UI . Xaml . Controls;
using Windows . UI . Xaml . Controls . Primitives;
using Windows . UI . Xaml . Data;
using Windows . UI . Xaml . Input;
using Windows . UI . Xaml . Media;
using Windows . UI . Xaml . Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace DQD.Core. Controls {
    public sealed partial class ToastSmooth : UserControl {
        private Popup DialogPopup;

        private string TextContent;
        private TimeSpan WholeTime;

        public ToastSmooth ( ) {
            this . InitializeComponent ( );
            DialogPopup = new Popup ( );
            this . Width = Window . Current . Bounds .Width;
            this . Height = Window . Current . Bounds . Height;
            DialogPopup . Child = this;
            this . Loaded += NotifyPopup_Loaded;
            this . Unloaded += NotifyPopup_Unloaded;
        }

        /// <summary>
        /// 构造特定时间的Toast
        /// </summary>
        /// <param name="content"></param>
        /// <param name="showTime"></param>
        public ToastSmooth ( string content , TimeSpan showTime ) : this() {
            this . TextContent = content;
            this . WholeTime = showTime;
        }

        /// <summary>
        /// 默认构造两秒的Toast
        /// </summary>
        /// <param name="content"></param>
        public ToastSmooth ( string content ) : this(content, TimeSpan.FromSeconds(2)) { }

        public void Show ( ) {
            this . DialogPopup . IsOpen = true;
        }

        private void NotifyPopup_Loaded ( object sender , RoutedEventArgs e ) {
            this . tbNotify . Text = TextContent;
            this . In . Begin ( );
            this . In . Completed += SbIn_Completed;
            Window . Current . SizeChanged += Current_SizeChanged;
        }

        private void SbIn_Completed ( object sender , object e ) {
            this . Out . BeginTime = this . WholeTime;
            this . Out . Completed += SbOut_Completed;
            this . Out . Begin ( );
        }

        private void SbOut_Completed ( object sender , object e ) {
            this . DialogPopup . IsOpen = false;
        }

        private void Current_SizeChanged ( object sender , Windows . UI . Core . WindowSizeChangedEventArgs e ) {
            this . Width = e . Size . Width;
            this . Height = e . Size . Height;
        }

        private void NotifyPopup_Unloaded ( object sender , RoutedEventArgs e ) {
            Window . Current . SizeChanged -= Current_SizeChanged;
        }
    }
}
