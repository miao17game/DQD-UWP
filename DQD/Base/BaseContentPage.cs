using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace DQD.Net.Base {
    public class BaseContentPage : Page {

        public BaseContentPage() {
            transToSideGrid = this.RenderTransform as TranslateTransform;
            if (transToSideGrid == null) this.RenderTransform = transToSideGrid = new TranslateTransform();
        }

        private Storyboard storyToSideGrid = new Storyboard();
        public Storyboard storyToSideGridOut = new Storyboard();
        TranslateTransform transToSideGrid;
        DoubleAnimation doubleAnimation;

        public void InitStoryBoard() {
            doubleAnimation = new DoubleAnimation() {
                Duration = new Duration(TimeSpan.FromMilliseconds(220)),
                From = this.ActualWidth,
                To = 0,
            };
            doubleAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
            doubleAnimation.Completed += DoublAnimation_Completed;
            storyToSideGrid = new Storyboard();
            Storyboard.SetTarget(doubleAnimation, transToSideGrid);
            Storyboard.SetTargetProperty(doubleAnimation, "X");
            storyToSideGrid.Children.Add(doubleAnimation);
            doubleAnimation = new DoubleAnimation() {
                Duration = new Duration(TimeSpan.FromMilliseconds(220)),
                From = 0,
                To = -this.ActualWidth,
            };
            doubleAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
            doubleAnimation.Completed += DoublAnimationSlideOut_Completed;
            storyToSideGridOut = new Storyboard();
            Storyboard.SetTarget(doubleAnimation, transToSideGrid);
            Storyboard.SetTargetProperty(doubleAnimation, "X");
            storyToSideGridOut.Children.Add(doubleAnimation);
            storyToSideGrid.Begin();
        }

        private void DoublAnimationSlideOut_Completed(object sender, object e) {
            storyToSideGridOut.Stop();
            doubleAnimation.Completed -= DoublAnimation_Completed;
            MainPage.Current.SideGrid.Visibility = Visibility.Collapsed;
            MainPage.Current.ContFrame.Content = null;
        }

        private void DoublAnimation_Completed(object sender, object e) {
            storyToSideGrid.Stop();
            doubleAnimation.Completed -= DoublAnimation_Completed;
        }

        public void ClearThisPageByAnima() {
            storyToSideGridOut.Completed += (obj, args) => { GC.Collect(); };
            storyToSideGridOut.Begin();
        }

        public void ClearThisPage() { GC.Collect(); }

    }
}
