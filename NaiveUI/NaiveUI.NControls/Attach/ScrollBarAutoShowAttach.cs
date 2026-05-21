using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace NaiveUI.NControls.Attach;

public static class ScrollViewerAttach
    {
        #region 自动隐藏滚动条
        public static bool GetAutoHideScrollBar(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoHideScrollBarProperty);
        }

        public static void SetAutoHideScrollBar(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoHideScrollBarProperty, value);
        }

        public static readonly DependencyProperty AutoHideScrollBarProperty =
            DependencyProperty.RegisterAttached("AutoHideScrollBar", typeof(bool),
                typeof(ScrollViewerAttach), new PropertyMetadata(false, OnAutoHideChanged));

        private static void OnAutoHideChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ScrollViewer sv) return;
            if ((bool)e.NewValue)
            {
                sv.Loaded += ScrollViewer_Loaded;
                sv.ScrollChanged += ScrollViewer_ScrollChanged;
                sv.MouseEnter += ScrollViewer_MouseEnter;
                sv.MouseLeave += ScrollViewer_MouseLeave;
            }
            else
            {
                sv.Loaded -= ScrollViewer_Loaded;
                sv.ScrollChanged -= ScrollViewer_ScrollChanged;
                sv.MouseEnter -= ScrollViewer_MouseEnter;
                sv.MouseLeave -= ScrollViewer_MouseLeave;
            }
        }

        private static void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            var sv = sender as ScrollViewer;
            SetScrollBarOpacity(sv, 0);
        }

        private static void ScrollViewer_MouseEnter(object sender, RoutedEventArgs e)
        {
            var sv = sender as ScrollViewer;
            BeginFadeAnimation(sv, 1, 0.2);
        }

        private static void ScrollViewer_MouseLeave(object sender, RoutedEventArgs e)
        {
            var sv = sender as ScrollViewer;
            BeginFadeAnimation(sv, 0, 0.3, 1200);
        }

        private static void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var sv = sender as ScrollViewer;
            BeginFadeAnimation(sv, 1, 0.2);
            // 滚动停止延时隐藏
            sv.Dispatcher.BeginInvoke(new Action(() =>
            {
                BeginFadeAnimation(sv, 0, 0.3, 1500);
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        private static void SetScrollBarOpacity(ScrollViewer sv, double opacity)
        {
            var vsb = sv.Template.FindName("PART_VerticalScrollBar", sv) as ScrollBar;
            var hsb = sv.Template.FindName("PART_HorizontalScrollBar", sv) as ScrollBar;
            if (vsb != null) vsb.Opacity = opacity;
            if (hsb != null) hsb.Opacity = opacity;
        }

        private static void BeginFadeAnimation(ScrollViewer sv, double targetOpacity, double duration, int delayMs = 0)
        {
            var vsb = sv.Template.FindName("PART_VerticalScrollBar", sv) as ScrollBar;
            var hsb = sv.Template.FindName("PART_HorizontalScrollBar", sv) as ScrollBar;
            if (vsb == null || hsb == null) return;

            var story = new Storyboard();
            var delay = TimeSpan.FromMilliseconds(delayMs);
            var dur = TimeSpan.FromSeconds(duration);

            DoubleAnimation aniV = new()
            {
                To = targetOpacity,
                Duration = dur,
                BeginTime = delay
            };
            Storyboard.SetTarget(aniV, vsb);
            Storyboard.SetTargetProperty(aniV, new PropertyPath(UIElement.OpacityProperty));

            DoubleAnimation aniH = new()
            {
                To = targetOpacity,
                Duration = dur,
                BeginTime = delay
            };
            Storyboard.SetTarget(aniH, hsb);
            Storyboard.SetTargetProperty(aniH, new PropertyPath(UIElement.OpacityProperty));

            story.Children.Add(aniV);
            story.Children.Add(aniH);
            story.Begin();
        }
        #endregion
    }
