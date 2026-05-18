using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public enum NCarouselEffect
{
    Slide,
    Fade
}

public enum NCarouselDotType
{
    Line,
    Dot
}

[TemplatePart(Name = CurrentPresenterPartName, Type = typeof(ContentPresenter))]
[TemplatePart(Name = StandbyPresenterPartName, Type = typeof(ContentPresenter))]
public class NCarousel : ItemsControl
{
    private const string CurrentPresenterPartName = "PART_CurrentPresenter";
    private const string StandbyPresenterPartName = "PART_StandbyPresenter";

    private readonly DispatcherTimer autoplayTimer = new();
    private ContentPresenter? currentPresenter;
    private ContentPresenter? standbyPresenter;
    private bool isAnimating;
    private int navigationDirection = 1;

    static NCarousel()
    {
        ElementBase.DefaultStyle<NCarousel>(DefaultStyleKeyProperty);
    }

    public NCarousel()
    {
        IndicatorItems = new ObservableCollection<NCarouselIndicatorItem>();
        PreviousCommand = new DelegateCommand(_ => MovePrevious(), _ => HasMultipleItems);
        NextCommand = new DelegateCommand(_ => MoveNext(), _ => HasMultipleItems);
        SelectIndexCommand = new DelegateCommand(index => MoveToIndex(index), _ => HasMultipleItems);

        autoplayTimer.Tick += HandleAutoplayTick;
        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;
        MouseEnter += HandleMouseEnter;
        MouseLeave += HandleMouseLeave;
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
        ElementBase.Property<NCarousel, bool>(nameof(ShowArrowProperty), false);

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
        ElementBase.Property<NCarousel, bool>(nameof(LoopProperty), true);

    public bool PauseOnHover
    {
        get => (bool)GetValue(PauseOnHoverProperty);
        set => SetValue(PauseOnHoverProperty, value);
    }

    public static readonly DependencyProperty PauseOnHoverProperty =
        ElementBase.Property<NCarousel, bool>(nameof(PauseOnHoverProperty), true);

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
        ElementBase.Property<NCarousel, NCarouselDotType>(nameof(DotTypeProperty), NCarouselDotType.Line);

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
        HandleItemsChanged();
        UpdateAutoplayState();
        RenderCurrentImmediately();
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        autoplayTimer.Stop();
    }

    private void HandleMouseEnter(object sender, MouseEventArgs e)
    {
        if (PauseOnHover)
        {
            autoplayTimer.Stop();
        }
    }

    private void HandleMouseLeave(object sender, MouseEventArgs e)
    {
        UpdateAutoplayState();
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
            UpdateAutoplayState();
            return;
        }

        var normalizedIndex = NormalizeIndex(CurrentIndex);
        if (normalizedIndex != CurrentIndex)
        {
            SetCurrentValue(CurrentIndexProperty, normalizedIndex);
        }

        UpdateIndicators();
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
            IndicatorItems[i].Index = i;
            IndicatorItems[i].IsActive = i == CurrentIndex;
        }
    }

    private void UpdateAutoplayState()
    {
        autoplayTimer.Stop();

        if (!IsLoaded || !Autoplay || !HasMultipleItems || Interval <= 0)
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

    private void MovePrevious()
    {
        if (!HasMultipleItems || isAnimating)
        {
            return;
        }

        navigationDirection = -1;
        if (Loop)
        {
            CurrentIndex = NormalizeIndex(CurrentIndex - 1);
            return;
        }

        CurrentIndex = Math.Max(0, CurrentIndex - 1);
    }

    private void MoveNext()
    {
        if (!HasMultipleItems || isAnimating)
        {
            return;
        }

        navigationDirection = 1;
        if (Loop)
        {
            CurrentIndex = NormalizeIndex(CurrentIndex + 1);
            return;
        }

        CurrentIndex = Math.Min(Items.Count - 1, CurrentIndex + 1);
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
        currentPresenter.Opacity = 1;
        currentPresenter.Visibility = currentItem is null ? Visibility.Collapsed : Visibility.Visible;
        currentPresenter.RenderTransform = new TranslateTransform();

        standbyPresenter.Content = null;
        standbyPresenter.Opacity = 0;
        standbyPresenter.Visibility = Visibility.Collapsed;
        standbyPresenter.RenderTransform = new TranslateTransform();
    }

    private void ClearPresenters()
    {
        if (currentPresenter is not null)
        {
            currentPresenter.Content = null;
            currentPresenter.Visibility = Visibility.Collapsed;
        }

        if (standbyPresenter is not null)
        {
            standbyPresenter.Content = null;
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

        var outgoingPresenter = currentPresenter;
        var incomingPresenter = standbyPresenter;
        incomingPresenter.Content = incomingContent;
        incomingPresenter.Visibility = Visibility.Visible;

        var duration = TimeSpan.FromMilliseconds(Math.Max(0, TransitionDuration));
        if (duration <= TimeSpan.Zero)
        {
            outgoingPresenter.Content = null;
            outgoingPresenter.Visibility = Visibility.Collapsed;
            incomingPresenter.Opacity = 1;
            SwapPresenters();
            isAnimating = false;
            UpdateAutoplayState();
            return;
        }

        Storyboard storyboard = Effect switch
        {
            NCarouselEffect.Fade => CreateFadeStoryboard(outgoingPresenter, incomingPresenter, duration),
            _ => CreateSlideStoryboard(outgoingPresenter, incomingPresenter, duration)
        };

        storyboard.Completed += (_, _) =>
        {
            outgoingPresenter.Content = null;
            outgoingPresenter.Visibility = Visibility.Collapsed;
            outgoingPresenter.Opacity = 1;
            outgoingPresenter.RenderTransform = new TranslateTransform();

            incomingPresenter.Opacity = 1;
            incomingPresenter.Visibility = Visibility.Visible;
            incomingPresenter.RenderTransform = new TranslateTransform();

            SwapPresenters();
            isAnimating = false;
            UpdateAutoplayState();
        };

        storyboard.Begin();
    }

    private Storyboard CreateFadeStoryboard(ContentPresenter outgoingPresenter, ContentPresenter incomingPresenter, TimeSpan duration)
    {
        outgoingPresenter.Opacity = 1;
        incomingPresenter.Opacity = 0;

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
        var width = ActualWidth > 0 ? ActualWidth : Width;
        if (double.IsNaN(width) || width <= 0)
        {
            width = 320;
        }

        var offset = navigationDirection >= 0 ? width : -width;

        outgoingPresenter.Opacity = 1;
        incomingPresenter.Opacity = 1;

        var outgoingTransform = new TranslateTransform();
        var incomingTransform = new TranslateTransform { X = offset };
        outgoingPresenter.RenderTransform = outgoingTransform;
        incomingPresenter.RenderTransform = incomingTransform;

        var easing = new CubicEase { EasingMode = EasingMode.EaseInOut };
        var storyboard = new Storyboard();

        var outgoingAnimation = new DoubleAnimation(0, -offset, duration) { EasingFunction = easing };
        Storyboard.SetTarget(outgoingAnimation, outgoingPresenter);
        Storyboard.SetTargetProperty(outgoingAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));

        var incomingAnimation = new DoubleAnimation(offset, 0, duration) { EasingFunction = easing };
        Storyboard.SetTarget(incomingAnimation, incomingPresenter);
        Storyboard.SetTargetProperty(incomingAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));

        storyboard.Children.Add(outgoingAnimation);
        storyboard.Children.Add(incomingAnimation);
        return storyboard;
    }

    private void SwapPresenters()
    {
        (currentPresenter, standbyPresenter) = (standbyPresenter, currentPresenter);
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
