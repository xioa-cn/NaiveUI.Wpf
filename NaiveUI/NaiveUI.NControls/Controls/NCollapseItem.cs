using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public enum NCollapseItemDisplayDirective
{
    Inherit,
    If,
    Show
}

[TemplatePart(Name = LeftArrowTogglePartName, Type = typeof(Border))]
[TemplatePart(Name = RightArrowTogglePartName, Type = typeof(Border))]
[TemplatePart(Name = MainTogglePartName, Type = typeof(Border))]
[TemplatePart(Name = ExtraTogglePartName, Type = typeof(Border))]
[TemplatePart(Name = LeftArrowTransformPartName, Type = typeof(RotateTransform))]
[TemplatePart(Name = RightArrowTransformPartName, Type = typeof(RotateTransform))]
[TemplatePart(Name = ContentContainerPartName, Type = typeof(Border))]
[TemplatePart(Name = ContentPresenterPartName, Type = typeof(ContentPresenter))]
public class NCollapseItem : HeaderedContentControl
{
    private const string LeftArrowTogglePartName = "PART_LeftArrowToggle";
    private const string RightArrowTogglePartName = "PART_RightArrowToggle";
    private const string MainTogglePartName = "PART_MainToggle";
    private const string ExtraTogglePartName = "PART_ExtraToggle";
    private const string LeftArrowTransformPartName = "PART_LeftArrowTransform";
    private const string RightArrowTransformPartName = "PART_RightArrowTransform";
    private const string ContentContainerPartName = "PART_ContentContainer";
    private const string ContentPresenterPartName = "PART_ContentPresenter";
    private const double ExpandDurationMilliseconds = 180d;

    private readonly string fallbackName = Guid.NewGuid().ToString("N");
    private Border? leftArrowToggle;
    private Border? rightArrowToggle;
    private Border? mainToggle;
    private Border? extraToggle;
    private Border? contentContainer;
    private ContentPresenter? contentPresenter;
    private RotateTransform? leftArrowTransform;
    private RotateTransform? rightArrowTransform;
    private NCollapse? parentCollapse;

    static NCollapseItem()
    {
        ElementBase.DefaultStyle<NCollapseItem>(DefaultStyleKeyProperty);
    }

    public NCollapseItem()
    {
        Loaded += (_, _) => ApplyImmediateExpansionState();
        UpdateHeaderState();
        UpdateResolvedContentPadding();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty TitleProperty =
        ElementBase.Property<NCollapseItem, string>(nameof(TitleProperty), string.Empty, OnItemStructureChanged);

    public object? HeaderExtra
    {
        get => GetValue(HeaderExtraProperty);
        set => SetValue(HeaderExtraProperty, value);
    }

    public static readonly DependencyProperty HeaderExtraProperty =
        ElementBase.Property<NCollapseItem, object?>(nameof(HeaderExtraProperty), null, OnItemStructureChanged);

    public string CollapseName
    {
        get => (string)GetValue(CollapseNameProperty);
        set => SetValue(CollapseNameProperty, value);
    }

    public static readonly DependencyProperty CollapseNameProperty =
        ElementBase.Property<NCollapseItem, string>(nameof(CollapseNameProperty), string.Empty);

    public bool Disabled
    {
        get => (bool)GetValue(DisabledProperty);
        set => SetValue(DisabledProperty, value);
    }

    public static readonly DependencyProperty DisabledProperty =
        ElementBase.Property<NCollapseItem, bool>(nameof(DisabledProperty), false, OnInteractionStateChanged);

    public NCollapseItemDisplayDirective DisplayDirective
    {
        get => (NCollapseItemDisplayDirective)GetValue(DisplayDirectiveProperty);
        set => SetValue(DisplayDirectiveProperty, value);
    }

    public static readonly DependencyProperty DisplayDirectiveProperty =
        ElementBase.Property<NCollapseItem, NCollapseItemDisplayDirective>(nameof(DisplayDirectiveProperty), NCollapseItemDisplayDirective.Inherit, OnItemDisplayDirectiveChanged);

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        private set => SetValue(IsExpandedProperty, value);
    }

    public static readonly DependencyProperty IsExpandedProperty =
        ElementBase.Property<NCollapseItem, bool>(nameof(IsExpandedProperty), false);

    public bool HasCustomHeader
    {
        get => (bool)GetValue(HasCustomHeaderProperty);
        private set => SetValue(HasCustomHeaderProperty, value);
    }

    public static readonly DependencyProperty HasCustomHeaderProperty =
        ElementBase.Property<NCollapseItem, bool>(nameof(HasCustomHeaderProperty), false);

    public bool HasTitle
    {
        get => (bool)GetValue(HasTitleProperty);
        private set => SetValue(HasTitleProperty, value);
    }

    public static readonly DependencyProperty HasTitleProperty =
        ElementBase.Property<NCollapseItem, bool>(nameof(HasTitleProperty), false);

    public bool HasHeaderExtra
    {
        get => (bool)GetValue(HasHeaderExtraProperty);
        private set => SetValue(HasHeaderExtraProperty, value);
    }

    public static readonly DependencyProperty HasHeaderExtraProperty =
        ElementBase.Property<NCollapseItem, bool>(nameof(HasHeaderExtraProperty), false);

    public object? PresentedContent
    {
        get => GetValue(PresentedContentProperty);
        private set => SetValue(PresentedContentProperty, value);
    }

    public static readonly DependencyProperty PresentedContentProperty =
        ElementBase.Property<NCollapseItem, object?>(nameof(PresentedContentProperty), null);

    public NCollapseArrowPlacement ResolvedArrowPlacement
    {
        get => (NCollapseArrowPlacement)GetValue(ResolvedArrowPlacementProperty);
        private set => SetValue(ResolvedArrowPlacementProperty, value);
    }

    public static readonly DependencyProperty ResolvedArrowPlacementProperty =
        ElementBase.Property<NCollapseItem, NCollapseArrowPlacement>(nameof(ResolvedArrowPlacementProperty), NCollapseArrowPlacement.Left);

    public NCollapseDisplayDirective ResolvedDisplayDirective
    {
        get => (NCollapseDisplayDirective)GetValue(ResolvedDisplayDirectiveProperty);
        private set => SetValue(ResolvedDisplayDirectiveProperty, value);
    }

    public static readonly DependencyProperty ResolvedDisplayDirectiveProperty =
        ElementBase.Property<NCollapseItem, NCollapseDisplayDirective>(nameof(ResolvedDisplayDirectiveProperty), NCollapseDisplayDirective.If);

    public NCollapseTriggerAreas ResolvedTriggerAreas
    {
        get => (NCollapseTriggerAreas)GetValue(ResolvedTriggerAreasProperty);
        private set => SetValue(ResolvedTriggerAreasProperty, value);
    }

    public static readonly DependencyProperty ResolvedTriggerAreasProperty =
        ElementBase.Property<NCollapseItem, NCollapseTriggerAreas>(nameof(ResolvedTriggerAreasProperty), NCollapseTriggerAreas.Main | NCollapseTriggerAreas.Arrow);

    public Thickness ResolvedContentPadding
    {
        get => (Thickness)GetValue(ResolvedContentPaddingProperty);
        private set => SetValue(ResolvedContentPaddingProperty, value);
    }

    public static readonly DependencyProperty ResolvedContentPaddingProperty =
        ElementBase.Property<NCollapseItem, Thickness>(nameof(ResolvedContentPaddingProperty), new Thickness(40, 0, 0, 16));

    public override void OnApplyTemplate()
    {
        DetachInteractionHandlers();

        base.OnApplyTemplate();

        leftArrowToggle = GetTemplateChild(LeftArrowTogglePartName) as Border;
        rightArrowToggle = GetTemplateChild(RightArrowTogglePartName) as Border;
        mainToggle = GetTemplateChild(MainTogglePartName) as Border;
        extraToggle = GetTemplateChild(ExtraTogglePartName) as Border;
        contentContainer = GetTemplateChild(ContentContainerPartName) as Border;
        contentPresenter = GetTemplateChild(ContentPresenterPartName) as ContentPresenter;
        leftArrowTransform = GetTemplateChild(LeftArrowTransformPartName) as RotateTransform;
        rightArrowTransform = GetTemplateChild(RightArrowTransformPartName) as RotateTransform;

        AttachInteractionHandlers();
        UpdateInteractiveState();
        ApplyImmediateExpansionState();
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        UpdatePresentedContent(expandImmediately: false);
        ApplyImmediateExpansionState();
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == HeaderProperty)
        {
            UpdateHeaderState();
        }
    }

    internal void AttachCollapse(NCollapse collapse)
    {
        parentCollapse = collapse;
        ApplyCollapseConfiguration(collapse.ArrowPlacement, collapse.DisplayDirective, collapse.TriggerAreas);
    }

    internal void DetachCollapse(NCollapse collapse)
    {
        if (ReferenceEquals(parentCollapse, collapse))
        {
            parentCollapse = null;
        }
    }

    internal void ApplyCollapseConfiguration(
        NCollapseArrowPlacement arrowPlacement,
        NCollapseDisplayDirective displayDirective,
        NCollapseTriggerAreas triggerAreas)
    {
        ResolvedArrowPlacement = arrowPlacement;
        ResolvedDisplayDirective = DisplayDirective switch
        {
            NCollapseItemDisplayDirective.Show => NCollapseDisplayDirective.Show,
            NCollapseItemDisplayDirective.If => NCollapseDisplayDirective.If,
            _ => displayDirective
        };
        ResolvedTriggerAreas = triggerAreas;
        UpdateResolvedContentPadding();
        UpdatePresentedContent(expandImmediately: false);
        UpdateInteractiveState();
        ApplyImmediateExpansionState();
    }

    internal string ResolveItemName()
    {
        return string.IsNullOrWhiteSpace(CollapseName) ? fallbackName : CollapseName;
    }

    internal void SetExpanded(bool expanded, bool animate)
    {
        if (IsExpanded == expanded && !animate)
        {
            ApplyImmediateExpansionState();
            return;
        }

        if (IsExpanded == expanded)
        {
            return;
        }

        if (expanded)
        {
            IsExpanded = true;
            UpdatePresentedContent(expandImmediately: true);
            if (!IsLoaded || !animate)
            {
                ApplyImmediateExpansionState();
                return;
            }

            AnimateExpand();
            return;
        }

        IsExpanded = false;
        if (!IsLoaded || !animate)
        {
            UpdatePresentedContent(expandImmediately: false);
            ApplyImmediateExpansionState();
            return;
        }

        AnimateCollapse();
    }

    private static void OnItemStructureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NCollapseItem item)
        {
            return;
        }

        item.UpdateHeaderState();
    }

    private static void OnItemDisplayDirectiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NCollapseItem item)
        {
            return;
        }

        item.RefreshResolvedState();
    }

    private static void OnInteractionStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NCollapseItem item)
        {
            return;
        }

        item.UpdateInteractiveState();
    }

    private void RefreshResolvedState()
    {
        ApplyCollapseConfiguration(
            parentCollapse?.ArrowPlacement ?? ResolvedArrowPlacement,
            parentCollapse?.DisplayDirective ?? NCollapseDisplayDirective.If,
            parentCollapse?.TriggerAreas ?? ResolvedTriggerAreas);
    }

    private void UpdateHeaderState()
    {
        HasCustomHeader = IsMeaningfulContent(Header);
        HasTitle = !HasCustomHeader && !string.IsNullOrWhiteSpace(Title);
        HasHeaderExtra = IsMeaningfulContent(HeaderExtra);
    }

    private void UpdateResolvedContentPadding()
    {
        ResolvedContentPadding = ResolvedArrowPlacement == NCollapseArrowPlacement.Left
            ? new Thickness(28, 0, 0, 16)
            : new Thickness(0, 0, 0, 16);
    }

    private void UpdatePresentedContent(bool expandImmediately)
    {
        if (ResolvedDisplayDirective == NCollapseDisplayDirective.Show || IsExpanded || expandImmediately)
        {
            PresentedContent = Content;
            return;
        }

        PresentedContent = null;
    }

    private void ApplyImmediateExpansionState()
    {
        SetArrowAngle(IsExpanded ? 90d : 0d);

        if (contentContainer is null)
        {
            return;
        }

        contentContainer.BeginAnimation(FrameworkElement.HeightProperty, null);
        contentContainer.BeginAnimation(UIElement.OpacityProperty, null);

        if (IsExpanded)
        {
            contentContainer.Visibility = Visibility.Visible;
            contentContainer.Height = double.NaN;
            contentContainer.Opacity = 1d;
            return;
        }

        if (ResolvedDisplayDirective == NCollapseDisplayDirective.Show)
        {
            contentContainer.Visibility = Visibility.Visible;
            contentContainer.Height = 0d;
            contentContainer.Opacity = 1d;
            return;
        }

        contentContainer.Visibility = Visibility.Collapsed;
        contentContainer.Height = 0d;
        contentContainer.Opacity = 0d;
    }

    private void AnimateExpand()
    {
        if (contentContainer is null)
        {
            return;
        }

        contentContainer.BeginAnimation(FrameworkElement.HeightProperty, null);
        contentContainer.BeginAnimation(UIElement.OpacityProperty, null);
        contentContainer.Visibility = Visibility.Visible;
        contentContainer.Height = 0d;
        contentContainer.Opacity = 0d;

        UpdateLayout();
        var targetHeight = Math.Max(0d, MeasureExpandedHeight());
        var duration = TimeSpan.FromMilliseconds(ExpandDurationMilliseconds);
        var easing = new CubicEase { EasingMode = EasingMode.EaseInOut };

        var heightAnimation = new DoubleAnimation(0d, targetHeight, duration) { EasingFunction = easing };
        var opacityAnimation = new DoubleAnimation(0d, 1d, duration) { EasingFunction = easing };

        heightAnimation.Completed += (_, _) =>
        {
            if (!IsExpanded || contentContainer is null)
            {
                return;
            }

            contentContainer.BeginAnimation(FrameworkElement.HeightProperty, null);
            contentContainer.Height = double.NaN;
            contentContainer.Opacity = 1d;
        };

        contentContainer.BeginAnimation(FrameworkElement.HeightProperty, heightAnimation);
        contentContainer.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
        AnimateArrow(90d);
    }

    private void AnimateCollapse()
    {
        if (contentContainer is null)
        {
            UpdatePresentedContent(expandImmediately: false);
            return;
        }

        contentContainer.BeginAnimation(FrameworkElement.HeightProperty, null);
        contentContainer.BeginAnimation(UIElement.OpacityProperty, null);

        var fromHeight = contentContainer.ActualHeight > 0d ? contentContainer.ActualHeight : MeasureExpandedHeight();
        var duration = TimeSpan.FromMilliseconds(ExpandDurationMilliseconds);
        var easing = new CubicEase { EasingMode = EasingMode.EaseInOut };

        var heightAnimation = new DoubleAnimation(fromHeight, 0d, duration) { EasingFunction = easing };
        var opacityAnimation = new DoubleAnimation(1d, 0d, duration) { EasingFunction = easing };

        heightAnimation.Completed += (_, _) =>
        {
            if (contentContainer is null)
            {
                return;
            }

            contentContainer.BeginAnimation(FrameworkElement.HeightProperty, null);
            contentContainer.BeginAnimation(UIElement.OpacityProperty, null);

            if (ResolvedDisplayDirective == NCollapseDisplayDirective.Show)
            {
                contentContainer.Visibility = Visibility.Visible;
                contentContainer.Height = 0d;
                contentContainer.Opacity = 1d;
                PresentedContent = Content;
            }
            else
            {
                UpdatePresentedContent(expandImmediately: false);
                contentContainer.Visibility = Visibility.Collapsed;
                contentContainer.Height = 0d;
                contentContainer.Opacity = 0d;
            }
        };

        contentContainer.BeginAnimation(FrameworkElement.HeightProperty, heightAnimation);
        contentContainer.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
        AnimateArrow(0d);
    }

    private double MeasureExpandedHeight()
    {
        if (contentPresenter is null)
        {
            return 0d;
        }

        var availableWidth = contentContainer?.ActualWidth ?? ActualWidth;
        if (double.IsNaN(availableWidth) || availableWidth <= 0d)
        {
            availableWidth = 480d;
        }

        contentPresenter.Measure(new Size(availableWidth, double.PositiveInfinity));
        return contentPresenter.DesiredSize.Height;
    }

    private void SetArrowAngle(double angle)
    {
        if (leftArrowTransform is not null)
        {
            leftArrowTransform.Angle = angle;
        }

        if (rightArrowTransform is not null)
        {
            rightArrowTransform.Angle = angle;
        }
    }

    private void AnimateArrow(double angle)
    {
        var duration = TimeSpan.FromMilliseconds(ExpandDurationMilliseconds);
        var easing = new CubicEase { EasingMode = EasingMode.EaseInOut };

        if (leftArrowTransform is not null)
        {
            leftArrowTransform.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation(angle, duration) { EasingFunction = easing });
        }

        if (rightArrowTransform is not null)
        {
            rightArrowTransform.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation(angle, duration) { EasingFunction = easing });
        }
    }

    private void AttachInteractionHandlers()
    {
        if (leftArrowToggle is not null)
        {
            leftArrowToggle.PreviewMouseLeftButtonUp += HandleArrowToggleClick;
        }

        if (rightArrowToggle is not null)
        {
            rightArrowToggle.PreviewMouseLeftButtonUp += HandleArrowToggleClick;
        }

        if (mainToggle is not null)
        {
            mainToggle.PreviewMouseLeftButtonUp += HandleMainToggleClick;
        }

        if (extraToggle is not null)
        {
            extraToggle.PreviewMouseLeftButtonUp += HandleExtraToggleClick;
        }
    }

    private void UpdateInteractiveState()
    {
        UpdateToggleCursor(leftArrowToggle, NCollapseTriggerAreas.Arrow);
        UpdateToggleCursor(rightArrowToggle, NCollapseTriggerAreas.Arrow);
        UpdateToggleCursor(mainToggle, NCollapseTriggerAreas.Main);
        UpdateToggleCursor(extraToggle, NCollapseTriggerAreas.Extra);
    }

    private void UpdateToggleCursor(Border? toggle, NCollapseTriggerAreas triggerArea)
    {
        if (toggle is null)
        {
            return;
        }

        toggle.Cursor = CanTrigger(triggerArea) ? Cursors.Hand : Cursors.Arrow;
    }

    private bool CanTrigger(NCollapseTriggerAreas triggerArea)
    {
        return !Disabled && (ResolvedTriggerAreas & triggerArea) == triggerArea;
    }

    private void ToggleFromArea(MouseButtonEventArgs e, NCollapseTriggerAreas triggerArea)
    {
        if (!CanTrigger(triggerArea))
        {
            return;
        }

        e.Handled = true;
        parentCollapse?.ToggleItem(this, triggerArea);
    }

    private void DetachInteractionHandlers()
    {
        if (leftArrowToggle is not null)
        {
            leftArrowToggle.PreviewMouseLeftButtonUp -= HandleArrowToggleClick;
        }

        if (rightArrowToggle is not null)
        {
            rightArrowToggle.PreviewMouseLeftButtonUp -= HandleArrowToggleClick;
        }

        if (mainToggle is not null)
        {
            mainToggle.PreviewMouseLeftButtonUp -= HandleMainToggleClick;
        }

        if (extraToggle is not null)
        {
            extraToggle.PreviewMouseLeftButtonUp -= HandleExtraToggleClick;
        }
    }

    private void HandleMainToggleClick(object sender, MouseButtonEventArgs e)
    {
        ToggleFromArea(e, NCollapseTriggerAreas.Main);
    }

    private void HandleArrowToggleClick(object sender, MouseButtonEventArgs e)
    {
        ToggleFromArea(e, NCollapseTriggerAreas.Arrow);
    }

    private void HandleExtraToggleClick(object sender, MouseButtonEventArgs e)
    {
        ToggleFromArea(e, NCollapseTriggerAreas.Extra);
    }

    private static bool IsMeaningfulContent(object? value)
    {
        return value switch
        {
            null => false,
            string text => !string.IsNullOrWhiteSpace(text),
            _ => true
        };
    }
}
