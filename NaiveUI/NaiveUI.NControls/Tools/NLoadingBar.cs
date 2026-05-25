using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using NaiveUI.NControls.Controls;

namespace NaiveUI.NControls.Tools;

public interface INLoadingBarApi
{
    void Start();

    void Finish();

    void Error();

    void Update(double progress);
}

public static class NLoadingBar
{
    public static INLoadingBarApi UseLoadingBar(Window? window = null)
    {
        return new LoadingBarApi(window, null);
    }

    public static INLoadingBarApi UseLoadingBar(NLoadingBarProvider provider)
    {
        return new LoadingBarApi(null, provider);
    }

    public static void SetGlobalProvider(NLoadingBarProvider? provider)
    {
        NLoadingBarManager.SetGlobalProvider(provider);
    }

    private sealed class LoadingBarApi(Window? window, NLoadingBarProvider? provider) : INLoadingBarApi
    {
        public void Start()
        {
            NLoadingBarManager.Start(window, provider);
        }

        public void Finish()
        {
            NLoadingBarManager.Finish(window, provider);
        }

        public void Error()
        {
            NLoadingBarManager.Error(window, provider);
        }

        public void Update(double progress)
        {
            NLoadingBarManager.Update(progress, window, provider);
        }
    }
}

internal static class NLoadingBarManager
{
    private static readonly Dictionary<Window, List<WeakReference<NLoadingBarProvider>>> Providers = new();
    private static WeakReference<NLoadingBarProvider>? globalProvider;
    private static readonly Dictionary<Window, FallbackLoadingBarHost> FallbackHosts = new();

    public static void SetGlobalProvider(NLoadingBarProvider? provider)
    {
        ExecuteOnUiThread(() =>
        {
            globalProvider = provider is null ? null : new WeakReference<NLoadingBarProvider>(provider);
        });
    }

    public static void RegisterProvider(NLoadingBarProvider provider)
    {
        ExecuteOnUiThread(() =>
        {
            var window = Window.GetWindow(provider);
            if (window is null)
            {
                return;
            }

            if (!Providers.TryGetValue(window, out var providers))
            {
                providers = [];
                Providers[window] = providers;
            }

            providers.RemoveAll(reference => !reference.TryGetTarget(out _));
            if (providers.Any(reference => reference.TryGetTarget(out var existing) && ReferenceEquals(existing, provider)))
            {
                return;
            }

            providers.Add(new WeakReference<NLoadingBarProvider>(provider));

            if (provider.IsGlobalProvider)
            {
                globalProvider = new WeakReference<NLoadingBarProvider>(provider);
            }
        });
    }

    public static void UnregisterProvider(NLoadingBarProvider provider)
    {
        ExecuteOnUiThread(() =>
        {
            var window = Window.GetWindow(provider);
            if (window is null || !Providers.TryGetValue(window, out var providers))
            {
                return;
            }

            providers.RemoveAll(reference => !reference.TryGetTarget(out var existing) || ReferenceEquals(existing, provider));
            if (providers.Count == 0)
            {
                Providers.Remove(window);
            }

            if (provider.IsGlobalProvider
                && globalProvider is not null
                && globalProvider.TryGetTarget(out var existingGlobal)
                && ReferenceEquals(existingGlobal, provider))
            {
                globalProvider = null;
            }
        });
    }

    public static void Start(Window? preferredWindow, NLoadingBarProvider? preferredProvider)
    {
        ExecuteOnUiThread(() => ResolveProvider(preferredWindow, preferredProvider)?.Start());
    }

    public static void Finish(Window? preferredWindow, NLoadingBarProvider? preferredProvider)
    {
        ExecuteOnUiThread(() => ResolveProvider(preferredWindow, preferredProvider)?.Finish());
    }

    public static void Error(Window? preferredWindow, NLoadingBarProvider? preferredProvider)
    {
        ExecuteOnUiThread(() => ResolveProvider(preferredWindow, preferredProvider)?.Error());
    }

    public static void Update(double progress, Window? preferredWindow, NLoadingBarProvider? preferredProvider)
    {
        ExecuteOnUiThread(() => ResolveProvider(preferredWindow, preferredProvider)?.Update(progress));
    }

    private static NLoadingBarIndicator? ResolveProvider(Window? preferredWindow, NLoadingBarProvider? preferredProvider)
    {
        if (preferredProvider is not null && preferredProvider.TryGetLoadingBar(out var explicitBar))
        {
            return explicitBar;
        }

        var window = preferredWindow ?? ResolveTargetWindow();
        if (window is null)
        {
            return null;
        }

        if (preferredProvider is null)
        {
            var globalIndicator = ResolveGlobalProvider();
            if (globalIndicator is not null)
            {
                return globalIndicator;
            }

            if (!FallbackHosts.TryGetValue(window, out var host) || host.IsDisposed)
            {
                host = FallbackLoadingBarHost.TryCreate(window);
                if (host is null)
                {
                    return null;
                }

                FallbackHosts[window] = host;
            }

            return host.Indicator;
        }

        if (!Providers.TryGetValue(window, out var providers))
        {
            return ResolveGlobalProvider();
        }

        providers.RemoveAll(reference => !reference.TryGetTarget(out _));
        foreach (var reference in providers)
        {
            if (!reference.TryGetTarget(out var provider))
            {
                continue;
            }

            if (provider.TryGetLoadingBar(out var indicator))
            {
                return indicator;
            }
        }

        return null;
    }

    private static NLoadingBarIndicator? ResolveGlobalProvider()
    {
        if (globalProvider is not null
            && globalProvider.TryGetTarget(out var provider)
            && provider.TryGetLoadingBar(out var indicator))
        {
            return indicator;
        }

        var mainWindow = Application.Current?.MainWindow;
        if (mainWindow is null)
        {
            return null;
        }

        var fallbackProvider = FindGlobalProvider(mainWindow);
        if (fallbackProvider is null || !fallbackProvider.TryGetLoadingBar(out var fallbackIndicator))
        {
            return null;
        }

        globalProvider = new WeakReference<NLoadingBarProvider>(fallbackProvider);
        return fallbackIndicator;
    }

    private static NLoadingBarProvider? FindGlobalProvider(DependencyObject root)
    {
        if (root is NLoadingBarProvider provider && provider.IsGlobalProvider)
        {
            return provider;
        }

        var childCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < childCount; i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
            var match = FindGlobalProvider(child);
            if (match is not null)
            {
                return match;
            }
        }

        return null;
    }

    private static Window? ResolveTargetWindow()
    {
        var application = Application.Current;
        return WindowHelper.GetActiveWindow()
               ?? application?.Windows.OfType<Window>().FirstOrDefault(item => item.IsActive)
               ?? application?.MainWindow
               ?? application?.Windows.OfType<Window>().FirstOrDefault(item => item.IsVisible);
    }

    private static void ExecuteOnUiThread(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null)
        {
            return;
        }

        if (dispatcher.CheckAccess())
        {
            action();
            return;
        }

        dispatcher.Invoke(action);
    }

    private sealed class FallbackLoadingBarHost : IDisposable
    {
        private readonly Popup popup;
        private readonly Window window;
        private readonly FrameworkElement placementTarget;

        private FallbackLoadingBarHost(Window window, FrameworkElement placementTarget)
        {
            this.window = window;
            this.placementTarget = placementTarget;

            Indicator = new NLoadingBarIndicator
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top
            };
            Indicator.SetBinding(FrameworkElement.WidthProperty, new Binding(nameof(FrameworkElement.ActualWidth)) { Source = placementTarget });

            popup = new Popup
            {
                AllowsTransparency = true,
                Focusable = false,
                IsHitTestVisible = false,
                Placement = PlacementMode.Relative,
                PlacementTarget = placementTarget,
                HorizontalOffset = 0d,
                VerticalOffset = 0d,
                StaysOpen = true,
                Child = Indicator,
                IsOpen = true
            };

            window.Closed += HandleWindowClosed;
        }

        public NLoadingBarIndicator Indicator { get; }

        public bool IsDisposed { get; private set; }

        public static FallbackLoadingBarHost? TryCreate(Window window)
        {
            var placementTarget = window.Content as FrameworkElement ?? window;
            return placementTarget is null ? null : new FallbackLoadingBarHost(window, placementTarget);
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;
            popup.IsOpen = false;
            window.Closed -= HandleWindowClosed;
        }

        private void HandleWindowClosed(object? sender, EventArgs e)
        {
            Dispose();
            FallbackHosts.Remove(window);
        }
    }
}
