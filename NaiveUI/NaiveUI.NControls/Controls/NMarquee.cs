using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public enum NMarqueeDirection
{
    Left,
    Right,
    Up,
    Down
}

[TemplatePart(Name = TrackPartName, Type = typeof(Panel))]
[TemplatePart(Name = ContentPresenterPartName, Type = typeof(ContentPresenter))]
[TemplatePart(Name = TailPartName, Type = typeof(Rectangle))]
public class NMarquee : ContentControl
{
    private const string TrackPartName = "PART_Track";
    private const string ContentPresenterPartName = "PART_ContentPresenter";
    private const string TailPartName = "PART_Tail";
    private const double DefaultSpeed = 48d;

    private Panel? track;
    private ContentPresenter? sourcePresenter;
    private Rectangle? tailElement;
    private readonly List<FrameworkElement> copyElements = [];
    private Brush? contentBrush;
    private DateTime lastRenderingTime;
    private DateTime delayUntilTime;
    private bool isRenderingSubscribed;
    private bool hasStarted;
    private bool isUpdatingVisuals;
    private bool isRefreshPending;
    private double offset;
    private Size contentSize = Size.Empty;

    private static readonly DependencyPropertyKey IsRunningPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsRunning),
            typeof(bool),
            typeof(NMarquee),
            new PropertyMetadata(false));

    private static readonly DependencyPropertyKey IsPausedPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsPaused),
            typeof(bool),
            typeof(NMarquee),
            new PropertyMetadata(false));

    private static readonly DependencyPropertyKey ContentExtentPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(ContentExtent),
            typeof(double),
            typeof(NMarquee),
            new PropertyMetadata(0d));

    private static readonly DependencyPropertyKey ViewportExtentPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(ViewportExtent),
            typeof(double),
            typeof(NMarquee),
            new PropertyMetadata(0d));

    private static readonly DependencyPropertyKey EffectiveRepeatCountPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(EffectiveRepeatCount),
            typeof(int),
            typeof(NMarquee),
            new PropertyMetadata(1));

    static NMarquee()
    {
        ElementBase.DefaultStyle<NMarquee>(DefaultStyleKeyProperty);
    }

    public NMarquee()
    {
        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;
        SizeChanged += HandleSizeChanged;
        MouseEnter += HandleMouseEnter;
        MouseLeave += HandleMouseLeave;
    }

    public bool Active
    {
        get => (bool)GetValue(ActiveProperty);
        set => SetValue(ActiveProperty, value);
    }

    public static readonly DependencyProperty ActiveProperty =
        ElementBase.Property<NMarquee, bool>(nameof(ActiveProperty), true, OnPlaybackPropertyChanged);

    public bool IsAnimationEnabled
    {
        get => (bool)GetValue(IsAnimationEnabledProperty);
        set => SetValue(IsAnimationEnabledProperty, value);
    }

    public static readonly DependencyProperty IsAnimationEnabledProperty =
        ElementBase.Property<NMarquee, bool>(nameof(IsAnimationEnabledProperty), true, OnPlaybackPropertyChanged);

    public NMarqueeDirection Direction
    {
        get => (NMarqueeDirection)GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    public static readonly DependencyProperty DirectionProperty =
        ElementBase.Property<NMarquee, NMarqueeDirection>(nameof(DirectionProperty), NMarqueeDirection.Left, OnLayoutPropertyChanged);

    public double Speed
    {
        get => (double)GetValue(SpeedProperty);
        set => SetValue(SpeedProperty, value);
    }

    public static readonly DependencyProperty SpeedProperty =
        ElementBase.Property<NMarquee, double>(nameof(SpeedProperty), DefaultSpeed, OnPlaybackPropertyChanged);

    public double Duration
    {
        get => (double)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public static readonly DependencyProperty DurationProperty =
        ElementBase.Property<NMarquee, double>(nameof(DurationProperty), 0d, OnPlaybackPropertyChanged);

    public double Delay
    {
        get => (double)GetValue(DelayProperty);
        set => SetValue(DelayProperty, value);
    }

    public static readonly DependencyProperty DelayProperty =
        ElementBase.Property<NMarquee, double>(nameof(DelayProperty), 0d, OnPlaybackPropertyChanged);

    public double Gap
    {
        get => (double)GetValue(GapProperty);
        set => SetValue(GapProperty, value);
    }

    public static readonly DependencyProperty GapProperty =
        ElementBase.Property<NMarquee, double>(nameof(GapProperty), 24d, OnLayoutPropertyChanged);

    public bool AutoFill
    {
        get => (bool)GetValue(AutoFillProperty);
        set => SetValue(AutoFillProperty, value);
    }

    public static readonly DependencyProperty AutoFillProperty =
        ElementBase.Property<NMarquee, bool>(nameof(AutoFillProperty), false, OnLayoutPropertyChanged);

    public int Repeat
    {
        get => (int)GetValue(RepeatProperty);
        set => SetValue(RepeatProperty, value);
    }

    public static readonly DependencyProperty RepeatProperty =
        ElementBase.Property<NMarquee, int>(nameof(RepeatProperty), 1, OnLayoutPropertyChanged);

    public bool Loop
    {
        get => (bool)GetValue(LoopProperty);
        set => SetValue(LoopProperty, value);
    }

    public static readonly DependencyProperty LoopProperty =
        ElementBase.Property<NMarquee, bool>(nameof(LoopProperty), true, OnPlaybackPropertyChanged);

    public bool PauseOnHover
    {
        get => (bool)GetValue(PauseOnHoverProperty);
        set => SetValue(PauseOnHoverProperty, value);
    }

    public static readonly DependencyProperty PauseOnHoverProperty =
        ElementBase.Property<NMarquee, bool>(nameof(PauseOnHoverProperty), true);

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        ElementBase.Property<NMarquee, CornerRadius>(nameof(CornerRadiusProperty), new CornerRadius(0));

    public bool IsRunning
    {
        get => (bool)GetValue(IsRunningProperty);
        private set => SetValue(IsRunningPropertyKey, value);
    }

    public static readonly DependencyProperty IsRunningProperty = IsRunningPropertyKey.DependencyProperty;

    public bool IsPaused
    {
        get => (bool)GetValue(IsPausedProperty);
        private set => SetValue(IsPausedPropertyKey, value);
    }

    public static readonly DependencyProperty IsPausedProperty = IsPausedPropertyKey.DependencyProperty;

    public double ContentExtent
    {
        get => (double)GetValue(ContentExtentProperty);
        private set => SetValue(ContentExtentPropertyKey, value);
    }

    public static readonly DependencyProperty ContentExtentProperty = ContentExtentPropertyKey.DependencyProperty;

    public double ViewportExtent
    {
        get => (double)GetValue(ViewportExtentProperty);
        private set => SetValue(ViewportExtentPropertyKey, value);
    }

    public static readonly DependencyProperty ViewportExtentProperty = ViewportExtentPropertyKey.DependencyProperty;

    public int EffectiveRepeatCount
    {
        get => (int)GetValue(EffectiveRepeatCountProperty);
        private set => SetValue(EffectiveRepeatCountPropertyKey, value);
    }

    public static readonly DependencyProperty EffectiveRepeatCountProperty = EffectiveRepeatCountPropertyKey.DependencyProperty;

    public event RoutedEventHandler Started
    {
        add => AddHandler(StartedEvent, value);
        remove => RemoveHandler(StartedEvent, value);
    }

    public static readonly RoutedEvent StartedEvent =
        ElementBase.RoutedEvent<NMarquee, RoutedEventHandler>(nameof(StartedEvent));

    public event RoutedEventHandler Stopped
    {
        add => AddHandler(StoppedEvent, value);
        remove => RemoveHandler(StoppedEvent, value);
    }

    public static readonly RoutedEvent StoppedEvent =
        ElementBase.RoutedEvent<NMarquee, RoutedEventHandler>(nameof(StoppedEvent));

    public event RoutedEventHandler CycleCompleted
    {
        add => AddHandler(CycleCompletedEvent, value);
        remove => RemoveHandler(CycleCompletedEvent, value);
    }

    public static readonly RoutedEvent CycleCompletedEvent =
        ElementBase.RoutedEvent<NMarquee, RoutedEventHandler>(nameof(CycleCompletedEvent));

    public override void OnApplyTemplate()
    {
        ClearTrack();
        base.OnApplyTemplate();

        track = GetTemplateChild(TrackPartName) as Panel;
        sourcePresenter = GetTemplateChild(ContentPresenterPartName) as ContentPresenter;
        tailElement = GetTemplateChild(TailPartName) as Rectangle;
        BuildTrack();
        RestartCycle();
    }

    public void Play()
    {
        SetCurrentValue(ActiveProperty, true);
        IsPaused = false;
        StartRendering(resetDelay: true);
    }

    public void Pause()
    {
        if (!IsRunning)
        {
            return;
        }

        StopRendering(raiseStopped: false);
        IsPaused = true;
    }

    public void Resume()
    {
        if (!IsPaused)
        {
            return;
        }

        if (!Active)
        {
            SetCurrentValue(ActiveProperty, true);
        }

        IsPaused = false;
        StartRendering(resetDelay: false);
    }

    public void Stop()
    {
        StopRendering(raiseStopped: true);
        SetCurrentValue(ActiveProperty, false);
        IsPaused = false;
        offset = 0d;
        UpdateVisualPositions();
    }

    public void Restart()
    {
        offset = 0d;
        hasStarted = false;
        IsPaused = false;
        SetCurrentValue(ActiveProperty, true);
        BuildTrack();
        StartRendering(resetDelay: true);
        UpdateVisualPositions();
    }

    public void Refresh()
    {
        offset = 0d;
        BuildTrack();
        RestartCycle();
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        BuildTrack();
        RestartCycle();
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (isUpdatingVisuals)
        {
            return;
        }

        if (e.Property == FontFamilyProperty
            || e.Property == FontSizeProperty
            || e.Property == FontStretchProperty
            || e.Property == FontStyleProperty
            || e.Property == FontWeightProperty
            || e.Property == ForegroundProperty
            || e.Property == ContentTemplateProperty
            || e.Property == ContentTemplateSelectorProperty
            || e.Property == ContentStringFormatProperty
            || e.Property == PaddingProperty
            || e.Property == HorizontalContentAlignmentProperty
            || e.Property == VerticalContentAlignmentProperty)
        {
            BuildTrack();
            RestartCycle();
        }
    }

    private static void OnPlaybackPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NMarquee marquee)
        {
            marquee.RestartCycle();
        }
    }

    private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NMarquee marquee)
        {
            return;
        }

        marquee.offset = 0d;
        marquee.BuildTrack();
        marquee.UpdateVisualPositions();
        marquee.StartRendering(resetDelay: true);
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        BuildTrack();
        RestartCycle();
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        StopRendering(raiseStopped: false);
        hasStarted = false;
    }

    private void HandleSizeChanged(object sender, SizeChangedEventArgs e)
    {
        BuildTrack();
        UpdateVisualPositions();
    }

    private void HandleMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (PauseOnHover)
        {
            Pause();
        }
    }

    private void HandleMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (PauseOnHover)
        {
            Resume();
        }
    }

    private void RestartCycle()
    {
        offset = 0d;
        hasStarted = false;
        IsPaused = false;
        UpdateMetrics();
        UpdateVisualPositions();

        if (Active)
        {
            StartRendering(resetDelay: true);
        }
        else
        {
            StopRendering(raiseStopped: false);
        }
    }

    private void StartRendering(bool resetDelay)
    {
        UpdateMetrics();
        EnsureCopyElements();
        UpdateVisualPositions();

        if (!IsLoaded || !Active || IsPaused || !IsAnimationEnabled || ResolvePixelsPerSecond() <= 0d)
        {
            StopRendering(raiseStopped: false);
            return;
        }

        var now = DateTime.UtcNow;
        lastRenderingTime = now;
        if (resetDelay)
        {
            delayUntilTime = now.AddMilliseconds(Math.Max(0d, Delay));
        }

        if (!hasStarted)
        {
            hasStarted = true;
            RaiseEvent(new RoutedEventArgs(StartedEvent));
        }

        if (!isRenderingSubscribed)
        {
            CompositionTarget.Rendering += HandleRendering;
            isRenderingSubscribed = true;
        }

        IsRunning = true;
    }

    private void StopRendering(bool raiseStopped)
    {
        if (isRenderingSubscribed)
        {
            CompositionTarget.Rendering -= HandleRendering;
            isRenderingSubscribed = false;
        }

        if (IsRunning && raiseStopped)
        {
            RaiseEvent(new RoutedEventArgs(StoppedEvent));
        }

        IsRunning = false;
    }

    private void HandleRendering(object? sender, EventArgs e)
    {
        if (!Active || IsPaused)
        {
            StopRendering(raiseStopped: false);
            return;
        }

        var now = DateTime.UtcNow;
        if (now < delayUntilTime)
        {
            lastRenderingTime = now;
            return;
        }

        var pathLength = GetPathLength();
        var pixelsPerSecond = ResolvePixelsPerSecond();
        if (ContentExtent <= 0d || pathLength <= 0d || pixelsPerSecond <= 0d)
        {
            lastRenderingTime = now;
            return;
        }

        var elapsedSeconds = Math.Max(0d, (now - lastRenderingTime).TotalSeconds);
        lastRenderingTime = now;
        offset += pixelsPerSecond * elapsedSeconds;

        if (offset >= pathLength)
        {
            var cycles = Math.Max(1, (int)Math.Floor(offset / pathLength));
            offset %= pathLength;
            for (var i = 0; i < cycles; i++)
            {
                RaiseEvent(new RoutedEventArgs(CycleCompletedEvent));
            }

            if (!Loop)
            {
                offset = pathLength;
                StopRendering(raiseStopped: true);
            }
        }

        UpdateVisualPositions();
    }

    private void BuildTrack()
    {
        if (track is null || sourcePresenter is null)
        {
            return;
        }

        isUpdatingVisuals = true;
        try
        {
            sourcePresenter.SizeChanged -= HandleSourcePresenterSizeChanged;
            sourcePresenter.SizeChanged += HandleSourcePresenterSizeChanged;
            UpdateMetrics();
            RebuildCopyElements();
            EnsureCopyElements();
            UpdateVisualPositions();
        }
        finally
        {
            isUpdatingVisuals = false;
        }
    }

    private void ClearTrack()
    {
        if (sourcePresenter is not null)
        {
            sourcePresenter.SizeChanged -= HandleSourcePresenterSizeChanged;
            sourcePresenter.Visibility = Visibility.Visible;
            SetElementTranslation(sourcePresenter, 0d, 0d);
        }

        if (tailElement is not null)
        {
            tailElement.Fill = null;
            tailElement.Visibility = Visibility.Collapsed;
        }

        if (track is not null)
        {
            foreach (var copyElement in copyElements)
            {
                track.Children.Remove(copyElement);
            }
        }

        copyElements.Clear();
        contentBrush = null;
        sourcePresenter = null;
        tailElement = null;
    }

    private void MeasureContent()
    {
        if (sourcePresenter is null)
        {
            contentSize = Size.Empty;
            return;
        }

        sourcePresenter.ClearValue(WidthProperty);
        sourcePresenter.ClearValue(HeightProperty);
        sourcePresenter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        contentSize = sourcePresenter.DesiredSize;

        if (contentSize.Width > 0d && contentSize.Height > 0d)
        {
            sourcePresenter.Arrange(new Rect(new Point(0d, 0d), contentSize));
        }
    }

    private void UpdateContentBrush()
    {
        if (sourcePresenter is null || contentSize.Width <= 0d || contentSize.Height <= 0d)
        {
            contentBrush = null;
            return;
        }

        var dpi = VisualTreeHelper.GetDpi(this);
        var pixelWidth = Math.Max(1, (int)Math.Ceiling(contentSize.Width * dpi.DpiScaleX));
        var pixelHeight = Math.Max(1, (int)Math.Ceiling(contentSize.Height * dpi.DpiScaleY));
        var bitmap = new RenderTargetBitmap(
            pixelWidth,
            pixelHeight,
            dpi.PixelsPerInchX,
            dpi.PixelsPerInchY,
            PixelFormats.Pbgra32);

        sourcePresenter.UpdateLayout();
        bitmap.Render(sourcePresenter);
        if (bitmap.CanFreeze)
        {
            bitmap.Freeze();
        }

        var brush = new ImageBrush(bitmap)
        {
            Stretch = Stretch.Fill,
            AlignmentX = AlignmentX.Left,
            AlignmentY = AlignmentY.Top
        };
        if (brush.CanFreeze)
        {
            brush.Freeze();
        }

        contentBrush = brush;
        sourcePresenter.Visibility = Visibility.Visible;
    }

    private void UpdateMetrics()
    {
        MeasureContent();

        var horizontal = IsHorizontal;
        var contentAxis = horizontal ? contentSize.Width : contentSize.Height;
        var viewportAxis = track is null
            ? (horizontal ? Math.Max(0d, ActualWidth - Padding.Left - Padding.Right) : Math.Max(0d, ActualHeight - Padding.Top - Padding.Bottom))
            : (horizontal ? track.ActualWidth : track.ActualHeight);

        ContentExtent = Math.Max(0d, contentAxis);
        ViewportExtent = Math.Max(0d, viewportAxis);
        EffectiveRepeatCount = ResolveRepeatCount(ContentExtent, ViewportExtent);
    }

    private int ResolveRepeatCount(double contentAxis, double viewportAxis)
    {
        if (contentAxis <= 0d)
        {
            return 1;
        }

        var requestedCount = Math.Max(1, Repeat);
        if (!AutoFill)
        {
            return requestedCount;
        }

        var step = Math.Max(1d, contentAxis + NormalizeGap());
        var fillCount = (int)Math.Ceiling(Math.Max(0d, viewportAxis) / step);
        return Math.Max(requestedCount, fillCount);
    }

    private void UpdateVisualPositions()
    {
        if (sourcePresenter is null || track is null || contentSize.Width <= 0d || contentSize.Height <= 0d)
        {
            return;
        }

        EnsureCopyElements();
        var itemStep = GetItemStep();
        var groupExtent = GetGroupExtent();
        var crossOffset = IsHorizontal
            ? ResolveVerticalOffset(track.ActualHeight, contentSize.Height)
            : ResolveHorizontalOffset(track.ActualWidth, contentSize.Width);

        var groupItemCount = Math.Max(1, EffectiveRepeatCount);
        var totalItemCount = groupItemCount * 2;
        var sourceIndex = IsReverseDirection ? groupItemCount : 0;
        var basePosition = IsReverseDirection ? offset - groupExtent : -offset;
        var copyIndex = 0;

        for (var itemIndex = 0; itemIndex < totalItemCount; itemIndex++)
        {
            var groupIndex = itemIndex / groupItemCount;
            var itemIndexInGroup = itemIndex % groupItemCount;
            var itemPosition = basePosition + (groupIndex * groupExtent) + (itemIndexInGroup * itemStep);
            if (itemIndex == sourceIndex)
            {
                sourcePresenter.Visibility = Visibility.Visible;
                SetVisualPosition(sourcePresenter, itemPosition, crossOffset);
                continue;
            }

            if (copyIndex >= copyElements.Count)
            {
                break;
            }

            var copyElement = copyElements[copyIndex++];
            copyElement.Visibility = Visibility.Visible;
            SetVisualPosition(copyElement, itemPosition, crossOffset);
        }

        for (; copyIndex < copyElements.Count; copyIndex++)
        {
            copyElements[copyIndex].Visibility = Visibility.Collapsed;
        }
    }

    private void HandleSourcePresenterSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (isUpdatingVisuals || isRefreshPending || e.NewSize.Width <= 0d || e.NewSize.Height <= 0d)
        {
            return;
        }

        isRefreshPending = true;
        Dispatcher.BeginInvoke(new Action(() =>
        {
            isRefreshPending = false;
            if (!IsLoaded)
            {
                return;
            }

            offset = 0d;
            BuildTrack();
            RestartCycle();
        }));
    }

    private void SetVisualPosition(UIElement element, double position, double crossOffset)
    {
        if (IsHorizontal)
        {
            SetElementPosition(element, position, crossOffset);
        }
        else
        {
            SetElementPosition(element, crossOffset, position);
        }
    }

    private void EnsureCopyElements()
    {
        if (track is null || sourcePresenter is null)
        {
            return;
        }

        var requiredCopyCount = Math.Max(0, (EffectiveRepeatCount * 2) - 1);
        if (copyElements.Count != requiredCopyCount)
        {
            RebuildCopyElements();
        }

        for (var i = 0; i < copyElements.Count; i++)
        {
            var copyElement = copyElements[i];
            EnsureElementTransform(copyElement);
            copyElement.IsHitTestVisible = false;

            if (i >= requiredCopyCount || contentSize.Width <= 0d || contentSize.Height <= 0d)
            {
                copyElement.Visibility = Visibility.Collapsed;
                continue;
            }

            copyElement.Width = contentSize.Width;
            copyElement.Height = contentSize.Height;
        }
    }

    private void RebuildCopyElements()
    {
        if (track is null)
        {
            return;
        }

        foreach (var copyElement in copyElements)
        {
            track.Children.Remove(copyElement);
        }

        copyElements.Clear();
        tailElement?.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);

        var requiredCopyCount = Math.Max(0, (EffectiveRepeatCount * 2) - 1);
        if (requiredCopyCount == 0)
        {
            return;
        }

        UpdateContentBrush();
        for (var i = 0; i < requiredCopyCount; i++)
        {
            var copyElement = CreateCopyElement();
            EnsureElementTransform(copyElement);
            copyElements.Add(copyElement);
            track.Children.Add(copyElement);
        }
    }

    private FrameworkElement CreateCopyElement()
    {
        var contentCopy = CreateCopyContent();
        if (contentCopy is not null)
        {
            var presenter = new ContentPresenter
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Content = contentCopy,
                ContentStringFormat = ContentStringFormat,
                ContentTemplate = contentCopy is UIElement ? null : ContentTemplate,
                ContentTemplateSelector = contentCopy is UIElement ? null : ContentTemplateSelector,
                SnapsToDevicePixels = SnapsToDevicePixels,
                IsHitTestVisible = false
            };
            return presenter;
        }

        return new Rectangle
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Fill = contentBrush,
            SnapsToDevicePixels = SnapsToDevicePixels,
            IsHitTestVisible = false
        };
    }

    private object? CreateCopyContent()
    {
        if (Content is null)
        {
            return null;
        }

        if (Content is not UIElement)
        {
            return Content;
        }

        try
        {
            return XamlReader.Parse(XamlWriter.Save(Content));
        }
        catch
        {
            return null;
        }
    }

    private static void SetElementTranslation(UIElement element, double x, double y)
    {
        if (element.RenderTransform is TranslateTransform transform)
        {
            transform.X = x;
            transform.Y = y;
            return;
        }

        element.RenderTransform = new TranslateTransform(x, y);
    }

    private static void SetElementPosition(UIElement element, double x, double y)
    {
        Canvas.SetLeft(element, x);
        Canvas.SetTop(element, y);
        SetElementTranslation(element, 0d, 0d);
    }

    private static void EnsureElementTransform(UIElement element)
    {
        if (element.RenderTransform is not TranslateTransform)
        {
            element.RenderTransform = new TranslateTransform();
        }
    }

    private double ResolvePixelsPerSecond()
    {
        if (Duration > 0d && ContentExtent > 0d)
        {
            return GetPathLength() / (Duration / 1000d);
        }

        return Math.Max(0d, Speed);
    }

    private double GetPathLength()
    {
        return GetGroupExtent();
    }

    private double GetGroupExtent()
    {
        if (ContentExtent <= 0d)
        {
            return 1d;
        }

        var groupItemCount = Math.Max(1, EffectiveRepeatCount);
        var itemStep = GetItemStep();
        if (AutoFill)
        {
            return Math.Max(1d, groupItemCount * itemStep);
        }

        return Math.Max(Math.Max(1d, ViewportExtent), groupItemCount * itemStep);
    }

    private double GetItemStep()
    {
        return Math.Max(1d, ContentExtent + NormalizeGap());
    }

    private double NormalizeGap()
    {
        return Math.Max(0d, Gap);
    }

    private double ResolveHorizontalOffset(double viewportWidth, double contentWidth)
    {
        return HorizontalContentAlignment switch
        {
            HorizontalAlignment.Center => Math.Max(0d, (viewportWidth - contentWidth) / 2d),
            HorizontalAlignment.Right => Math.Max(0d, viewportWidth - contentWidth),
            HorizontalAlignment.Stretch => 0d,
            _ => 0d
        };
    }

    private double ResolveVerticalOffset(double viewportHeight, double contentHeight)
    {
        return VerticalContentAlignment switch
        {
            VerticalAlignment.Center => Math.Max(0d, (viewportHeight - contentHeight) / 2d),
            VerticalAlignment.Bottom => Math.Max(0d, viewportHeight - contentHeight),
            VerticalAlignment.Stretch => 0d,
            _ => 0d
        };
    }

    private bool IsHorizontal => Direction is NMarqueeDirection.Left or NMarqueeDirection.Right;

    private bool IsReverseDirection => Direction is NMarqueeDirection.Right or NMarqueeDirection.Down;
}
