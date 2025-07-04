using System.Text;
using System.Windows.Threading;

namespace NaiveUI.NControls.Tools
{
    public static class DispatcherHelper
    {
        public static Dispatcher? UIDispatcher { get; private set; }

        public static void CheckBeginInvokeOnUI(Action action)
        {
            if (action != null)
            {
                CheckDispatcher();
                if (UIDispatcher!.CheckAccess())
                {
                    action();
                }
                else
                {
                    UIDispatcher.BeginInvoke(action);
                }
            }
        }

        private static void CheckDispatcher()
        {
            if (UIDispatcher == null)
            {
                StringBuilder stringBuilder = new("The DispatcherHelper is not initialized.");
                stringBuilder.AppendLine();
                stringBuilder.Append("Call DispatcherHelper.Initialize() in the static App constructor.");
                throw new InvalidOperationException(stringBuilder.ToString());
            }
        }

        public static DispatcherOperation RunAsync(Action action)
        {
            CheckDispatcher();
            return UIDispatcher!.BeginInvoke(action);
        }

        public static void Initialize()
        {
            if (UIDispatcher == null || !UIDispatcher.Thread.IsAlive)
            {
                UIDispatcher = Dispatcher.CurrentDispatcher;
            }
        }

        public static void Reset()
        {
            UIDispatcher = null;
        }
    }
}
