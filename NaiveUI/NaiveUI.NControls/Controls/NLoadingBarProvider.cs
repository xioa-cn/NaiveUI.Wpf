using System.Windows;
using System.Windows.Controls;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

[TemplatePart(Name = LoadingBarPartName, Type = typeof(NLoadingBarIndicator))]
public class NLoadingBarProvider : ContentControl
{
    private const string LoadingBarPartName = "PART_LoadingBar";
    private NLoadingBarIndicator? loadingBarPart;

    static NLoadingBarProvider()
    {
        ElementBase.DefaultStyle<NLoadingBarProvider>(DefaultStyleKeyProperty);
    }

    public NLoadingBarProvider()
    {
        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;
    }

    public bool IsGlobalProvider
    {
        get => (bool)GetValue(IsGlobalProviderProperty);
        set => SetValue(IsGlobalProviderProperty, value);
    }

    public static readonly DependencyProperty IsGlobalProviderProperty =
        ElementBase.Property<NLoadingBarProvider, bool>(nameof(IsGlobalProviderProperty), false);

    public double BarHeight
    {
        get => (double)GetValue(BarHeightProperty);
        set => SetValue(BarHeightProperty, value);
    }

    public static readonly DependencyProperty BarHeightProperty =
        ElementBase.Property<NLoadingBarProvider, double>(nameof(BarHeightProperty), 2.5d);

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        loadingBarPart = GetTemplateChild(LoadingBarPartName) as NLoadingBarIndicator;
    }

    public INLoadingBarApi UseLoadingBar()
    {
        return NLoadingBar.UseLoadingBar(this);
    }

    internal bool TryGetLoadingBar(out NLoadingBarIndicator indicator)
    {
        ApplyTemplate();
        indicator = loadingBarPart!;
        return indicator is not null;
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        NLoadingBarManager.RegisterProvider(this);
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        NLoadingBarManager.UnregisterProvider(this);
    }
}
