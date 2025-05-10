using System.Windows;
using System.Windows.Media;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.ControlsExample
{
    public partial class CardElement : System.Windows.Controls.Control
    {
        public object ContentElement
        {
            get { return (object)GetValue(ContentElementProperty); }
            set { SetValue(ContentElementProperty, value); }
        }

        public static readonly DependencyProperty ContentElementProperty =
            ElementBase.Property<CardElement, object>(nameof(ContentElementProperty), "");


        public object HeaderElement
        {
            get { return (object)GetValue(HeaderElementProperty); }
            set { SetValue(HeaderElementProperty, value); }
        }

        public static readonly DependencyProperty HeaderElementProperty =
            ElementBase.Property<CardElement, object>(nameof(HeaderElementProperty), "Header");


        public object? FooterElement
        {
            get { return (object?)GetValue(FooterElementProperty); }
            set { SetValue(FooterElementProperty, value); }
        }

        public static readonly DependencyProperty FooterElementProperty =
            ElementBase.Property<CardElement, object?>(nameof(FooterElementProperty), null);


        public Color BoderColor
        {
            get { return (Color)GetValue(BoderColorProperty); }
            set { SetValue(BoderColorProperty, value); }
        }

        public static readonly DependencyProperty BoderColorProperty =
            ElementBase.Property<CardElement, Color>(nameof(BoderColorProperty), Colors.Gray);

        public Brush FooterBackgroundBrush
        {
            get { return (Brush)GetValue(FooterBackgroundBrushProperty); }
            set { SetValue(FooterBackgroundBrushProperty, value); }
        }

        public static readonly DependencyProperty FooterBackgroundBrushProperty =
            ElementBase.Property<CardElement, Brush>(nameof(FooterBackgroundBrushProperty), Brushes.Gray);
    }
}