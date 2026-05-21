using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

[TemplatePart(Name = OverlayHostPartName, Type = typeof(FrameworkElement))]
public class NWatermark : ContentControl
{
    private const string OverlayHostPartName = "PART_OverlayHost";
    private const double RotatedOverlayMarginSize = 2048d;

    private static readonly Brush DefaultFontColorBrush = CreateDefaultFontColorBrush();

    private FrameworkElement? overlayHost;
    private BitmapImage? bitmapImage;
    private ImageSource? loadedImageSource;
    private AdornerLayer? fullscreenAdornerLayer;
    private UIElement? fullscreenAdornedElement;
    private NWatermarkAdorner? fullscreenAdorner;

    static NWatermark()
    {
        ElementBase.DefaultStyle<NWatermark>(DefaultStyleKeyProperty);
    }

    public NWatermark()
    {
        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;
        IsVisibleChanged += HandleIsVisibleChanged;
        SizeChanged += HandleSizeChanged;
        UpdateResolvedState();
        LoadImageSource();
    }

    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty TextProperty =
        ElementBase.Property<NWatermark, string?>(nameof(TextProperty), null, OnWatermarkPropertyChanged);

    public bool Cross
    {
        get => (bool)GetValue(CrossProperty);
        set => SetValue(CrossProperty, value);
    }

    public static readonly DependencyProperty CrossProperty =
        ElementBase.Property<NWatermark, bool>(nameof(CrossProperty), false, OnWatermarkPropertyChanged);

    public bool Debug
    {
        get => (bool)GetValue(DebugProperty);
        set => SetValue(DebugProperty, value);
    }

    public static readonly DependencyProperty DebugProperty =
        ElementBase.Property<NWatermark, bool>(nameof(DebugProperty), false, OnWatermarkPropertyChanged);

    public bool Fullscreen
    {
        get => (bool)GetValue(FullscreenProperty);
        set => SetValue(FullscreenProperty, value);
    }

    public static readonly DependencyProperty FullscreenProperty =
        ElementBase.Property<NWatermark, bool>(nameof(FullscreenProperty), false, OnWatermarkPropertyChanged);

    public double MarkWidth
    {
        get => (double)GetValue(MarkWidthProperty);
        set => SetValue(MarkWidthProperty, value);
    }

    public static readonly DependencyProperty MarkWidthProperty =
        ElementBase.Property<NWatermark, double>(nameof(MarkWidthProperty), 32d, OnWatermarkPropertyChanged);

    public double MarkHeight
    {
        get => (double)GetValue(MarkHeightProperty);
        set => SetValue(MarkHeightProperty, value);
    }

    public static readonly DependencyProperty MarkHeightProperty =
        ElementBase.Property<NWatermark, double>(nameof(MarkHeightProperty), 32d, OnWatermarkPropertyChanged);

    public int ZIndex
    {
        get => (int)GetValue(ZIndexProperty);
        set => SetValue(ZIndexProperty, value);
    }

    public static readonly DependencyProperty ZIndexProperty =
        ElementBase.Property<NWatermark, int>(nameof(ZIndexProperty), 10, OnWatermarkPropertyChanged);

    public double XGap
    {
        get => (double)GetValue(XGapProperty);
        set => SetValue(XGapProperty, value);
    }

    public static readonly DependencyProperty XGapProperty =
        ElementBase.Property<NWatermark, double>(nameof(XGapProperty), 0d, OnWatermarkPropertyChanged);

    public double YGap
    {
        get => (double)GetValue(YGapProperty);
        set => SetValue(YGapProperty, value);
    }

    public static readonly DependencyProperty YGapProperty =
        ElementBase.Property<NWatermark, double>(nameof(YGapProperty), 0d, OnWatermarkPropertyChanged);

    public double YOffset
    {
        get => (double)GetValue(YOffsetProperty);
        set => SetValue(YOffsetProperty, value);
    }

    public static readonly DependencyProperty YOffsetProperty =
        ElementBase.Property<NWatermark, double>(nameof(YOffsetProperty), 0d, OnWatermarkPropertyChanged);

    public double XOffset
    {
        get => (double)GetValue(XOffsetProperty);
        set => SetValue(XOffsetProperty, value);
    }

    public static readonly DependencyProperty XOffsetProperty =
        ElementBase.Property<NWatermark, double>(nameof(XOffsetProperty), 0d, OnWatermarkPropertyChanged);

    public double Rotate
    {
        get => (double)GetValue(RotateProperty);
        set => SetValue(RotateProperty, value);
    }

    public static readonly DependencyProperty RotateProperty =
        ElementBase.Property<NWatermark, double>(nameof(RotateProperty), 0d, OnWatermarkPropertyChanged);

    public TextAlignment TextAlign
    {
        get => (TextAlignment)GetValue(TextAlignProperty);
        set => SetValue(TextAlignProperty, value);
    }

    public static readonly DependencyProperty TextAlignProperty =
        ElementBase.Property<NWatermark, TextAlignment>(nameof(TextAlignProperty), TextAlignment.Left, OnWatermarkPropertyChanged);

    public string? Image
    {
        get => (string?)GetValue(ImageProperty);
        set => SetValue(ImageProperty, value);
    }

    public static readonly DependencyProperty ImageProperty =
        ElementBase.Property<NWatermark, string?>(nameof(ImageProperty), null, OnWatermarkImagePropertyChanged);

    public double ImageOpacity
    {
        get => (double)GetValue(ImageOpacityProperty);
        set => SetValue(ImageOpacityProperty, value);
    }

    public static readonly DependencyProperty ImageOpacityProperty =
        ElementBase.Property<NWatermark, double>(nameof(ImageOpacityProperty), 1d, OnWatermarkPropertyChanged);

    public double ImageHeight
    {
        get => (double)GetValue(ImageHeightProperty);
        set => SetValue(ImageHeightProperty, value);
    }

    public static readonly DependencyProperty ImageHeightProperty =
        ElementBase.Property<NWatermark, double>(nameof(ImageHeightProperty), double.NaN, OnWatermarkPropertyChanged);

    public double ImageWidth
    {
        get => (double)GetValue(ImageWidthProperty);
        set => SetValue(ImageWidthProperty, value);
    }

    public static readonly DependencyProperty ImageWidthProperty =
        ElementBase.Property<NWatermark, double>(nameof(ImageWidthProperty), double.NaN, OnWatermarkPropertyChanged);

    public bool Selectable
    {
        get => (bool)GetValue(SelectableProperty);
        set => SetValue(SelectableProperty, value);
    }

    public static readonly DependencyProperty SelectableProperty =
        ElementBase.Property<NWatermark, bool>(nameof(SelectableProperty), true, OnWatermarkPropertyChanged);

    public Brush FontColor
    {
        get => (Brush)GetValue(FontColorProperty);
        set => SetValue(FontColorProperty, value);
    }

    public static readonly DependencyProperty FontColorProperty =
        ElementBase.Property<NWatermark, Brush>(nameof(FontColorProperty), DefaultFontColorBrush, OnWatermarkPropertyChanged);

    public string FontVariant
    {
        get => (string)GetValue(FontVariantProperty);
        set => SetValue(FontVariantProperty, value);
    }

    public static readonly DependencyProperty FontVariantProperty =
        ElementBase.Property<NWatermark, string>(nameof(FontVariantProperty), string.Empty, OnWatermarkPropertyChanged);

    public double LineHeight
    {
        get => (double)GetValue(LineHeightProperty);
        set => SetValue(LineHeightProperty, value);
    }

    public static readonly DependencyProperty LineHeightProperty =
        ElementBase.Property<NWatermark, double>(nameof(LineHeightProperty), 14d, OnWatermarkPropertyChanged);

    public double GlobalRotate
    {
        get => (double)GetValue(GlobalRotateProperty);
        set => SetValue(GlobalRotateProperty, value);
    }

    public static readonly DependencyProperty GlobalRotateProperty =
        ElementBase.Property<NWatermark, double>(nameof(GlobalRotateProperty), 0d, OnWatermarkPropertyChanged);

    public Brush? ResolvedPrimaryBrush
    {
        get => (Brush?)GetValue(ResolvedPrimaryBrushProperty);
        private set => SetValue(ResolvedPrimaryBrushProperty, value);
    }

    public static readonly DependencyProperty ResolvedPrimaryBrushProperty =
        ElementBase.Property<NWatermark, Brush?>(nameof(ResolvedPrimaryBrushProperty), null);

    public Brush? ResolvedCrossBrush
    {
        get => (Brush?)GetValue(ResolvedCrossBrushProperty);
        private set => SetValue(ResolvedCrossBrushProperty, value);
    }

    public static readonly DependencyProperty ResolvedCrossBrushProperty =
        ElementBase.Property<NWatermark, Brush?>(nameof(ResolvedCrossBrushProperty), null);

    public bool ShowLocalOverlay
    {
        get => (bool)GetValue(ShowLocalOverlayProperty);
        private set => SetValue(ShowLocalOverlayProperty, value);
    }

    public static readonly DependencyProperty ShowLocalOverlayProperty =
        ElementBase.Property<NWatermark, bool>(nameof(ShowLocalOverlayProperty), true);

    public bool ShowCrossOverlay
    {
        get => (bool)GetValue(ShowCrossOverlayProperty);
        private set => SetValue(ShowCrossOverlayProperty, value);
    }

    public static readonly DependencyProperty ShowCrossOverlayProperty =
        ElementBase.Property<NWatermark, bool>(nameof(ShowCrossOverlayProperty), false);

    public Thickness ResolvedOverlayMargin
    {
        get => (Thickness)GetValue(ResolvedOverlayMarginProperty);
        private set => SetValue(ResolvedOverlayMarginProperty, value);
    }

    public static readonly DependencyProperty ResolvedOverlayMarginProperty =
        ElementBase.Property<NWatermark, Thickness>(nameof(ResolvedOverlayMarginProperty), new Thickness(0));

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        overlayHost = GetTemplateChild(OverlayHostPartName) as FrameworkElement;
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == FontFamilyProperty
            || e.Property == FontStyleProperty
            || e.Property == FontWeightProperty
            || e.Property == FontStretchProperty
            || e.Property == FontSizeProperty)
        {
            UpdateResolvedState();
        }
    }

    private static void OnWatermarkPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NWatermark watermark)
        {
            watermark.UpdateResolvedState();
        }
    }

    private static void OnWatermarkImagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NWatermark watermark)
        {
            watermark.LoadImageSource();
            watermark.UpdateResolvedState();
        }
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        UpdateResolvedState();
        UpdateFullscreenAdorner();
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        DetachFullscreenAdorner();
        UnhookBitmapImageEvents(bitmapImage);
    }

    private void HandleIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        UpdateFullscreenAdorner();
    }

    private void HandleSizeChanged(object sender, SizeChangedEventArgs e)
    {
        fullscreenAdorner?.InvalidateVisual();
    }

    private void UpdateResolvedState()
    {
        ShowLocalOverlay = !Fullscreen;
        ResolvedOverlayMargin = Math.Abs(GlobalRotate) > double.Epsilon
            ? new Thickness(-RotatedOverlayMarginSize)
            : new Thickness(0);

        var primaryBrush = CreateTileBrush(0d, 0d);
        ResolvedPrimaryBrush = primaryBrush;
        ResolvedCrossBrush = Cross ? CreateTileBrush(MarkWidth / 2d, MarkHeight / 2d) : null;
        ShowCrossOverlay = Cross && ResolvedCrossBrush is not null;

        UpdateFullscreenAdorner();
        fullscreenAdorner?.InvalidateVisual();
        overlayHost?.InvalidateVisual();
    }

    private void LoadImageSource()
    {
        loadedImageSource = null;
        UnhookBitmapImageEvents(bitmapImage);
        bitmapImage = null;

        if (string.IsNullOrWhiteSpace(Image))
        {
            return;
        }

        try
        {
            var uri = new Uri(Image, UriKind.RelativeOrAbsolute);
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = uri;
            image.CacheOption = BitmapCacheOption.OnDemand;
            image.CreateOptions = BitmapCreateOptions.DelayCreation;
            image.EndInit();

            bitmapImage = image;
            loadedImageSource = image;

            if (image.IsDownloading)
            {
                image.DownloadCompleted += HandleBitmapImageDownloadCompleted;
                image.DownloadFailed += HandleBitmapImageDownloadFailed;
            }
        }
        catch
        {
            loadedImageSource = null;
        }
    }

    private void HandleBitmapImageDownloadCompleted(object? sender, EventArgs e)
    {
        UpdateResolvedState();
    }

    private void HandleBitmapImageDownloadFailed(object? sender, ExceptionEventArgs e)
    {
        loadedImageSource = null;
        UpdateResolvedState();
    }

    private void UnhookBitmapImageEvents(BitmapImage? image)
    {
        if (image is null)
        {
            return;
        }

        image.DownloadCompleted -= HandleBitmapImageDownloadCompleted;
        image.DownloadFailed -= HandleBitmapImageDownloadFailed;
    }

    private Brush? CreateTileBrush(double additionalOffsetX, double additionalOffsetY)
    {
        var tileWidth = Math.Max(1d, MarkWidth + XGap);
        var tileHeight = Math.Max(1d, MarkHeight + YGap);
        var watermarkCanvas = new Canvas
        {
            Width = tileWidth,
            Height = tileHeight,
            ClipToBounds = false,
            IsHitTestVisible = false
        };

        if (Debug)
        {
            watermarkCanvas.Children.Add(new Rectangle
            {
                Width = Math.Max(1d, MarkWidth),
                Height = Math.Max(1d, MarkHeight),
                Stroke = Brushes.Gray,
                StrokeThickness = 1d
            });
        }

        if (loadedImageSource is not null)
        {
            var drawWidth = ResolveImageWidth(loadedImageSource);
            var drawHeight = ResolveImageHeight(loadedImageSource);
            if (drawWidth <= 0d || drawHeight <= 0d)
            {
                return null;
            }

            var image = new Image
            {
                Source = loadedImageSource,
                Width = drawWidth,
                Height = drawHeight,
                Opacity = Math.Clamp(ImageOpacity, 0d, 1d),
                Stretch = Stretch.Fill,
                RenderTransform = new RotateTransform(Rotate),
                RenderTransformOrigin = new Point(0d, 0d)
            };

            Canvas.SetLeft(image, XOffset + additionalOffsetX);
            Canvas.SetTop(image, YOffset + additionalOffsetY);
            watermarkCanvas.Children.Add(image);

            if (Debug)
            {
                watermarkCanvas.Children.Add(new Rectangle
                {
                    Width = drawWidth,
                    Height = drawHeight,
                    Stroke = Brushes.Green,
                    StrokeThickness = 1d,
                    RenderTransform = new RotateTransform(Rotate),
                    RenderTransformOrigin = new Point(0d, 0d)
                });

                var markerRect = (Rectangle)watermarkCanvas.Children[watermarkCanvas.Children.Count - 1];
                Canvas.SetLeft(markerRect, XOffset + additionalOffsetX);
                Canvas.SetTop(markerRect, YOffset + additionalOffsetY);
            }
        }
        else if (!string.IsNullOrWhiteSpace(Text))
        {
            var textBlock = new TextBlock
            {
                Text = NormalizeMultilineText(Text),
                FontFamily = FontFamily,
                FontStyle = FontStyle,
                FontWeight = FontWeight,
                FontStretch = FontStretch,
                FontSize = FontSize,
                Foreground = FontColor,
                TextAlignment = TextAlign,
                TextWrapping = TextWrapping.NoWrap,
                LineHeight = LineHeight,
                LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
                RenderTransform = new RotateTransform(Rotate),
                RenderTransformOrigin = new Point(0d, 0d)
            };

            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            textBlock.Width = Math.Max(1d, Math.Ceiling(textBlock.DesiredSize.Width));
            textBlock.Height = Math.Max(1d, Math.Ceiling(textBlock.DesiredSize.Height));
            Canvas.SetLeft(textBlock, XOffset + additionalOffsetX);
            Canvas.SetTop(textBlock, YOffset + additionalOffsetY);
            watermarkCanvas.Children.Add(textBlock);

            if (Debug)
            {
                var debugRect = new Rectangle
                {
                    Width = textBlock.Width,
                    Height = textBlock.Height,
                    Stroke = Brushes.Green,
                    StrokeThickness = 1d,
                    RenderTransform = new RotateTransform(Rotate),
                    RenderTransformOrigin = new Point(0d, 0d)
                };
                Canvas.SetLeft(debugRect, XOffset + additionalOffsetX);
                Canvas.SetTop(debugRect, YOffset + additionalOffsetY);
                watermarkCanvas.Children.Add(debugRect);
            }
        }
        else
        {
            return null;
        }

        return new VisualBrush(watermarkCanvas)
        {
            AlignmentX = AlignmentX.Left,
            AlignmentY = AlignmentY.Top,
            Stretch = Stretch.None,
            TileMode = TileMode.Tile,
            ViewportUnits = BrushMappingMode.Absolute,
            ViewboxUnits = BrushMappingMode.Absolute,
            Viewport = new Rect(0d, 0d, tileWidth, tileHeight),
            Viewbox = new Rect(0d, 0d, tileWidth, tileHeight)
        };
    }

    private double ResolveImageWidth(ImageSource imageSource)
    {
        if (!double.IsNaN(ImageWidth) && ImageWidth > 0d)
        {
            return ImageWidth;
        }

        if (!double.IsNaN(ImageHeight) && ImageHeight > 0d)
        {
            var pixelHeight = Math.Max(1d, imageSource.Height);
            return Math.Max(1d, imageSource.Width * ImageHeight / pixelHeight);
        }

        return imageSource.Width;
    }

    private double ResolveImageHeight(ImageSource imageSource)
    {
        if (!double.IsNaN(ImageHeight) && ImageHeight > 0d)
        {
            return ImageHeight;
        }

        if (!double.IsNaN(ImageWidth) && ImageWidth > 0d)
        {
            var pixelWidth = Math.Max(1d, imageSource.Width);
            return Math.Max(1d, imageSource.Height * ImageWidth / pixelWidth);
        }

        return imageSource.Height;
    }

    private void UpdateFullscreenAdorner()
    {
        if (!IsLoaded || !Fullscreen || !IsVisible || Visibility != Visibility.Visible)
        {
            DetachFullscreenAdorner();
            return;
        }

        var window = Window.GetWindow(this);
        if (window?.Content is not UIElement adornedElement)
        {
            DetachFullscreenAdorner();
            return;
        }

        var adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
        if (adornerLayer is null)
        {
            DetachFullscreenAdorner();
            return;
        }

        if (ReferenceEquals(fullscreenAdornedElement, adornedElement)
            && ReferenceEquals(fullscreenAdornerLayer, adornerLayer)
            && fullscreenAdorner is not null)
        {
            fullscreenAdorner.InvalidateVisual();
            return;
        }

        DetachFullscreenAdorner();

        fullscreenAdornedElement = adornedElement;
        fullscreenAdornerLayer = adornerLayer;
        fullscreenAdorner = new NWatermarkAdorner(adornedElement, this);
        fullscreenAdornerLayer.Add(fullscreenAdorner);
        fullscreenAdorner.InvalidateVisual();
    }

    private void DetachFullscreenAdorner()
    {
        if (fullscreenAdornerLayer is not null && fullscreenAdorner is not null)
        {
            fullscreenAdornerLayer.Remove(fullscreenAdorner);
        }

        fullscreenAdorner = null;
        fullscreenAdornedElement = null;
        fullscreenAdornerLayer = null;
    }

    internal void RenderWatermark(DrawingContext drawingContext, Rect bounds)
    {
        if (ResolvedPrimaryBrush is null)
        {
            return;
        }

        drawingContext.PushClip(new RectangleGeometry(bounds));

        var renderBounds = bounds;
        if (Math.Abs(GlobalRotate) > double.Epsilon)
        {
            var extent = Math.Max(bounds.Width, bounds.Height) * 4d;
            var center = new Point(bounds.Left + bounds.Width / 2d, bounds.Top + bounds.Height / 2d);
            renderBounds = new Rect(center.X - extent / 2d, center.Y - extent / 2d, extent, extent);
            drawingContext.PushTransform(new RotateTransform(GlobalRotate, center.X, center.Y));
        }

        drawingContext.DrawRectangle(ResolvedPrimaryBrush, null, renderBounds);

        if (ShowCrossOverlay && ResolvedCrossBrush is not null)
        {
            drawingContext.DrawRectangle(ResolvedCrossBrush, null, renderBounds);
        }

        if (Math.Abs(GlobalRotate) > double.Epsilon)
        {
            drawingContext.Pop();
        }

        drawingContext.Pop();
    }

    private static string NormalizeMultilineText(string? text)
    {
        return (text ?? string.Empty).Replace("\r\n", "\n");
    }

    private static Brush CreateDefaultFontColorBrush()
    {
        var brush = new SolidColorBrush(Color.FromArgb(77, 128, 128, 128));
        brush.Freeze();
        return brush;
    }

    private sealed class NWatermarkAdorner : Adorner
    {
        private readonly NWatermark owner;

        public NWatermarkAdorner(UIElement adornedElement, NWatermark owner)
            : base(adornedElement)
        {
            this.owner = owner;
            IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            owner.RenderWatermark(drawingContext, new Rect(AdornedElement.RenderSize));
        }
    }
}
