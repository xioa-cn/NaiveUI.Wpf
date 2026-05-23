using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;
using NaiveUI.NControls.Controls;

namespace NaiveUI.NControls.Tools;

public enum NMessageType
{
    Info,
    Success,
    Warning,
    Error,
    Loading
}

public sealed class NMessageOptions
{
    public string Content { get; set; } = string.Empty;

    public NMessageType Type { get; set; } = NMessageType.Info;

    public int Duration { get; set; } = 3000;
}

public interface INMessageApi
{
    NMessageReactive Create(string content, NMessageType type = NMessageType.Info, int duration = 3000);

    NMessageReactive Create(NMessageOptions options);

    NMessageReactive Info(string content, int duration = 3000);

    NMessageReactive Success(string content, int duration = 3000);

    NMessageReactive Warning(string content, int duration = 3000);

    NMessageReactive Error(string content, int duration = 3000);

    NMessageReactive Loading(string content, int duration = 0);
}

public sealed class NMessageReactive
{
    private Action? destroyAction;
    private Action<string>? updateContentAction;
    private string content;

    internal NMessageReactive(string initialContent = "", Action? destroyAction = null, Action<string>? updateContentAction = null)
    {
        content = initialContent;
        this.destroyAction = destroyAction;
        this.updateContentAction = updateContentAction;
    }

    public bool IsDestroyed { get; private set; }

    public string Content
    {
        get => content;
        set
        {
            if (IsDestroyed || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            var normalized = value.Trim();
            content = normalized;
            updateContentAction?.Invoke(normalized);
        }
    }

    public void Destroy()
    {
        if (IsDestroyed)
        {
            return;
        }

        IsDestroyed = true;
        Interlocked.Exchange(ref updateContentAction, null);
        Interlocked.Exchange(ref destroyAction, null)?.Invoke();
    }
}

public static class NMessage
{
    public static INMessageApi UseMessage(Window? window = null)
    {
        return new MessageApi(window, null);
    }

    public static INMessageApi UseMessage(NMessageProvider provider)
    {
        return new MessageApi(null, provider);
    }

    private sealed class MessageApi(Window? window, NMessageProvider? provider) : INMessageApi
    {
        public NMessageReactive Create(string content, NMessageType type = NMessageType.Info, int duration = 3000)
        {
            return Create(new NMessageOptions
            {
                Content = content,
                Type = type,
                Duration = duration
            });
        }

        public NMessageReactive Create(NMessageOptions options)
        {
            return NMessageManager.Show(options, window, provider);
        }

        public NMessageReactive Info(string content, int duration = 3000)
        {
            return Create(content, NMessageType.Info, duration);
        }

        public NMessageReactive Success(string content, int duration = 3000)
        {
            return Create(content, NMessageType.Success, duration);
        }

        public NMessageReactive Warning(string content, int duration = 3000)
        {
            return Create(content, NMessageType.Warning, duration);
        }

        public NMessageReactive Error(string content, int duration = 3000)
        {
            return Create(content, NMessageType.Error, duration);
        }

        public NMessageReactive Loading(string content, int duration = 0)
        {
            return Create(content, NMessageType.Loading, duration);
        }
    }
}

internal static class NMessageManager
{
    private const double MessageHostTopOffset = 36d;
    private static readonly Dictionary<Window, List<WeakReference<NMessageProvider>>> Providers = new();
    private static readonly Dictionary<Window, FallbackMessageLayerHost> FallbackHosts = new();

    public static void RegisterProvider(NMessageProvider provider)
    {
        ExecuteOnUiThread(() =>
        {
            var window = Window.GetWindow(provider);
            if (window is not null)
            {
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

                providers.Add(new WeakReference<NMessageProvider>(provider));
            }
        });
    }

    public static void UnregisterProvider(NMessageProvider provider)
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
                return;
            }

            providers.RemoveAll(reference =>
                !reference.TryGetTarget(out var existing) || ReferenceEquals(existing, provider));

            if (providers.Count == 0)
            {
                Providers.Remove(window);
            }
        });
    }

    public static NMessageReactive Show(NMessageOptions options, Window? preferredWindow, NMessageProvider? preferredProvider)
    {
        if (string.IsNullOrWhiteSpace(options.Content))
        {
            return new NMessageReactive();
        }

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null)
        {
            return new NMessageReactive();
        }

        if (dispatcher.CheckAccess())
        {
            return ShowCore(options, preferredWindow, preferredProvider);
        }

        return dispatcher.Invoke(() => ShowCore(options, preferredWindow, preferredProvider));
    }

    public static Window? ResolveTargetWindow()
    {
        var application = Application.Current;
        return WindowHelper.GetActiveWindow()
               ?? application?.Windows.OfType<Window>().FirstOrDefault(item => item.IsActive)
               ?? application?.MainWindow
               ?? application?.Windows.OfType<Window>().FirstOrDefault(item => item.IsVisible);
    }

    private static NMessageReactive ShowCore(NMessageOptions options, Window? preferredWindow, NMessageProvider? preferredProvider)
    {
        var window = preferredWindow
                     ?? (preferredProvider is null ? null : Window.GetWindow(preferredProvider))
                     ?? ResolveTargetWindow();
        if (window is null)
        {
            return new NMessageReactive();
        }

        var normalizedOptions = NormalizeOptions(options);
        var layerLease = ResolveLayer(window, preferredProvider);
        if (layerLease is null)
        {
            return new NMessageReactive();
        }

        var instance = new MessageInstance(layerLease.Value.Host, layerLease.Value.ReleaseIfEmpty, normalizedOptions);
        instance.Show();
        return instance.Reactive;
    }

    private static MessageLayerLease? ResolveLayer(Window window, NMessageProvider? preferredProvider)
    {
        if (preferredProvider is not null && TryResolveProviderHost(preferredProvider, out var explicitProviderHost))
        {
            return new MessageLayerLease(explicitProviderHost, null);
        }

        if (TryResolveProviderHost(window, out var providerHost))
        {
            return new MessageLayerLease(providerHost, null);
        }

        if (!FallbackHosts.TryGetValue(window, out var host) || host.IsDisposed)
        {
            host = FallbackMessageLayerHost.TryCreate(window);
            if (host is null)
            {
                return null;
            }

            FallbackHosts[window] = host;
        }

        return new MessageLayerLease(host, () => ReleaseFallbackHost(window, host));
    }

    private static bool TryResolveProviderHost(Window window, out IMessageLayerHost providerHost)
    {
        providerHost = null!;

        if (!Providers.TryGetValue(window, out var providers))
        {
            return false;
        }

        providers.RemoveAll(reference => !reference.TryGetTarget(out var existing) || !existing.IsLoaded);
        if (providers.Count == 0)
        {
            Providers.Remove(window);
            return false;
        }

        var provider = providers
            .Select(reference => reference.TryGetTarget(out var existing) ? existing : null)
            .FirstOrDefault(existing => existing is not null);

        if (provider is null)
        {
            Providers.Remove(window);
            return false;
        }

        if (!provider.TryGetMessageHostPanel(out var panel))
        {
            return false;
        }

        providerHost = new ProviderMessageLayerHost(panel);
        return true;
    }

    private static bool TryResolveProviderHost(NMessageProvider provider, out IMessageLayerHost providerHost)
    {
        providerHost = null!;

        if (!provider.IsLoaded || !provider.TryGetMessageHostPanel(out var panel))
        {
            return false;
        }

        providerHost = new ProviderMessageLayerHost(panel);
        return true;
    }

    private static void ReleaseFallbackHost(Window window, FallbackMessageLayerHost host)
    {
        if (host.Count > 0 || !FallbackHosts.TryGetValue(window, out var existing) || !ReferenceEquals(existing, host))
        {
            return;
        }

        existing.Dispose();
        FallbackHosts.Remove(window);
    }

    private static NMessageOptions NormalizeOptions(NMessageOptions options)
    {
        return new NMessageOptions
        {
            Content = options.Content.Trim(),
            Type = options.Type,
            Duration = options.Duration < 0 ? 0 : options.Duration
        };
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

    private readonly record struct MessageLayerLease(IMessageLayerHost Host, Action? ReleaseIfEmpty);

    private sealed class MessageInstance
    {
        private readonly IMessageLayerHost host;
        private readonly Action? releaseIfEmpty;
        private readonly NMessageOptions options;
        private readonly Border root;
        private readonly TextBlock textBlock;
        private readonly TranslateTransform transform;
        private DispatcherTimer? timer;
        private bool isDestroyed;

        public MessageInstance(IMessageLayerHost host, Action? releaseIfEmpty, NMessageOptions options)
        {
            this.host = host;
            this.releaseIfEmpty = releaseIfEmpty;
            this.options = options;

            (root, textBlock, transform) = CreateMessageElement(options);
            Reactive = new NMessageReactive(options.Content, Destroy, UpdateContent);
        }

        public NMessageReactive Reactive { get; }

        public void Show()
        {
            host.Add(root);
            PlayShowAnimation(root, transform);

            if (options.Duration <= 0)
            {
                return;
            }

            timer = new DispatcherTimer(DispatcherPriority.Background, root.Dispatcher)
            {
                Interval = TimeSpan.FromMilliseconds(options.Duration)
            };
            timer.Tick += HandleTimerTick;
            timer.Start();
        }

        private void HandleTimerTick(object? sender, EventArgs e)
        {
            Destroy();
        }

        private void Destroy()
        {
            var dispatcher = root.Dispatcher;
            if (!dispatcher.CheckAccess())
            {
                dispatcher.Invoke(Destroy);
                return;
            }

            if (isDestroyed)
            {
                return;
            }

            isDestroyed = true;

            if (timer is not null)
            {
                timer.Stop();
                timer.Tick -= HandleTimerTick;
                timer = null;
            }

            PlayHideAnimation(root, transform, () =>
            {
                host.Remove(root);
                if (host.Count == 0)
                {
                    releaseIfEmpty?.Invoke();
                }
            });
        }

        private void UpdateContent(string content)
        {
            var dispatcher = root.Dispatcher;
            if (!dispatcher.CheckAccess())
            {
                dispatcher.Invoke(() => UpdateContent(content));
                return;
            }

            if (isDestroyed)
            {
                return;
            }

            textBlock.Text = content;
        }

        private static (Border Root, TextBlock TextBlock, TranslateTransform Transform) CreateMessageElement(NMessageOptions options)
        {
            var transform = new TranslateTransform(0, -12);

            var root = new Border
            {
                Margin = new Thickness(0, 0, 0, 12),
                Padding = new Thickness(14, 10, 14, 10),
                MaxWidth = 460,
                CornerRadius = new CornerRadius(10),
                Opacity = 0,
                RenderTransform = transform,
                Effect = new DropShadowEffect
                {
                    BlurRadius = 18,
                    ShadowDepth = 4,
                    Opacity = 0.16
                }
            };
            root.SetResourceReference(Control.BackgroundProperty, "Theme.Surface.0.Brush");
            root.SetResourceReference(Border.BorderBrushProperty, "Theme.Border.Brush");
            root.BorderThickness = new Thickness(1);

            var contentGrid = new Grid();
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var indicator = CreateIndicator(options.Type);
            var textBlock = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 13,
                Text = options.Content,
                TextWrapping = TextWrapping.Wrap
            };
            textBlock.SetResourceReference(TextBlock.ForegroundProperty, "Theme.Text.Primary.Brush");

            Grid.SetColumn(indicator, 0);
            Grid.SetColumn(textBlock, 2);
            contentGrid.Children.Add(indicator);
            contentGrid.Children.Add(textBlock);

            root.Child = contentGrid;
            return (root, textBlock, transform);
        }

        private static FrameworkElement CreateIndicator(NMessageType type)
        {
            if (type == NMessageType.Loading)
            {
                var container = new Grid
                {
                    Width = 14,
                    Height = 14,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var track = new Ellipse
                {
                    Width = 14,
                    Height = 14,
                    StrokeThickness = 2,
                    Opacity = 0.22
                };
                track.SetResourceReference(Shape.StrokeProperty, "Primary.First.Brush");

                var path = new Path
                {
                    Data = Geometry.Parse("M 7,1 A 6,6 0 1 1 1,7"),
                    Width = 14,
                    Height = 14,
                    Stretch = Stretch.Fill,
                    StrokeThickness = 2,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round
                };
                path.SetResourceReference(Shape.StrokeProperty, "Primary.First.Brush");

                var rotateTransform = new RotateTransform(0, 7, 7);
                path.RenderTransform = rotateTransform;
                rotateTransform.BeginAnimation(
                    RotateTransform.AngleProperty,
                    new DoubleAnimation(0, 360, TimeSpan.FromMilliseconds(900))
                    {
                        RepeatBehavior = RepeatBehavior.Forever
                    });

                container.Children.Add(track);
                container.Children.Add(path);
                return container;
            }

            if (type == NMessageType.Success && Application.Current.TryFindResource("CheckmarkCircle") is Geometry geometry)
            {
                var path = new Path
                {
                    Data = geometry,
                    Width = 16,
                    Height = 16,
                    Stretch = Stretch.Uniform,
                    VerticalAlignment = VerticalAlignment.Center
                };
                path.SetResourceReference(Shape.FillProperty, "Success.Second.Brush");
                return path;
            }

            var ellipse = new Ellipse
            {
                Width = 10,
                Height = 10,
                VerticalAlignment = VerticalAlignment.Center
            };
            ellipse.SetResourceReference(
                Shape.FillProperty,
                type switch
                {
                    NMessageType.Warning => "Warning.Second.Brush",
                    NMessageType.Error => "Error.Second.Brush",
                    _ => "Info.Second.Brush"
                });

            return ellipse;
        }

        private static void PlayShowAnimation(UIElement element, TranslateTransform transform)
        {
            var storyboard = new Storyboard();

            var fadeAnimation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180));
            Storyboard.SetTarget(fadeAnimation, element);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(UIElement.OpacityProperty));

            var slideAnimation = new DoubleAnimation(-12, 0, TimeSpan.FromMilliseconds(180));
            Storyboard.SetTarget(slideAnimation, transform);
            Storyboard.SetTargetProperty(slideAnimation, new PropertyPath(TranslateTransform.YProperty));

            storyboard.Children.Add(fadeAnimation);
            storyboard.Children.Add(slideAnimation);
            storyboard.Begin();
        }

        private static void PlayHideAnimation(UIElement element, TranslateTransform transform, Action completed)
        {
            var storyboard = new Storyboard();

            var fadeAnimation = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(160));
            Storyboard.SetTarget(fadeAnimation, element);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(UIElement.OpacityProperty));

            var slideAnimation = new DoubleAnimation(0, -8, TimeSpan.FromMilliseconds(160));
            Storyboard.SetTarget(slideAnimation, transform);
            Storyboard.SetTargetProperty(slideAnimation, new PropertyPath(TranslateTransform.YProperty));

            storyboard.Children.Add(fadeAnimation);
            storyboard.Children.Add(slideAnimation);
            storyboard.Completed += (_, _) => completed();
            storyboard.Begin();
        }
    }

    private interface IMessageLayerHost
    {
        int Count { get; }

        void Add(UIElement element);

        void Remove(UIElement element);
    }

    private sealed class ProviderMessageLayerHost(Panel panel) : IMessageLayerHost
    {
        public int Count => panel.Children.Count;

        public void Add(UIElement element)
        {
            panel.Children.Add(element);
        }

        public void Remove(UIElement element)
        {
            panel.Children.Remove(element);
        }
    }

    private sealed class FallbackMessageLayerHost : IMessageLayerHost, IDisposable
    {
        private readonly Window window;
        private readonly IMessageDisposableLayerHost layerHost;

        private FallbackMessageLayerHost(Window window, IMessageDisposableLayerHost layerHost)
        {
            this.window = window;
            this.layerHost = layerHost;
            window.Closed += HandleWindowClosed;
        }

        public bool IsDisposed { get; private set; }

        public int Count => layerHost.Count;

        public static FallbackMessageLayerHost? TryCreate(Window window)
        {
            if (window.Content is not FrameworkElement adornedElement)
            {
                return null;
            }

            adornedElement.ApplyTemplate();
            adornedElement.UpdateLayout();

            var layer = AdornerLayer.GetAdornerLayer(adornedElement)
                        ?? VisualHelper.GetChild<AdornerDecorator>(adornedElement)?.AdornerLayer
                        ?? VisualHelper.GetChild<AdornerDecorator>(window)?.AdornerLayer;
            if (layer is not null)
            {
                var adorner = new MessageHostAdorner(adornedElement);
                layer.Add(adorner);
                return new FallbackMessageLayerHost(window, new AdornerMessageLayerHost(layer, adorner));
            }

            return new FallbackMessageLayerHost(window, new PopupMessageLayerHost(adornedElement));
        }

        public void Add(UIElement element)
        {
            if (!IsDisposed)
            {
                layerHost.Add(element);
            }
        }

        public void Remove(UIElement element)
        {
            if (!IsDisposed)
            {
                layerHost.Remove(element);
            }
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;
            window.Closed -= HandleWindowClosed;
            layerHost.Dispose();
        }

        private void HandleWindowClosed(object? sender, EventArgs e)
        {
            Dispose();
        }
    }

    private interface IMessageDisposableLayerHost : IMessageLayerHost, IDisposable;

    private sealed class AdornerMessageLayerHost(AdornerLayer layer, MessageHostAdorner adorner) : IMessageDisposableLayerHost
    {
        public int Count => adorner.Count;

        public void Add(UIElement element)
        {
            adorner.Add(element);
        }

        public void Remove(UIElement element)
        {
            adorner.Remove(element);
        }

        public void Dispose()
        {
            layer.Remove(adorner);
        }
    }

    private sealed class PopupMessageLayerHost : IMessageDisposableLayerHost
    {
        private readonly Popup popup;
        private readonly StackPanel stackPanel;

        public PopupMessageLayerHost(FrameworkElement placementTarget)
        {
            stackPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(24, MessageHostTopOffset, 24, 0),
                IsHitTestVisible = false
            };

            var root = new Grid
            {
                Background = Brushes.Transparent,
                IsHitTestVisible = false
            };

            BindingOperations.SetBinding(
                root,
                FrameworkElement.WidthProperty,
                new Binding(nameof(FrameworkElement.ActualWidth))
                {
                    Source = placementTarget
                });

            root.Children.Add(stackPanel);

            popup = new Popup
            {
                AllowsTransparency = true,
                Child = root,
                IsHitTestVisible = false,
                Placement = PlacementMode.Relative,
                PlacementTarget = placementTarget,
                StaysOpen = true,
                IsOpen = true
            };
        }

        public int Count => stackPanel.Children.Count;

        public void Add(UIElement element)
        {
            stackPanel.Children.Add(element);
        }

        public void Remove(UIElement element)
        {
            stackPanel.Children.Remove(element);
        }

        public void Dispose()
        {
            popup.IsOpen = false;
            popup.Child = null;
        }
    }

    private sealed class MessageHostAdorner : Adorner
    {
        private readonly VisualCollection visuals;
        private readonly Grid root;
        private readonly StackPanel stackPanel;

        public MessageHostAdorner(UIElement adornedElement) : base(adornedElement)
        {
            IsHitTestVisible = false;

            visuals = new VisualCollection(this);
            root = new Grid();
            stackPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(24, MessageHostTopOffset, 24, 0)
            };

            root.Children.Add(stackPanel);
            visuals.Add(root);
        }

        public int Count => stackPanel.Children.Count;

        public void Add(UIElement element)
        {
            stackPanel.Children.Add(element);
        }

        public void Remove(UIElement element)
        {
            stackPanel.Children.Remove(element);
        }

        protected override int VisualChildrenCount => visuals.Count;

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= visuals.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return visuals[index];
        }

        protected override Size MeasureOverride(Size constraint)
        {
            root.Measure(constraint);
            return constraint;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            root.Arrange(new Rect(finalSize));
            return finalSize;
        }
    }
}
