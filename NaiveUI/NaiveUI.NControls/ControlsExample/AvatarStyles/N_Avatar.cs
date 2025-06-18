using NaiveUI.NControls.Tools;
using System.Windows;
using System.Windows.Controls;

namespace NaiveUI.NControls.ControlsExample
{
    public enum AvatarBorder
    {
        Normal,
        Round
    }

    public class N_Avatar : Control
    {
        static N_Avatar() {
            ElementBase.DefaultStyle<N_Avatar>(DefaultStyleKeyProperty);
        }
        
        
        public double Size
        {
            get { return (double)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public static readonly DependencyProperty SizeProperty =
            ElementBase.Property<N_Avatar, double>(nameof(SizeProperty), 20);

        public object Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty ContentProperty =
            ElementBase.Property<N_Avatar, object>(nameof(ContentProperty), null);


        public AvatarBorder AvatarBorder
        {
            get { return (AvatarBorder)GetValue(AvatarBorderProperty); }
            set { SetValue(AvatarBorderProperty, value); }
        }

        public static readonly DependencyProperty AvatarBorderProperty =
            ElementBase.Property<N_Avatar, AvatarBorder>(nameof(AvatarBorderProperty), AvatarBorder.Normal);


        public string Badge
        {
            get { return (string)GetValue(BadgeProperty); }
            set { SetValue(BadgeProperty, value); }
        }

        public static readonly DependencyProperty BadgeProperty =
            ElementBase.Property<N_Avatar, string>(nameof(BadgeProperty), string.Empty);

    }
}
