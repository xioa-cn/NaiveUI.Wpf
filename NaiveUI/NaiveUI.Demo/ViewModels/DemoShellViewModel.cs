using System.Windows;
using NaiveUI.Demo.Views.Pages;
using NaiveUI.NControls.Themes;

namespace NaiveUI.Demo.ViewModels;

public sealed class DemoShellViewModel : ViewModelBase
{
    private readonly HomePage homePage = new();
    private readonly DocsPage docsPage = new();
    private object currentPageView;
    private DemoPage currentPage;
    private string maximizeGlyph = "\uE922";

    public DemoShellViewModel()
    {
        currentPageView = homePage;
        currentPage = DemoPage.Home;
        VersionText = "1.0.2";
        ThemeManager.ThemeChanged += HandleThemeChanged;
    }

    private enum DemoPage
    {
        Home,
        Components
    }
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
        CurrentPageView = docsPage;
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

    private void RaiseSelectionChanged()
    {
        OnPropertyChanged(nameof(IsHomeSelected));
        OnPropertyChanged(nameof(IsComponentsSelected));
    }
}
