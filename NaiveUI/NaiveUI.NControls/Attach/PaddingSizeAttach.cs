using NaiveUI.NControls.ControlsExample;
using NaiveUI.NControls.Tools;
using System.Windows;
using System.Windows.Controls;


namespace NaiveUI.NControls.Attach
{
    public enum PSize
    {
        /// <summary>
        /// 小小
        /// </summary>
        Tiny,
        /// <summary>
        /// 小
        /// </summary>
        Small,
        /// <summary>
        /// 不小
        /// </summary>
        Medium,
        /// <summary>
        /// 不不小
        /// </summary>
        Large,
    }

    public class PaddingSizeAttach
    {
        public static PSize GetPaddingSize(DependencyObject obj)
        {
            return (PSize)obj.GetValue(PaddingSizeProperty);
        }

        public static void SetPaddingSize(DependencyObject obj, PSize value)
        {
            obj.SetValue(PaddingSizeProperty, value);
        }

        // Using a DependencyProperty as the backing store for PaddingSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PaddingSizeProperty =       
            ElementBase.PropertyAttach<PaddingSizeAttach, PSize?>(nameof(PaddingSizeProperty), default(PSize),OnPropertyValueChange);

        private static void OnPropertyValueChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button btn && e.NewValue is PSize p)
            {
                var size = p switch
                {
                    PSize.Tiny => 3,
                    PSize.Small => 10,
                    PSize.Medium => 20,
                    PSize.Large => 40,
                };


                btn.Padding = new Thickness(size, size / 2, size, size / 2);
                btn.Width = double.NaN;
                btn.Height = double.NaN;
            }
            if(d is N_Tag nTag && e.NewValue is PSize pTag)
            {
                var size = pTag switch
                {
                    PSize.Tiny => 3,
                    PSize.Small => 10,
                    PSize.Medium => 20,
                    PSize.Large => 40,
                };
                nTag.Padding = new Thickness(size, size / 2, size, size / 2);
                nTag.Width = double.NaN;
                nTag.Height = double.NaN;
            }
        }
    }
}
