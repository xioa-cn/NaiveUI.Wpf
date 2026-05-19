using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public enum NCarouselEffect
{
    Slide,
    Fade,
    Card
}

public enum NCarouselDotType
{
    Dot,
    Line
}

public enum NCarouselDotPlacement
{
    Bottom,
    Top,
    Left,
    Right
}

public enum NCarouselTrigger
{
    Click,
    Hover
}

[TemplatePart(Name = CurrentPresenterPartName, Type = typeof(ContentPresenter))]
[TemplatePart(Name = StandbyPresenterPartName, Type = typeof(ContentPresenter))]
public class NCarousel : ItemsControl
{
    private const string CurrentPresenterPartName = "PART_CurrentPresenter";
    private const string StandbyPresenterPartName = "PART_StandbyPresenter";
    private const double CardPeekWidth = 48d;
    private const double DragThreshold = 36d;

    private readonly DispatcherTimer autoplayTimer = new();
    private readonly DelegateCommand previousCommand;
    private readonly DelegateCommand nextCommand;
    private readonly DelegateCommand selectIndexCommand;
    private ContentPresenter? currentPresenter;
    private ContentPresenter? standbyPresenter;
    private bool isAnimating;
    private bool isPointerCaptured;
    private Point dragStartPoint;
    private int navigationDirection = 1;

    static NCarousel()
    {
        ElementBase.DefaultStyle<NCarousel>(DefaultStyleKeyProperty);
    }

    public NCarousel()
    {
        IndicatorItems = new ObservableCollection<NCarouselIndicatorItem>();
        previousCommand = new DelegateCommand(_ => MovePrevious(), _ => CanMovePrevious());
        nextCommand = new DelegateCommand(_ => MoveNext(), _ => CanMoveNext());
        selectIndexCommand = new DelegateCommand(MoveToIndex, _ => HasMultipleItems);

        PreviousCommand = previousCommand;
        NextCommand = nextCommand;
        SelectIndexCommand = selectIndexCommand;

        autoplayTimer.Tick += HandleAutoplayTick;
        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;
        MouseEnter += HandleMouseEnter;
        MouseLeave += HandleMouseLeave;
        PreviewMouseLeftButtonDown += HandlePreviewMouseLeftButtonDown;
        PreviewMouseMove += HandlePreviewMouseMove;
        PreviewMouseLeftButtonUp += HandlePreviewMouseLeftButtonUp;
        PreviewKeyDown += HandlePreviewKeyDown;
    }

    public int CurrentIndex
    {
        get => (int)GetValue(CurrentIndexProperty);
        set => SetValue(CurrentIndexProperty, value);
    }

    public static readonly DependencyProperty CurrentIndexProperty =
        ElementBase.Property<NCarousel, int>(nameof(CurrentIndexProperty), 0, OnCurrentIndexChanged);

    public bool Autoplay
    {
        get => (bool)GetValue(AutoplayProperty);
        set => SetValue(AutoplayProperty, value);
    }

    public static readonly DependencyProperty AutoplayProperty =
        ElementBase.Property<NCarousel, bool>(nameof(AutoplayProperty), false, OnAutoplaySettingsChanged);

    public int Interval
    {
        get => (int)GetValue(IntervalProperty);
        set => SetValue(IntervalProperty, value);
    }

    public static readonly DependencyProperty IntervalProperty =
        ElementBase.Property<NCarousel, int>(nameof(IntervalProperty), 5000, OnAutoplaySettingsChanged);

    public int TransitionDuration
    {
        get => (int)GetValue(TransitionDurationProperty);
        set => SetValue(TransitionDurationProperty, value);
    }

    public static readonly DependencyProperty TransitionDurationProperty =
        ElementBase.Property<NCarousel, int>(nameof(TransitionDurationProperty), 300);

    public bool ShowArrow
    {
        get => (bool)GetValue(ShowArrowProperty);
        set => SetValue(ShowArrowProperty, value);
    }

    public static readonly DependencyProperty ShowArrowProperty =
        ElementBase.Property<NCarousel, bool>(nameof(ShowArrowProperty), false, OnVisualSettingsChanged);

    public bool ShowDots
    {
        get => (bool)GetValue(ShowDotsProperty);
        set => SetValue(ShowDotsProperty, value);
    }

    public static readonly DependencyProperty ShowDotsProperty =
        ElementBase.Property<NCarousel, bool>(nameof(ShowDotsProperty), true);

    public bool Loop
    {
        get => (bool)GetValue(LoopProperty);
        set => SetValue(LoopProperty, value);
    }

    public static readonly DependencyProperty LoopProperty =
        ElementBase.Property<NCarousel, bool>(nameof(LoopProperty), true, OnNavigationSettingsChanged);

    public bool PauseOnHover
    {
        get => (bool)GetValue(PauseOnHoverProperty);
        set => SetValue(PauseOnHoverProperty, value);
    }

    public static readonly DependencyProperty PauseOnHoverProperty =
        ElementBase.Property<NCarousel, bool>(nameof(PauseOnHoverProperty), true, OnAutoplaySettingsChanged);

    public bool Draggable
    {
        get => (bool)GetValue(DraggableProperty);
        set => SetValue(DraggableProperty, value);
    }

    public static readonly DependencyProperty DraggableProperty =
        ElementBase.Property<NCarousel, bool>(nameof(DraggableProperty), true);

    public bool Keyboard
    {
        get => (bool)GetValue(KeyboardProperty);
        set => SetValue(KeyboardProperty, value);
    }

    public static readonly DependencyProperty KeyboardProperty =
        ElementBase.Property<NCarousel, bool>(nameof(KeyboardProperty), true, OnVisualSettingsChanged);

    public NCarouselEffect Effect
    {
        get => (NCarouselEffect)GetValue(EffectProperty);
        set => SetValue(EffectProperty, value);
    }

    public static readonly DependencyProperty EffectProperty =
        ElementBase.Property<NCarousel, NCarouselEffect>(nameof(EffectProperty), NCarouselEffect.Slide);

    public NCarouselDotType DotType
    {
        get => (NCarouselDotType)GetValue(DotTypeProperty);
        set => SetValue(DotTypeProperty, value);
    }

    public static readonly DependencyProperty DotTypeProperty =
        ElementBase.Property<NCarousel, NCarouselDotType>(nameof(DotTypeProperty), NCarouselDotType.Dot);

    public NCarouselDotPlacement DotPlacement
    {
        get => (NCarouselDotPlacement)GetValue(DotPlacementProperty);
        set => SetValue(DotPlacementProperty, value);
    }

    public static readonly DependencyProperty DotPlacementProperty =
        ElementBase.Property<NCarousel, NCarouselDotPlacement>(nameof(DotPlacementProperty), NCarouselDotPlacement.Bottom, OnVisualSettingsChanged);

    public NCarouselTrigger Trigger
    {
        get => (NCarouselTrigger)GetValue(TriggerProperty);
        set => SetValue(TriggerProperty, value);
    }

    public static readonly DependencyProperty TriggerProperty =
        ElementBase.Property<NCarousel, NCarouselTrigger>(nameof(TriggerProperty), NCarouselTrigger.Click);

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        ElementBase.Property<NCarousel, CornerRadius>(nameof(CornerRadiusProperty), new CornerRadius(3));

    public bool HasMultipleItems
    {
        get => (bool)GetValue(HasMultipleItemsProperty);
        private set => SetValue(HasMultipleItemsProperty, value);
    }

    public static readonly DependencyProperty HasMultipleItemsProperty =
        ElementBase.Property<NCarousel, bool>(nameof(HasMultipleItemsProperty), false);

    public ObservableCollection<NCarouselIndicatorItem> IndicatorItems
    {
        get => (ObservableCollection<NCarouselIndicatorItem>)GetValue(IndicatorItemsProperty);
        private set => SetValue(IndicatorItemsProperty, value);
    }

    public static readonly DependencyProperty IndicatorItemsProperty =
        ElementBase.Property<NCarousel, ObservableCollection<NCarouselIndicatorItem>>(
            nameof(IndicatorItemsProperty),
            new ObservableCollection<NCarouselIndicatorItem>());

    public Visibility ArrowVisibility
    {
        get => (Visibility)GetValue(ArrowVisibilityProperty);
        private set => SetValue(ArrowVisibilityProperty, value);
    }

    public static readonly DependencyProperty ArrowVisibilityProperty =
        ElementBase.Property<NCarousel, Visibility>(nameof(ArrowVisibilityProperty), Visibility.Collapsed);

    public Visibility PreviousArrowVisibility
    {
        get => (Visibility)GetValue(PreviousArrowVisibilityProperty);
        private set => SetValue(PreviousArrowVisibilityProperty, value);
    }

    public static readonly DependencyProperty PreviousArrowVisibilityProperty =
        ElementBase.Property<NCarousel, Visibility>(nameof(PreviousArrowVisibilityProperty), Visibility.Collapsed);

    public Visibility NextArrowVisibility
    {
        get => (Visibility)GetValue(NextArrowVisibilityProperty);
        private set => SetValue(NextArrowVisibilityProperty, value);
    }

    public static readonly DependencyProperty NextArrowVisibilityProperty =
        ElementBase.Property<NCarousel, Visibility>(nameof(NextArrowVisibilityProperty), Visibility.Collapsed);

    public Thickness DotContainerMargin
    {
        get => (Thickness)GetValue(DotContainerMarginProperty);
        private set => SetValue(DotContainerMarginProperty, value);
    }

    public static readonly DependencyProperty DotContainerMarginProperty =
        ElementBase.Property<NCarousel, Thickness>(nameof(DotContainerMarginProperty), new Thickness(0, 0, 0, 16));

    public Thickness ArrowLayerMargin
    {
        get => (Thickness)GetValue(ArrowLayerMarginProperty);
        private set => SetValue(ArrowLayerMarginProperty, value);
    }

    public static readonly DependencyProperty ArrowLayerMarginProperty =
        ElementBase.Property<NCarousel, Thickness>(nameof(ArrowLayerMarginProperty), new Thickness(16, 0, 16, 0));

    public HorizontalAlignment DotHorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(DotHorizontalAlignmentProperty);
        private set => SetValue(DotHorizontalAlignmentProperty, value);
    }

    public static readonly DependencyProperty DotHorizontalAlignmentProperty =
        ElementBase.Property<NCarousel, HorizontalAlignment>(nameof(DotHorizontalAlignmentProperty), HorizontalAlignment.Center);

    public VerticalAlignment DotVerticalAlignment
    {
        get => (VerticalAlignment)GetValue(DotVerticalAlignmentProperty);
        private set => SetValue(DotVerticalAlignmentProperty, value);
    }

    public static readonly DependencyProperty DotVerticalAlignmentProperty =
        ElementBase.Property<NCarousel, VerticalAlignment>(nameof(DotVerticalAlignmentProperty), VerticalAlignment.Bottom);

    public Orientation DotOrientation
    {
        get => (Orientation)GetValue(DotOrientationProperty);
        private set => SetValue(DotOrientationProperty, value);
    }

    public static readonly DependencyProperty DotOrientationProperty =
        ElementBase.Property<NCarousel, Orientation>(nameof(DotOrientationProperty), Orientation.Horizontal);

    public ICommand PreviousCommand { get; }

    public ICommand NextCommand { get; }

    public ICommand SelectIndexCommand { get; }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        currentPresenter = GetTemplateChild(CurrentPresenterPartName) as ContentPresenter;
        standbyPresenter = GetTemplateChild(StandbyPresenterPartName) as ContentPresenter;

        RenderCurrentImmediately();
    }

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        HandleItemsChanged();
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        Focusable = Keyboard;
        HandleItemsChanged();
        UpdateIndicatorLayout();
        UpdateAutoplayState();
        RenderCurrentImmediately();
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        autoplayTimer.Stop();
    }

    private void HandleMouseEnter(object sender, MouseEventArgs e)
    {
        UpdateArrowVisibility();

        if (PauseOnHover)
        {
            autoplayTimer.Stop();
        }
    }

    private void HandleMouseLeave(object sender, MouseEventArgs e)
    {
        UpdateArrowVisibility();
        UpdateAutoplayState();
    }

    private void HandlePreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!Draggable || !HasMultipleItems || isAnimating || IsInteractiveOrigin(e.OriginalSource as DependencyObject))
        {
            return;
        }

        dragStartPoint = e.GetPosition(this);
        isPointerCaptured = CaptureMouse();
    }

    private void HandlePreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!isPointerCaptured && Trigger == NCarouselTrigger.Hover && !isAnimating
            && TryGetIndicatorIndex(e.OriginalSource as DependencyObject, out var hoveredIndex)
            && hoveredIndex != CurrentIndex)
        {
            MoveToIndex(hoveredIndex);
        }

        if (!isPointerCaptured || !Draggable || isAnimating)
        {
            return;
        }

        var currentPoint = e.GetPosition(this);
        var deltaX = currentPoint.X - dragStartPoint.X;
        if (Math.Abs(deltaX) < DragThreshold)
        {
            return;
        }

        ReleaseDragCapture();
        navigationDirection = deltaX < 0 ? 1 : -1;
        if (deltaX < 0)
        {
            MoveNext();
        }
        else
        {
            MovePrevious();
        }
    }

    private void HandlePreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        ReleaseDragCapture();
    }

    private void HandlePreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (!Keyboard || !HasMultipleItems || isAnimating)
        {
            return;
        }

        if (e.Key == Key.Left)
        {
            MovePrevious();
            e.Handled = true;
        }
        else if (e.Key == Key.Right)
        {
            MoveNext();
            e.Handled = true;
        }
    }

    private void ReleaseDragCapture()
    {
        if (!isPointerCaptured)
        {
            return;
        }

        isPointerCaptured = false;
        if (IsMouseCaptured)
        {
            ReleaseMouseCapture();
        }
    }

    private static void OnCurrentIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NCarousel carousel)
        {
            return;
        }

        carousel.HandleCurrentIndexChanged((int)e.OldValue, (int)e.NewValue);
    }

    private static void OnAutoplaySettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NCarousel carousel)
        {
            carousel.UpdateAutoplayState();
        }
    }

    private static void OnNavigationSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NCarousel carousel)
        {
            carousel.UpdateNavigationState();
        }
    }

    private static void OnVisualSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NCarousel carousel)
        {
            return;
        }

        if (e.Property == KeyboardProperty)
        {
            carousel.Focusable = carousel.Keyboard;
        }

        if (e.Property == DotPlacementProperty)
        {
            carousel.UpdateIndicatorLayout();
        }

        carousel.UpdateNavigationState();
    }

    private void HandleCurrentIndexChanged(int oldIndex, int newIndex)
    {
        if (Items.Count == 0)
        {
            return;
        }

        var normalizedIndex = NormalizeIndex(newIndex);
        if (normalizedIndex != newIndex)
        {
            SetCurrentValue(CurrentIndexProperty, normalizedIndex);
            return;
        }

        UpdateIndicators();
        UpdateNavigationState();

        if (!IsLoaded)
        {
            return;
        }

        if (oldIndex == newIndex)
        {
            RenderCurrentImmediately();
            return;
        }

        BeginTransition(oldIndex, newIndex);
        UpdateAutoplayState();
    }

    private void HandleItemsChanged()
    {
        HasMultipleItems = Items.Count > 1;
        if (Items.Count == 0)
        {
            SetCurrentValue(CurrentIndexProperty, 0);
            UpdateIndicators();
            ClearPresenters();
            UpdateNavigationState();
            UpdateAutoplayState();
            return;
        }

        var normalizedIndex = NormalizeIndex(CurrentIndex);
        if (normalizedIndex != CurrentIndex)
        {
            SetCurrentValue(CurrentIndexProperty, normalizedIndex);
        }

        UpdateIndicators();
        UpdateNavigationState();
        RenderCurrentImmediately();
        UpdateAutoplayState();
    }

    private void UpdateIndicators()
    {
        while (IndicatorItems.Count < Items.Count)
        {
            IndicatorItems.Add(new NCarouselIndicatorItem());
        }

        while (IndicatorItems.Count > Items.Count)
        {
            IndicatorItems.RemoveAt(IndicatorItems.Count - 1);
        }

        for (var i = 0; i < IndicatorItems.Count; i++)
        {
            var isActive = i == CurrentIndex;
            IndicatorItems[i].Index = i;
            IndicatorItems[i].IsActive = isActive;
            IndicatorItems[i].IsPrev = HasMultipleItems && i == NormalizeIndex(CurrentIndex - 1);
            IndicatorItems[i].IsNext = HasMultipleItems && i == NormalizeIndex(CurrentIndex + 1);
        }

        UpdateIndicatorLayout();
    }

    private void UpdateIndicatorLayout()
    {
        DotOrientation = DotPlacement is NCarouselDotPlacement.Left or NCarouselDotPlacement.Right
            ? Orientation.Vertical
            : Orientation.Horizontal;

        switch (DotPlacement)
        {
            case NCarouselDotPlacement.Top:
                DotHorizontalAlignment = HorizontalAlignment.Center;
                DotVerticalAlignment = VerticalAlignment.Top;
                DotContainerMargin = new Thickness(0, 16, 0, 0);
                ArrowLayerMargin = new Thickness(16, 0, 16, 0);
                break;
            case NCarouselDotPlacement.Left:
                DotHorizontalAlignment = HorizontalAlignment.Left;
                DotVerticalAlignment = VerticalAlignment.Center;
                DotContainerMargin = new Thickness(16, 0, 0, 0);
                ArrowLayerMargin = new Thickness(56, 0, 16, 0);
                break;
            case NCarouselDotPlacement.Right:
                DotHorizontalAlignment = HorizontalAlignment.Right;
                DotVerticalAlignment = VerticalAlignment.Center;
                DotContainerMargin = new Thickness(0, 0, 16, 0);
                ArrowLayerMargin = new Thickness(16, 0, 56, 0);
                break;
            default:
                DotHorizontalAlignment = HorizontalAlignment.Center;
                DotVerticalAlignment = VerticalAlignment.Bottom;
                DotContainerMargin = new Thickness(0, 0, 0, 16);
                ArrowLayerMargin = new Thickness(16, 0, 16, 0);
                break;
        }
    }

    private void UpdateAutoplayState()
    {
        autoplayTimer.Stop();

        if (!IsLoaded || !Autoplay || !HasMultipleItems || Interval <= 0 || isPointerCaptured)
        {
            return;
        }

        if (PauseOnHover && IsMouseOver)
        {
            return;
        }

        autoplayTimer.Interval = TimeSpan.FromMilliseconds(Interval);
        autoplayTimer.Start();
    }

    private void HandleAutoplayTick(object? sender, EventArgs e)
    {
        if (isAnimating)
        {
            return;
        }

        navigationDirection = 1;
        MoveNext();
    }

    private bool CanMovePrevious()
    {
        return HasMultipleItems && !isAnimating && (Loop || CurrentIndex > 0);
    }

    private bool CanMoveNext()
    {
        return HasMultipleItems && !isAnimating && (Loop || CurrentIndex < Items.Count - 1);
    }

    private void MovePrevious()
    {
        if (!CanMovePrevious())
        {
            return;
        }

        navigationDirection = -1;
        CurrentIndex = Loop ? NormalizeIndex(CurrentIndex - 1) : Math.Max(0, CurrentIndex - 1);
    }

    private void MoveNext()
    {
        if (!CanMoveNext())
        {
            return;
        }

        navigationDirection = 1;
        CurrentIndex = Loop ? NormalizeIndex(CurrentIndex + 1) : Math.Min(Items.Count - 1, CurrentIndex + 1);
    }

    private void MoveToIndex(object? targetIndex)
    {
        if (!HasMultipleItems || isAnimating)
        {
            return;
        }

        if (!TryConvertToIndex(targetIndex, out var index) || index == CurrentIndex || index < 0 || index >= Items.Count)
        {
            return;
        }

        navigationDirection = index > CurrentIndex ? 1 : -1;
        CurrentIndex = index;
    }

    public void HandleIndicatorHover(object? parameter)
    {
        if (Trigger != NCarouselTrigger.Hover)
        {
            return;
        }

        MoveToIndex(parameter);
    }

    private static bool TryConvertToIndex(object? value, out int index)
    {
        switch (value)
        {
            case int directIndex:
                index = directIndex;
                return true;
            case string text when int.TryParse(text, out var parsedIndex):
                index = parsedIndex;
                return true;
            default:
                index = 0;
                return false;
        }
    }

    private static bool TryGetIndicatorIndex(DependencyObject? origin, out int index)
    {
        while (origin is not null)
        {
            if (origin is FrameworkElement { DataContext: NCarouselIndicatorItem item })
            {
                index = item.Index;
                return true;
            }

            origin = VisualTreeHelper.GetParent(origin);
        }

        index = 0;
        return false;
    }

    private static bool IsInteractiveOrigin(DependencyObject? origin)
    {
        while (origin is not null)
        {
            if (origin is ButtonBase or TextBoxBase or PasswordBox or Selector or ScrollBar or Slider)
            {
                return true;
            }

            origin = origin switch
            {
                Visual visual => VisualTreeHelper.GetParent(visual),
                Visual3D visual3D => VisualTreeHelper.GetParent(visual3D),
                FrameworkContentElement contentElement => contentElement.Parent,
                _ => null
            };
        }

        return false;
    }

    private int NormalizeIndex(int rawIndex)
    {
        if (Items.Count == 0)
        {
            return 0;
        }

        if (!Loop)
        {
            return Math.Max(0, Math.Min(Items.Count - 1, rawIndex));
        }

        var normalized = rawIndex % Items.Count;
        return normalized < 0 ? normalized + Items.Count : normalized;
    }

    private object? GetItemAt(int index)
    {
        return index >= 0 && index < Items.Count ? Items[index] : null;
    }

    private void RenderCurrentImmediately()
    {
        if (currentPresenter is null || standbyPresenter is null)
        {
            return;
        }

        var currentItem = GetItemAt(CurrentIndex);

        currentPresenter.Content = currentItem;
        currentPresenter.Tag = CurrentIndex;
        currentPresenter.Opacity = 1;
        currentPresenter.Visibility = currentItem is null ? Visibility.Collapsed : Visibility.Visible;
        currentPresenter.RenderTransform = CreatePresenterTransform();
        Panel.SetZIndex(currentPresenter, 2);

        standbyPresenter.Content = null;
        standbyPresenter.Tag = null;
        standbyPresenter.Opacity = 0;
        standbyPresenter.Visibility = Visibility.Collapsed;
        standbyPresenter.RenderTransform = CreatePresenterTransform();
        Panel.SetZIndex(standbyPresenter, 1);

        ApplyCardStaticTransforms();
    }

    private void ClearPresenters()
    {
        if (currentPresenter is not null)
        {
            currentPresenter.Content = null;
            currentPresenter.Tag = null;
            currentPresenter.Visibility = Visibility.Collapsed;
        }

        if (standbyPresenter is not null)
        {
            standbyPresenter.Content = null;
            standbyPresenter.Tag = null;
            standbyPresenter.Visibility = Visibility.Collapsed;
        }
    }

    private void BeginTransition(int oldIndex, int newIndex)
    {
        if (currentPresenter is null || standbyPresenter is null)
        {
            return;
        }

        var incomingContent = GetItemAt(newIndex);
        if (incomingContent is null)
        {
            RenderCurrentImmediately();
            return;
        }

        isAnimating = true;
        UpdateNavigationState();

        var outgoingPresenter = currentPresenter;
        var incomingPresenter = standbyPresenter;
        incomingPresenter.Content = incomingContent;
        incomingPresenter.Tag = newIndex;
        incomingPresenter.Visibility = Visibility.Visible;
        incomingPresenter.RenderTransform = CreatePresenterTransform();

        var duration = TimeSpan.FromMilliseconds(Math.Max(0, TransitionDuration));
        if (duration <= TimeSpan.Zero)
        {
            outgoingPresenter.Content = null;
            outgoingPresenter.Tag = null;
            outgoingPresenter.Visibility = Visibility.Collapsed;
            incomingPresenter.Opacity = 1;
            SwapPresenters();
            isAnimating = false;
            UpdateNavigationState();
            UpdateAutoplayState();
            return;
        }

        Storyboard storyboard = Effect switch
        {
            NCarouselEffect.Fade => CreateFadeStoryboard(outgoingPresenter, incomingPresenter, duration),
            NCarouselEffect.Card => CreateCardStoryboard(outgoingPresenter, incomingPresenter, duration),
            _ => CreateSlideStoryboard(outgoingPresenter, incomingPresenter, duration)
        };

        storyboard.Completed += (_, _) =>
        {
            outgoingPresenter.Content = null;
            outgoingPresenter.Tag = null;
            outgoingPresenter.Visibility = Visibility.Collapsed;
            outgoingPresenter.Opacity = 1;
            outgoingPresenter.RenderTransform = CreatePresenterTransform();
            Panel.SetZIndex(outgoingPresenter, 1);

            incomingPresenter.Opacity = 1;
            incomingPresenter.Visibility = Visibility.Visible;
            incomingPresenter.RenderTransform = CreatePresenterTransform();
            Panel.SetZIndex(incomingPresenter, 2);

            SwapPresenters();
            isAnimating = false;
            UpdateNavigationState();
            RenderCurrentImmediately();
            UpdateAutoplayState();
        };

        storyboard.Begin();
    }

    private TransformGroup CreatePresenterTransform()
    {
        var scale = new ScaleTransform(1, 1);
        var translate = new TranslateTransform();
        return new TransformGroup
        {
            Children =
            {
                scale,
                translate
            }
        };
    }

    private Storyboard CreateFadeStoryboard(ContentPresenter outgoingPresenter, ContentPresenter incomingPresenter, TimeSpan duration)
    {
        outgoingPresenter.Opacity = 1;
        incomingPresenter.Opacity = 0;
        Panel.SetZIndex(outgoingPresenter, 1);
        Panel.SetZIndex(incomingPresenter, 2);

        var storyboard = new Storyboard();

        var fadeOut = new DoubleAnimation(1, 0, duration)
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };
        Storyboard.SetTarget(fadeOut, outgoingPresenter);
        Storyboard.SetTargetProperty(fadeOut, new PropertyPath(UIElement.OpacityProperty));

        var fadeIn = new DoubleAnimation(0, 1, duration)
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };
        Storyboard.SetTarget(fadeIn, incomingPresenter);
        Storyboard.SetTargetProperty(fadeIn, new PropertyPath(UIElement.OpacityProperty));

        storyboard.Children.Add(fadeOut);
        storyboard.Children.Add(fadeIn);
        return storyboard;
    }

    private Storyboard CreateSlideStoryboard(ContentPresenter outgoingPresenter, ContentPresenter incomingPresenter, TimeSpan duration)
    {
        var width = ResolveSlideWidth();
        var offset = navigationDirection >= 0 ? width : -width;

        outgoingPresenter.Opacity = 1;
        incomingPresenter.Opacity = 1;
        Panel.SetZIndex(outgoingPresenter, 1);
        Panel.SetZIndex(incomingPresenter, 2);

        SetTranslateX(outgoingPresenter, 0);
        SetTranslateX(incomingPresenter, offset);

        var easing = new CubicEase { EasingMode = EasingMode.EaseInOut };
        var storyboard = new Storyboard();

        storyboard.Children.Add(CreateTranslateAnimation(outgoingPresenter, 0, -offset, duration, easing));
        storyboard.Children.Add(CreateTranslateAnimation(incomingPresenter, offset, 0, duration, easing));
        return storyboard;
    }

    private Storyboard CreateCardStoryboard(ContentPresenter outgoingPresenter, ContentPresenter incomingPresenter, TimeSpan duration)
    {
        var width = ResolveSlideWidth();
        var offset = navigationDirection >= 0 ? width - CardPeekWidth : -(width - CardPeekWidth);
        var easing = new CubicEase { EasingMode = EasingMode.EaseInOut };

        outgoingPresenter.Opacity = 0.7;
        incomingPresenter.Opacity = 0.78;
        Panel.SetZIndex(outgoingPresenter, 1);
        Panel.SetZIndex(incomingPresenter, 2);

        SetScale(outgoingPresenter, 0.86);
        SetScale(incomingPresenter, 0.86);
        SetTranslateX(outgoingPresenter, 0);
        SetTranslateX(incomingPresenter, offset);

        var storyboard = new Storyboard();
        storyboard.Children.Add(CreateTranslateAnimation(outgoingPresenter, 0, -offset * 0.24, duration, easing));
        storyboard.Children.Add(CreateTranslateAnimation(incomingPresenter, offset, 0, duration, easing));
        AddScaleAnimations(storyboard, outgoingPresenter, 0.86, 0.78, duration, easing);
        AddScaleAnimations(storyboard, incomingPresenter, 0.86, 1, duration, easing);
        storyboard.Children.Add(CreateOpacityAnimation(outgoingPresenter, 0.7, 0.36, duration, easing));
        storyboard.Children.Add(CreateOpacityAnimation(incomingPresenter, 0.78, 1, duration, easing));
        return storyboard;
    }

    private DoubleAnimation CreateTranslateAnimation(ContentPresenter target, double from, double to, TimeSpan duration, IEasingFunction easing)
    {
        var animation = new DoubleAnimation(from, to, duration) { EasingFunction = easing };
        Storyboard.SetTarget(animation, target);
        Storyboard.SetTargetProperty(animation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.X)"));
        return animation;
    }

    private DoubleAnimation CreateScaleAnimation(ContentPresenter target, double from, double to, TimeSpan duration, IEasingFunction easing)
    {
        var animationX = new DoubleAnimation(from, to, duration) { EasingFunction = easing };
        Storyboard.SetTarget(animationX, target);
        Storyboard.SetTargetProperty(animationX, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));
        return animationX;
    }

    private void AddScaleAnimations(Storyboard storyboard, ContentPresenter target, double from, double to, TimeSpan duration, IEasingFunction easing)
    {
        storyboard.Children.Add(CreateScaleAnimation(target, from, to, duration, easing));

        var animationY = new DoubleAnimation(from, to, duration) { EasingFunction = easing };
        Storyboard.SetTarget(animationY, target);
        Storyboard.SetTargetProperty(animationY, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));
        storyboard.Children.Add(animationY);
    }

    private DoubleAnimation CreateOpacityAnimation(ContentPresenter target, double from, double to, TimeSpan duration, IEasingFunction easing)
    {
        var animation = new DoubleAnimation(from, to, duration) { EasingFunction = easing };
        Storyboard.SetTarget(animation, target);
        Storyboard.SetTargetProperty(animation, new PropertyPath(UIElement.OpacityProperty));
        return animation;
    }

    private void SetTranslateX(ContentPresenter presenter, double value)
    {
        if (presenter.RenderTransform is not TransformGroup transformGroup || transformGroup.Children.Count < 2)
        {
            presenter.RenderTransform = CreatePresenterTransform();
            transformGroup = (TransformGroup)presenter.RenderTransform;
        }

        if (transformGroup.Children[1] is TranslateTransform translate)
        {
            translate.X = value;
        }
    }

    private void SetScale(ContentPresenter presenter, double value)
    {
        if (presenter.RenderTransform is not TransformGroup transformGroup || transformGroup.Children.Count < 2)
        {
            presenter.RenderTransform = CreatePresenterTransform();
            transformGroup = (TransformGroup)presenter.RenderTransform;
        }

        if (transformGroup.Children[0] is ScaleTransform scale)
        {
            scale.ScaleX = value;
            scale.ScaleY = value;
        }
    }

    private double ResolveSlideWidth()
    {
        var width = ActualWidth > 0 ? ActualWidth : Width;
        if (double.IsNaN(width) || width <= 0)
        {
            width = 320;
        }

        return width;
    }

    private void ApplyCardStaticTransforms()
    {
        if (Effect != NCarouselEffect.Card || currentPresenter is null)
        {
            if (currentPresenter is not null)
            {
                currentPresenter.Opacity = 1;
                SetScale(currentPresenter, 1);
                SetTranslateX(currentPresenter, 0);
            }

            return;
        }

        currentPresenter.Opacity = 1;
        SetScale(currentPresenter, 1);
        SetTranslateX(currentPresenter, 0);
    }

    private void SwapPresenters()
    {
        (currentPresenter, standbyPresenter) = (standbyPresenter, currentPresenter);
    }

    private void UpdateNavigationState()
    {
        UpdateArrowVisibility();
        previousCommand.RaiseCanExecuteChanged();
        nextCommand.RaiseCanExecuteChanged();
        selectIndexCommand.RaiseCanExecuteChanged();
    }

    private void UpdateArrowVisibility()
    {
        var shouldShowArrows = ShowArrow && HasMultipleItems;
        ArrowVisibility = shouldShowArrows && IsMouseOver ? Visibility.Visible : Visibility.Collapsed;
        PreviousArrowVisibility = shouldShowArrows && (Loop || CurrentIndex > 0) ? Visibility.Visible : Visibility.Collapsed;
        NextArrowVisibility = shouldShowArrows && (Loop || CurrentIndex < Items.Count - 1) ? Visibility.Visible : Visibility.Collapsed;
    }

    private sealed class DelegateCommand : ICommand
    {
        private readonly Action<object?> execute;
        private readonly Predicate<object?>? canExecute;

        public DelegateCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => canExecute?.Invoke(parameter) ?? true;

        public void Execute(object? parameter) => execute(parameter);

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

public sealed class NCarouselIndicatorItem : INotifyPropertyChanged
{
    private int index;
    private bool isActive;
    private bool isPrev;
    private bool isNext;

    public int Index
    {
        get => index;
        set => SetProperty(ref index, value);
    }

    public bool IsActive
    {
        get => isActive;
        set => SetProperty(ref isActive, value);
    }

    public bool IsPrev
    {
        get => isPrev;
        set => SetProperty(ref isPrev, value);
    }

    public bool IsNext
    {
        get => isNext;
        set => SetProperty(ref isNext, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}


