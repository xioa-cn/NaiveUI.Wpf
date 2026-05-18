using System.Windows;

namespace NaiveUI.NControls.Themes;

public static class ThemeManager
{
    private static readonly Uri LightThemeUri = new("/NaiveUI.NControls;component/Themes/LightTheme.xaml", UriKind.Relative);
    private static readonly Uri DarkThemeUri = new("/NaiveUI.NControls;component/Themes/DarkTheme.xaml", UriKind.Relative);

    public static ThemeMode CurrentTheme { get; private set; } = ThemeMode.Light;

    public static event Action<ThemeMode>? ThemeChanged;

    public static void ApplyTheme(ThemeMode mode, Application? application = null)
    {
        application ??= Application.Current;
        if (application is null)
        {
            return;
        }

        var resources = application.Resources.MergedDictionaries;
        var existing = resources.FirstOrDefault(dictionary => dictionary.Contains("NaiveUI.Theme.Name"));
        if (existing is not null)
        {
            resources.Remove(existing);
        }

        resources.Add(new ResourceDictionary
        {
            Source = mode == ThemeMode.Dark ? DarkThemeUri : LightThemeUri
        });

        CurrentTheme = mode;
        ThemeChanged?.Invoke(mode);
    }

    public static void Toggle(Application? application = null)
    {
        ApplyTheme(CurrentTheme == ThemeMode.Light ? ThemeMode.Dark : ThemeMode.Light, application);
    }
}
