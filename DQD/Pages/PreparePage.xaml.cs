using DQD.Core.Controls;
using DQD.Core.Helpers;
using DQD.Core.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace DQD.Net.Pages {

    public sealed partial class PreparePage : BasePage {
        private TranslateTransform translateT;
        DispatcherTimer WeocomeTimer;
        private bool isColorfulOrNot;
        private bool isLightOrNot;

        public PreparePage ( ) {
            translateT = this . RenderTransform as TranslateTransform;
            this . InitializeComponent ( );
            isColorfulOrNot = (bool?)SettingsHelper.ReadSettingsValue(SettingsConstants.IsColorfulOrNot) ?? false;
            isLightOrNot = (bool?)SettingsHelper.ReadSettingsValue(SettingsConstants.IsLigheOrNot) ?? false;
            if (StatusBarInit.IsTargetMobile()) { StatusBarInit.InitInnerMobileStatusBar(true);}
            StatusBarInit.InitDesktopStatusBar(false);
            StatusBarInit.InitMobileStatusBar(false);
            InitSliderTimer ( );
            OutIMG.BeginTime = new TimeSpan(0, 0, 0, 0, 500);
            OutREC.BeginTime = new TimeSpan(0, 0, 0, 0, 500);
            OutIMG.SpeedRatio = 0.1;
            OutREC.SpeedRatio = 0.1;
            OutIMG.Begin();
            OutREC.Begin();
        }

        private void InitSliderTimer ( ) {
            WeocomeTimer = new DispatcherTimer ( );
            WeocomeTimer . Tick += dispatcherTimer_TickSliderbarMP;
            WeocomeTimer . Interval = new TimeSpan ( 0 , 0 , 0 , 0 , 100 );
            WeocomeTimer . Start ( );
        }
        
        void dispatcherTimer_TickSliderbarMP ( object sender , object e ) {
            progressBar . Value += 120; 
            if ( progressBar.Value - progressBar.Maximum >-0.00001 ) {
                WeocomeTimer . Stop ( );
                SetAnimation ( );
            }
        }

        private void SetAnimation ( ) {
            var storyboard = new Storyboard ( );
            var doubleanimation = new DoubleAnimation ( ) { Duration = new Duration ( TimeSpan . FromMilliseconds ( 520 ) ) , From = translateT . Y , To = -this.ActualHeight };
            doubleanimation . EasingFunction = new CubicEase ( ) { EasingMode = EasingMode . EaseOut };
            doubleanimation . Completed += Doubleanimation_Completed;
            Storyboard . SetTarget ( doubleanimation , translateT );
            Storyboard . SetTargetProperty ( doubleanimation , "Y" );
            storyboard . Children . Add ( doubleanimation );
            storyboard . Begin ( );
        }

        private void Doubleanimation_Completed ( object sender , object e ) {
            this . Frame . Content = null;
            if ( translateT == null )
                this . RenderTransform = new TranslateTransform ( );
            translateT . Y = 0;
            RequestedTheme = isLightOrNot ? ElementTheme.Light : ElementTheme.Dark;
            MainPage.Current.ChangeStatusBar(isColorfulOrNot, isLightOrNot);
        }
    }
}
