using System.Runtime.InteropServices;

namespace NaiveUI.NControls.Tools
{
    public static class InteropMethods
    {
        [DllImport("user32.dll")]
        internal static extern nint GetActiveWindow();
    }
}
