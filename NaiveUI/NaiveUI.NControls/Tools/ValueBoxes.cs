using System.Windows.Controls;
using System.Windows;

namespace NaiveUI.NControls.Tools
{
    internal static class ValueBoxes
    {
        internal static object TrueBox = true;

        internal static object FalseBox = false;

        internal static object VerticalBox = Orientation.Vertical;

        internal static object HorizontalBox = Orientation.Horizontal;

        internal static object VisibleBox = Visibility.Visible;

        internal static object CollapsedBox = Visibility.Collapsed;

        internal static object HiddenBox = Visibility.Hidden;

        internal static object Double01Box = 0.1;

        internal static object Double0Box = 0.0;

        internal static object Double1Box = 1.0;

        internal static object Double10Box = 10.0;

        internal static object Double20Box = 20.0;

        internal static object Double100Box = 100.0;

        internal static object Double200Box = 200.0;

        internal static object Double300Box = 300.0;

        internal static object DoubleNeg1Box = -1.0;

        internal static object Int0Box = 0;

        internal static object Int1Box = 1;

        internal static object Int2Box = 2;

        internal static object Int5Box = 5;

        internal static object Int99Box = 99;

        internal static object BooleanBox(bool value)
        {
            if (!value)
            {
                return FalseBox;
            }

            return TrueBox;
        }

        internal static object OrientationBox(Orientation value)
        {
            if (value != 0)
            {
                return VerticalBox;
            }

            return HorizontalBox;
        }

        internal static object VisibilityBox(Visibility value)
        {
            return value switch
            {
                Visibility.Visible => VisibleBox,
                Visibility.Hidden => HiddenBox,
                Visibility.Collapsed => CollapsedBox,
                _ => throw new ArgumentOutOfRangeException("value", value, null),
            };
        }
    }
}
