using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NaiveUI.NControls.Tools;

public static class NElMessage
{
    private const int DefaultDuration = 3000;
    private static readonly Dictionary<Window, MessageHost> Hosts = new();

    public static void Show(string message, string type = "info", int duration = DefaultDuration, Window? window = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null)
        {
            return;
        }

        if (dispatcher.CheckAccess())
        {
            ShowCore(message, type, duration, window);
            return;
        }

        _ = dispatcher.BeginInvoke(() => ShowCore(message, type, duration, window), DispatcherPriority.Normal);
    }

    public static void Success(string message, int duration = DefaultDuration, Window? window = null)
    {
        Show(message, "success", duration, window);
    }

    public static void Warning(string message, int duration = DefaultDuration, Window? window = null)
    {
        Show(message, "warning", duration, window);
    }

    public static void Error(string message, int duration = DefaultDuration, Window? window = null)
    {
        Show(message, "error", duration, window);
    }

    public static void Info(string message, int duration = DefaultDuration, Window? window = null)
    {
        Show(message, "info", duration, window);
    }

    private static void ShowCore(string message, string type, int duration, Window? window)
    {
        window ??= ResolveTargetWindow();
        if (window is null)
        {
            return;
        }

        if (!Hosts.TryGetValue(window, out var host) || host.IsDisposed)
        {
            host = MessageHost.TryCreate(window, RemoveHost);
            if (host is null)
            {
                return;
            }

            Hosts[window] = host;
        }

        host.Show(message, MessageStyle.From(type), duration > 0 ? duration : DefaultDuration);
    }

    private static Window? ResolveTargetWindow()
    {
        var application = Application.Current;
        return WindowHelper.GetActiveWindow()
               ?? application?.Windows.OfType<Window>().FirstOrDefault(item => item.IsActive)
               ?? application?.MainWindow
               ?? application?.Windows.OfType<Window>().FirstOrDefault(item => item.IsVisible);
    }

    private static void RemoveHost(Window window)
    {
        Hosts.Remove(window);
    }

    private readonly record struct MessageStyle(string AccentBrushKey)
    {
        public static MessageStyle From(string type)
        {
            return type.ToLowerInvariant() switch
            {
                "success" => new MessageStyle("Success.Second.Brush"),
                "warning" => new MessageStyle("Warning.Second.Brush"),
                "error" => new MessageStyle("Error.Second.Brush"),
                _ => new MessageStyle("Info.Second.Brush")
            };
        }
    }

    private sealed class MessageHost
    {
        private readonly Window window;
        private readonly IMessageLayerHost layerHost;
        private readonly Action<Window> disposeCallback;
        private readonly List<DispatcherTimer> timers = new();

        private MessageHost(Window window, IMessageLayerHost layerHost, Action<Window> disposeCallback)
        {
            this.window = window;
            this.layerHost = layerHost;
            this.disposeCallback = disposeCallback;
            window.Closed += HandleWindowClosed;
        }

        public bool IsDisposed { get; private set; }

        public static MessageHost? TryCreate(Window window, Action<Window> disposeCallback)
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
                return new MessageHost(window, new AdornerMessageLayerHost(layer, adorner), disposeCallback);
            }

            return new MessageHost(window, new PopupMessageLayerHost(adornedElement), disposeCallback);
        }

        public void Show(string message, MessageStyle style, int duration)
        {
            if (IsDisposed)
            {
                return;
            }

            var (root, translateTransform) = CreateMessageElement(message, style);
            layerHost.Add(root);
            PlayShowAnimation(root, translateTransform);

            var timer = new DispatcherTimer(DispatcherPriority.Background, window.Dispatcher)
            {
                Interval = TimeSpan.FromMilliseconds(duration)
            };

            void TickHandler(object? sender, EventArgs args)
            {
                timer.Stop();
                timer.Tick -= TickHandler;
                timers.Remove(timer);

                if (IsDisposed)
                {
                    return;
                }

                PlayHideAnimation(root, translateTransform, () =>
                {
                    layerHost.Remove(root);
                    if (layerHost.Count == 0)
                    {
                        Dispose();
                    }
                });
            }

            timer.Tick += TickHandler;
            timers.Add(timer);
            timer.Start();
        }

        private void HandleWindowClosed(object? sender, EventArgs e)
        {
            Dispose();
        }

        private void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;
            window.Closed -= HandleWindowClosed;

            foreach (var timer in timers.ToArray())
            {
                timer.Stop();
            }

            timers.Clear();
            layerHost.Dispose();
            disposeCallback(window);
        }

        private static (Border Root, TranslateTransform Transform) CreateMessageElement(string message, MessageStyle style)
        {
            var transform = new TranslateTransform(0, -12);

            var root = new Border
            {
                Margin = new Thickness(0, 0, 0, 12),
                Padding = new Thickness(16, 12, 16, 12),
                MaxWidth = 420,
                CornerRadius = new CornerRadius(12),
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

            var indicator = new Ellipse
            {
                Width = 10,
                Height = 10,
                VerticalAlignment = VerticalAlignment.Center
            };
            indicator.SetResourceReference(Shape.FillProperty, style.AccentBrushKey);

            var textBlock = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 13,
                Text = message,
                TextWrapping = TextWrapping.Wrap
            };
            textBlock.SetResourceReference(TextBlock.ForegroundProperty, "Theme.Text.Primary.Brush");

            Grid.SetColumn(indicator, 0);
            Grid.SetColumn(textBlock, 2);
            contentGrid.Children.Add(indicator);
            contentGrid.Children.Add(textBlock);

            root.Child = contentGrid;
            return (root, transform);
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

    private interface IMessageLayerHost : IDisposable
    {
        int Count { get; }
        void Add(UIElement element);
        void Remove(UIElement element);
    }

    private sealed class AdornerMessageLayerHost : IMessageLayerHost
    {
        private readonly AdornerLayer layer;
        private readonly MessageHostAdorner adorner;

        public AdornerMessageLayerHost(AdornerLayer layer, MessageHostAdorner adorner)
        {
            this.layer = layer;
            this.adorner = adorner;
        }

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

    private sealed class PopupMessageLayerHost : IMessageLayerHost
    {
        private readonly Popup popup;
        private readonly StackPanel stackPanel;

        public PopupMessageLayerHost(FrameworkElement placementTarget)
        {
            stackPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(24, 24, 24, 0),
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
                VerticalOffset = 0,
                HorizontalOffset = 0,
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
        private VisualCollection? visuals;
        private Grid? root;
        private StackPanel? stackPanel;

        public MessageHostAdorner(UIElement adornedElement) : base(adornedElement)
        {
            IsHitTestVisible = false;

            visuals = new VisualCollection(this);
            root = new Grid();
            stackPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(24, 24, 24, 0)
            };

            root.Children.Add(stackPanel);
            visuals.Add(root);
        }

        public int Count => stackPanel?.Children.Count ?? 0;

        public void Add(UIElement element)
        {
            stackPanel?.Children.Add(element);
        }

        public void Remove(UIElement element)
        {
            stackPanel?.Children.Remove(element);
        }

        protected override int VisualChildrenCount => visuals?.Count ?? 0;

        protected override Visual GetVisualChild(int index)
        {
            if (visuals is null || index < 0 || index >= visuals.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return visuals[index];
        }

        protected override Size MeasureOverride(Size constraint)
        {
            root?.Measure(constraint);
            return constraint;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            root?.Arrange(new Rect(finalSize));
            return finalSize;
        }
    }
}
