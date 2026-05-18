using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NaiveUI.Demo.Views.Shell;

public partial class TopNavBar : UserControl
{
    public static readonly DependencyProperty IsHomeSelectedProperty =
        DependencyProperty.Register(nameof(IsHomeSelected), typeof(bool), typeof(TopNavBar), new PropertyMetadata(false));

    public static readonly DependencyProperty IsComponentsSelectedProperty =
        DependencyProperty.Register(nameof(IsComponentsSelected), typeof(bool), typeof(TopNavBar), new PropertyMetadata(false));

    public static readonly DependencyProperty ThemeToggleTextProperty =
        DependencyProperty.Register(nameof(ThemeToggleText), typeof(string), typeof(TopNavBar), new PropertyMetadata("深色"));

    public static readonly DependencyProperty VersionTextProperty =
        DependencyProperty.Register(nameof(VersionText), typeof(string), typeof(TopNavBar), new PropertyMetadata("2.44.1"));

    public static readonly DependencyProperty MaximizeGlyphProperty =
        DependencyProperty.Register(nameof(MaximizeGlyph), typeof(string), typeof(TopNavBar), new PropertyMetadata("\uE922"));

    public TopNavBar()
    {
        InitializeComponent();
    }

    public event RoutedEventHandler? HomeRequested;

    public event RoutedEventHandler? ComponentsRequested;

    public event RoutedEventHandler? ThemeToggleRequested;

    public event RoutedEventHandler? MinimizeRequested;

    public event RoutedEventHandler? MaximizeRestoreRequested;

    public event RoutedEventHandler? CloseRequested;

    public event MouseButtonEventHandler? DragRequested;

    public bool IsHomeSelected
    {
        get => (bool)GetValue(IsHomeSelectedProperty);
        set => SetValue(IsHomeSelectedProperty, value);
    }

    public bool IsComponentsSelected
    {
        get => (bool)GetValue(IsComponentsSelectedProperty);
        set => SetValue(IsComponentsSelectedProperty, value);
    }

    public string ThemeToggleText
    {
        get => (string)GetValue(ThemeToggleTextProperty);
        set => SetValue(ThemeToggleTextProperty, value);
    }

    public string VersionText
    {
        get => (string)GetValue(VersionTextProperty);
        set => SetValue(VersionTextProperty, value);
    }

    public string MaximizeGlyph
    {
        get => (string)GetValue(MaximizeGlyphProperty);
        set => SetValue(MaximizeGlyphProperty, value);
    }

    private void HandleHomeClick(object sender, RoutedEventArgs e)
    {
        HomeRequested?.Invoke(this, e);
    }

    private void HandleComponentsClick(object sender, RoutedEventArgs e)
    {
        ComponentsRequested?.Invoke(this, e);
    }

    private void HandleThemeToggleClick(object sender, RoutedEventArgs e)
    {
        ThemeToggleRequested?.Invoke(this, e);
    }

    private void HandleMinimizeClick(object sender, RoutedEventArgs e)
    {
        MinimizeRequested?.Invoke(this, e);
    }

    private void HandleMaximizeRestoreClick(object sender, RoutedEventArgs e)
    {
        MaximizeRestoreRequested?.Invoke(this, e);
    }

    private void HandleCloseClick(object sender, RoutedEventArgs e)
    {
        CloseRequested?.Invoke(this, e);
    }

    private void HandleDragRegionMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is DependencyObject dependencyObject && FindAncestor<Button>(dependencyObject) is not null)
        {
            return;
        }

        DragRequested?.Invoke(this, e);
    }

    private static T? FindAncestor<T>(DependencyObject dependencyObject)
        where T : DependencyObject
    {
        var current = dependencyObject;
        while (current is not null)
        {
            if (current is T match)
            {
                return match;
            }

            current = System.Windows.Media.VisualTreeHelper.GetParent(current);
        }

        return null;
    }
}
