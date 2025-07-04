namespace NaiveUI.NControls.Tools
{
    public static class WindowHelper
    {
        public static System.Windows.Window? GetActiveWindow()
        {
            nint activeWindow = InteropMethods.GetActiveWindow();
            return System.Windows.Application.Current.Windows.OfType<System.Windows.Window>()
                .FirstOrDefault((System.Windows.Window x) => x.GetHandle() == activeWindow);
        }

        public static nint GetHandle(this System.Windows.Window window)
        {
            return new System.Windows.Interop.WindowInteropHelper(window).EnsureHandle();
        }
    }
}
