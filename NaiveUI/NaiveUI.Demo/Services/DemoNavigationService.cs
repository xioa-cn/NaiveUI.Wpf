namespace NaiveUI.Demo.Services;

internal static class DemoNavigationService
{
    public static event Action<string>? ComponentRequested;

    public static void RequestComponent(string componentKey)
    {
        if (string.IsNullOrWhiteSpace(componentKey))
        {
            return;
        }

        ComponentRequested?.Invoke(componentKey);
    }
}
