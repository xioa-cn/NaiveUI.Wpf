using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using NaiveUI.Demo.Services;
using NaiveUI.Demo.Views.Pages;
using NaiveUI.NControls.Themes;

namespace NaiveUI.Demo.ViewModels;

public sealed class DemoShellViewModel : INotifyPropertyChanged
{
    private readonly Dictionary<string, Func<object>> componentPageFactories;
    private readonly Dictionary<string, object> componentPageCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly HomePage homePage = new();
    private object currentPageView;
    private DemoPage currentPage;
    private string currentComponentKey = "button";
    private string maximizeGlyph = "\uE922";
    private static bool IsInDesignMode => DesignerProperties.GetIsInDesignMode(new DependencyObject());

    public DemoShellViewModel()
    {
        componentPageFactories = new Dictionary<string, Func<object>>(StringComparer.OrdinalIgnoreCase)
        {
            ["button"] = static () => new ButtonDocsPage(),
            ["avatar"] = static () => new AvatarDocsPage(),
            ["card"] = static () => new CardDocsPage()
        };

        currentPageView = homePage;
        currentPage = DemoPage.Home;
        VersionText = "1.0.1";
        ThemeManager.ThemeChanged += HandleThemeChanged;
        DemoNavigationService.ComponentRequested += HandleComponentRequested;

        if (!IsInDesignMode)
        {
            _ = ResolveComponentPage(currentComponentKey);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public object CurrentPageView
    {
        get => currentPageView;
        private set => SetProperty(ref currentPageView, value);
    }

    public bool IsHomeSelected => currentPage == DemoPage.Home;

    public bool IsComponentsSelected => currentPage == DemoPage.Components;

    public string ThemeToggleText => ThemeManager.CurrentTheme == ThemeMode.Dark ? "浅色" : "深色";

    public string VersionText { get; }

    public string MaximizeGlyph
    {
        get => maximizeGlyph;
        private set => SetProperty(ref maximizeGlyph, value);
    }

    public void ShowHomePage()
    {
        currentPage = DemoPage.Home;
        CurrentPageView = homePage;
        RaiseSelectionChanged();
    }

    public void ShowComponentsPage()
    {
        currentPage = DemoPage.Components;
        CurrentPageView = ResolveComponentPage(currentComponentKey);
        RaiseSelectionChanged();
    }

    public void ToggleTheme()
    {
        ThemeManager.Toggle(Application.Current);
        OnPropertyChanged(nameof(ThemeToggleText));
    }

    public void SyncWindowState(WindowState state)
    {
        MaximizeGlyph = state == WindowState.Maximized ? "\uE923" : "\uE922";
    }

    private void HandleThemeChanged(ThemeMode mode)
    {
        OnPropertyChanged(nameof(ThemeToggleText));
    }

    private void HandleComponentRequested(string componentKey)
    {
        if (!componentPageFactories.ContainsKey(componentKey))
        {
            return;
        }

        currentComponentKey = componentKey;
        currentPage = DemoPage.Components;
        CurrentPageView = ResolveComponentPage(componentKey);
        RaiseSelectionChanged();
    }

    private object ResolveComponentPage(string componentKey)
    {
        if (componentPageCache.TryGetValue(componentKey, out var existingPage))
        {
            return existingPage;
        }

        if (!componentPageFactories.TryGetValue(componentKey, out var factory))
        {
            factory = componentPageFactories["button"];
            componentKey = "button";
        }

        var page = factory();
        componentPageCache[componentKey] = page;
        return page;
    }

    private void RaiseSelectionChanged()
    {
        OnPropertyChanged(nameof(IsHomeSelected));
        OnPropertyChanged(nameof(IsComponentsSelected));
    }

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        OnPropertyChanged(propertyName);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private enum DemoPage
    {
        Home,
        Components
    }
}
