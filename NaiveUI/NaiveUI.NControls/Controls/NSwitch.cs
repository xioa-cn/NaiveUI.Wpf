using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using NaiveUI.NControls.Themes;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

[TemplatePart(Name = ThumbTransformPartName, Type = typeof(TranslateTransform))]
[TemplatePart(Name = RailPartName, Type = typeof(FrameworkElement))]
public class NSwitch : ToggleButton
{
    private const string RailPartName = "PART_Rail";
    private const string ThumbTransformPartName = "PART_ThumbTransform";
    private static readonly Duration ThumbAnimationDuration = new(TimeSpan.FromMilliseconds(160));
    private static readonly IEasingFunction ThumbAnimationEasing = new CubicEase { EasingMode = EasingMode.EaseInOut };
    private static readonly CornerRadius RoundCornerRadius = new(999d);

    private FrameworkElement? railPart;
    private TranslateTransform? thumbTransformPart;
    private bool hasInitializedValue;
    private bool isSyncingDisabled;
    private bool isSyncingIsChecked;
    private bool isSyncingValue;

    static NSwitch()
    {
        ElementBase.DefaultStyle<NSwitch>(DefaultStyleKeyProperty);
        IsCheckedProperty.OverrideMetadata(
            typeof(NSwitch),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsCheckedChanged));
    }

    public NSwitch()
    {
        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;
        SizeChanged += HandleSizeChanged;
        UpdateResolvedState(false);
    }

    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(object),
            typeof(NSwitch),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public object? DefaultValue
    {
        get => GetValue(DefaultValueProperty);
        set => SetValue(DefaultValueProperty, value);
    }

    public static readonly DependencyProperty DefaultValueProperty =
        ElementBase.Property<NSwitch, object?>(nameof(DefaultValueProperty), null);

    public object? CheckedValue
    {
        get => GetValue(CheckedValueProperty);
        set => SetValue(CheckedValueProperty, value);
    }

    public static readonly DependencyProperty CheckedValueProperty =
        ElementBase.Property<NSwitch, object?>(nameof(CheckedValueProperty), ValueBoxes.TrueBox, OnStateValuePropertyChanged);

    public object? UncheckedValue
    {
        get => GetValue(UncheckedValueProperty);
        set => SetValue(UncheckedValueProperty, value);
    }

    public static readonly DependencyProperty UncheckedValueProperty =
        ElementBase.Property<NSwitch, object?>(nameof(UncheckedValueProperty), ValueBoxes.FalseBox, OnStateValuePropertyChanged);

    public bool Disabled
    {
        get => (bool)GetValue(DisabledProperty);
        set => SetValue(DisabledProperty, value);
    }

    public static readonly DependencyProperty DisabledProperty =
        ElementBase.Property<NSwitch, bool>(nameof(DisabledProperty), false, OnDisabledPropertyChanged);

    public bool Loading
    {
        get => (bool)GetValue(LoadingProperty);
        set => SetValue(LoadingProperty, value);
    }

    public static readonly DependencyProperty LoadingProperty =
        ElementBase.Property<NSwitch, bool>(nameof(LoadingProperty), false, OnAppearancePropertyChanged);

    public bool Round
    {
        get => (bool)GetValue(RoundProperty);
        set => SetValue(RoundProperty, value);
    }

    public static readonly DependencyProperty RoundProperty =
        ElementBase.Property<NSwitch, bool>(nameof(RoundProperty), true, OnLayoutPropertyChanged);

    public bool RubberBand
    {
        get => (bool)GetValue(RubberBandProperty);
        set => SetValue(RubberBandProperty, value);
    }

    public static readonly DependencyProperty RubberBandProperty =
        ElementBase.Property<NSwitch, bool>(nameof(RubberBandProperty), true);

    public NControlSize Size
    {
        get => (NControlSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public static readonly DependencyProperty SizeProperty =
        ElementBase.Property<NSwitch, NControlSize>(nameof(SizeProperty), NControlSize.Medium, OnLayoutPropertyChanged);

    public double TrackWidth
    {
        get => (double)GetValue(TrackWidthProperty);
        set => SetValue(TrackWidthProperty, value);
    }

    public static readonly DependencyProperty TrackWidthProperty =
        ElementBase.Property<NSwitch, double>(nameof(TrackWidthProperty), double.NaN, OnLayoutPropertyChanged);

    public double TrackHeight
    {
        get => (double)GetValue(TrackHeightProperty);
        set => SetValue(TrackHeightProperty, value);
    }

    public static readonly DependencyProperty TrackHeightProperty =
        ElementBase.Property<NSwitch, double>(nameof(TrackHeightProperty), double.NaN, OnLayoutPropertyChanged);

    public double ThumbSize
    {
        get => (double)GetValue(ThumbSizeProperty);
        set => SetValue(ThumbSizeProperty, value);
    }

    public static readonly DependencyProperty ThumbSizeProperty =
        ElementBase.Property<NSwitch, double>(nameof(ThumbSizeProperty), double.NaN, OnLayoutPropertyChanged);

    public double RailRadius
    {
        get => (double)GetValue(RailRadiusProperty);
        set => SetValue(RailRadiusProperty, value);
    }

    public static readonly DependencyProperty RailRadiusProperty =
        ElementBase.Property<NSwitch, double>(nameof(RailRadiusProperty), double.NaN, OnLayoutPropertyChanged);

    public double ThumbRadius
    {
        get => (double)GetValue(ThumbRadiusProperty);
        set => SetValue(ThumbRadiusProperty, value);
    }

    public static readonly DependencyProperty ThumbRadiusProperty =
        ElementBase.Property<NSwitch, double>(nameof(ThumbRadiusProperty), double.NaN, OnLayoutPropertyChanged);

    public double ContentFontSize
    {
        get => (double)GetValue(ContentFontSizeProperty);
        set => SetValue(ContentFontSizeProperty, value);
    }

    public static readonly DependencyProperty ContentFontSizeProperty =
        ElementBase.Property<NSwitch, double>(nameof(ContentFontSizeProperty), double.NaN, OnLayoutPropertyChanged);

    public FontFamily? ContentFontFamily
    {
        get => (FontFamily?)GetValue(ContentFontFamilyProperty);
        set => SetValue(ContentFontFamilyProperty, value);
    }

    public static readonly DependencyProperty ContentFontFamilyProperty =
        ElementBase.Property<NSwitch, FontFamily?>(nameof(ContentFontFamilyProperty), null, OnTypographyPropertyChanged);

    public FontWeight ContentFontWeight
    {
        get => (FontWeight)GetValue(ContentFontWeightProperty);
        set => SetValue(ContentFontWeightProperty, value);
    }

    public static readonly DependencyProperty ContentFontWeightProperty =
        ElementBase.Property<NSwitch, FontWeight>(nameof(ContentFontWeightProperty), FontWeights.SemiBold, OnTypographyPropertyChanged);

    public object? CheckedContent
    {
        get => GetValue(CheckedContentProperty);
        set => SetValue(CheckedContentProperty, value);
    }

    public static readonly DependencyProperty CheckedContentProperty =
        ElementBase.Property<NSwitch, object?>(nameof(CheckedContentProperty), null, OnSupplementalContentPropertyChanged);

    public object? UncheckedContent
    {
        get => GetValue(UncheckedContentProperty);
        set => SetValue(UncheckedContentProperty, value);
    }

    public static readonly DependencyProperty UncheckedContentProperty =
        ElementBase.Property<NSwitch, object?>(nameof(UncheckedContentProperty), null, OnSupplementalContentPropertyChanged);

    public object? IconContent
    {
        get => GetValue(IconContentProperty);
        set => SetValue(IconContentProperty, value);
    }

    public static readonly DependencyProperty IconContentProperty =
        ElementBase.Property<NSwitch, object?>(nameof(IconContentProperty), null, OnSupplementalContentPropertyChanged);

    public object? CheckedIconContent
    {
        get => GetValue(CheckedIconContentProperty);
        set => SetValue(CheckedIconContentProperty, value);
    }

    public static readonly DependencyProperty CheckedIconContentProperty =
        ElementBase.Property<NSwitch, object?>(nameof(CheckedIconContentProperty), null, OnSupplementalContentPropertyChanged);

    public object? UncheckedIconContent
    {
        get => GetValue(UncheckedIconContentProperty);
        set => SetValue(UncheckedIconContentProperty, value);
    }

    public static readonly DependencyProperty UncheckedIconContentProperty =
        ElementBase.Property<NSwitch, object?>(nameof(UncheckedIconContentProperty), null, OnSupplementalContentPropertyChanged);

    public Brush? CheckedRailBrush
    {
        get => (Brush?)GetValue(CheckedRailBrushProperty);
        set => SetValue(CheckedRailBrushProperty, value);
    }

    public static readonly DependencyProperty CheckedRailBrushProperty =
        ElementBase.Property<NSwitch, Brush?>(nameof(CheckedRailBrushProperty), null, OnAppearancePropertyChanged);

    public Brush? UncheckedRailBrush
    {
        get => (Brush?)GetValue(UncheckedRailBrushProperty);
        set => SetValue(UncheckedRailBrushProperty, value);
    }

    public static readonly DependencyProperty UncheckedRailBrushProperty =
        ElementBase.Property<NSwitch, Brush?>(nameof(UncheckedRailBrushProperty), null, OnAppearancePropertyChanged);

    public Brush? CheckedRailHoverBrush
    {
        get => (Brush?)GetValue(CheckedRailHoverBrushProperty);
        set => SetValue(CheckedRailHoverBrushProperty, value);
    }

    public static readonly DependencyProperty CheckedRailHoverBrushProperty =
        ElementBase.Property<NSwitch, Brush?>(nameof(CheckedRailHoverBrushProperty), null, OnAppearancePropertyChanged);

    public Brush? UncheckedRailHoverBrush
    {
        get => (Brush?)GetValue(UncheckedRailHoverBrushProperty);
        set => SetValue(UncheckedRailHoverBrushProperty, value);
    }

    public static readonly DependencyProperty UncheckedRailHoverBrushProperty =
        ElementBase.Property<NSwitch, Brush?>(nameof(UncheckedRailHoverBrushProperty), null, OnAppearancePropertyChanged);

    public Brush? CheckedRailPressedBrush
    {
        get => (Brush?)GetValue(CheckedRailPressedBrushProperty);
        set => SetValue(CheckedRailPressedBrushProperty, value);
    }

    public static readonly DependencyProperty CheckedRailPressedBrushProperty =
        ElementBase.Property<NSwitch, Brush?>(nameof(CheckedRailPressedBrushProperty), null, OnAppearancePropertyChanged);

    public Brush? UncheckedRailPressedBrush
    {
        get => (Brush?)GetValue(UncheckedRailPressedBrushProperty);
        set => SetValue(UncheckedRailPressedBrushProperty, value);
    }

    public static readonly DependencyProperty UncheckedRailPressedBrushProperty =
        ElementBase.Property<NSwitch, Brush?>(nameof(UncheckedRailPressedBrushProperty), null, OnAppearancePropertyChanged);

    public Brush? CheckedButtonBrush
    {
        get => (Brush?)GetValue(CheckedButtonBrushProperty);
        set => SetValue(CheckedButtonBrushProperty, value);
    }

    public static readonly DependencyProperty CheckedButtonBrushProperty =
        ElementBase.Property<NSwitch, Brush?>(nameof(CheckedButtonBrushProperty), null, OnAppearancePropertyChanged);

    public Brush? ThumbBrush
    {
        get => (Brush?)GetValue(ThumbBrushProperty);
        set => SetValue(ThumbBrushProperty, value);
    }

    public static readonly DependencyProperty ThumbBrushProperty =
        ElementBase.Property<NSwitch, Brush?>(nameof(ThumbBrushProperty), null, OnAppearancePropertyChanged);

    public Brush? CheckedThumbBrush
    {
        get => (Brush?)GetValue(CheckedThumbBrushProperty);
        set => SetValue(CheckedThumbBrushProperty, value);
    }

    public static readonly DependencyProperty CheckedThumbBrushProperty =
        ElementBase.Property<NSwitch, Brush?>(nameof(CheckedThumbBrushProperty), null, OnAppearancePropertyChanged);

    public Brush? UncheckedButtonBrush
    {
        get => (Brush?)GetValue(UncheckedButtonBrushProperty);
        set => SetValue(UncheckedButtonBrushProperty, value);
    }

    public static readonly DependencyProperty UncheckedButtonBrushProperty =
        ElementBase.Property<NSwitch, Brush?>(nameof(UncheckedButtonBrushProperty), null, OnAppearancePropertyChanged);

    public Brush? UncheckedThumbBrush
    {
        get => (Brush?)GetValue(UncheckedThumbBrushProperty);
        set => SetValue(UncheckedThumbBrushProperty, value);
    }

    public static readonly DependencyProperty UncheckedThumbBrushProperty =
        ElementBase.Property<NSwitch, Brush?>(nameof(UncheckedThumbBrushProperty), null, OnAppearancePropertyChanged);

    public Brush? CheckedContentBrush
    {
        get => (Brush?)GetValue(CheckedContentBrushProperty);
        set => SetValue(CheckedContentBrushProperty, value);
    }

    public static readonly DependencyProperty CheckedContentBrushProperty =
        ElementBase.Property<NSwitch, Brush?>(nameof(CheckedContentBrushProperty), null, OnAppearancePropertyChanged);

    public Brush? UncheckedContentBrush
    {
        get => (Brush?)GetValue(UncheckedContentBrushProperty);
        set => SetValue(UncheckedContentBrushProperty, value);
    }

    public static readonly DependencyProperty UncheckedContentBrushProperty =
        ElementBase.Property<NSwitch, Brush?>(nameof(UncheckedContentBrushProperty), null, OnAppearancePropertyChanged);

    public Brush? CheckedIconBrush
    {
        get => (Brush?)GetValue(CheckedIconBrushProperty);
        set => SetValue(CheckedIconBrushProperty, value);
    }

    public static readonly DependencyProperty CheckedIconBrushProperty =
        ElementBase.Property<NSwitch, Brush?>(nameof(CheckedIconBrushProperty), null, OnAppearancePropertyChanged);

    public Brush? UncheckedIconBrush
    {
        get => (Brush?)GetValue(UncheckedIconBrushProperty);
        set => SetValue(UncheckedIconBrushProperty, value);
    }

    public static readonly DependencyProperty UncheckedIconBrushProperty =
        ElementBase.Property<NSwitch, Brush?>(nameof(UncheckedIconBrushProperty), null, OnAppearancePropertyChanged);

    public Brush? FocusRingBrush
    {
        get => (Brush?)GetValue(FocusRingBrushProperty);
        set => SetValue(FocusRingBrushProperty, value);
    }

    public static readonly DependencyProperty FocusRingBrushProperty =
        ElementBase.Property<NSwitch, Brush?>(nameof(FocusRingBrushProperty), null, OnAppearancePropertyChanged);

    public Brush? LoadingBrush
    {
        get => (Brush?)GetValue(LoadingBrushProperty);
        set => SetValue(LoadingBrushProperty, value);
    }

    public static readonly DependencyProperty LoadingBrushProperty =
        ElementBase.Property<NSwitch, Brush?>(nameof(LoadingBrushProperty), null, OnAppearancePropertyChanged);

    public double LoadingStrokeThickness
    {
        get => (double)GetValue(LoadingStrokeThicknessProperty);
        set => SetValue(LoadingStrokeThicknessProperty, value);
    }

    public static readonly DependencyProperty LoadingStrokeThicknessProperty =
        ElementBase.Property<NSwitch, double>(nameof(LoadingStrokeThicknessProperty), 2d);

    public double LoadingScale
    {
        get => (double)GetValue(LoadingScaleProperty);
        set => SetValue(LoadingScaleProperty, value);
    }

    public static readonly DependencyProperty LoadingScaleProperty =
        ElementBase.Property<NSwitch, double>(nameof(LoadingScaleProperty), 1d, OnLayoutPropertyChanged);

    public object? DisplayThumbContent
    {
        get => GetValue(DisplayThumbContentProperty);
        private set => SetValue(DisplayThumbContentProperty, value);
    }

    public static readonly DependencyProperty DisplayThumbContentProperty =
        ElementBase.Property<NSwitch, object?>(nameof(DisplayThumbContentProperty), null);

    public bool HasCheckedContent
    {
        get => (bool)GetValue(HasCheckedContentProperty);
        private set => SetValue(HasCheckedContentProperty, value);
    }

    public static readonly DependencyProperty HasCheckedContentProperty =
        ElementBase.Property<NSwitch, bool>(nameof(HasCheckedContentProperty), false);

    public bool HasUncheckedContent
    {
        get => (bool)GetValue(HasUncheckedContentProperty);
        private set => SetValue(HasUncheckedContentProperty, value);
    }

    public static readonly DependencyProperty HasUncheckedContentProperty =
        ElementBase.Property<NSwitch, bool>(nameof(HasUncheckedContentProperty), false);

    public bool HasThumbContent
    {
        get => (bool)GetValue(HasThumbContentProperty);
        private set => SetValue(HasThumbContentProperty, value);
    }

    public static readonly DependencyProperty HasThumbContentProperty =
        ElementBase.Property<NSwitch, bool>(nameof(HasThumbContentProperty), false);

    public double ResolvedTrackHeight
    {
        get => (double)GetValue(ResolvedTrackHeightProperty);
        private set => SetValue(ResolvedTrackHeightProperty, value);
    }

    public static readonly DependencyProperty ResolvedTrackHeightProperty =
        ElementBase.Property<NSwitch, double>(nameof(ResolvedTrackHeightProperty), 22d);

    public double ResolvedTrackWidth
    {
        get => (double)GetValue(ResolvedTrackWidthProperty);
        private set => SetValue(ResolvedTrackWidthProperty, value);
    }

    public static readonly DependencyProperty ResolvedTrackWidthProperty =
        ElementBase.Property<NSwitch, double>(nameof(ResolvedTrackWidthProperty), double.NaN);

    public double ResolvedTrackMinWidth
    {
        get => (double)GetValue(ResolvedTrackMinWidthProperty);
        private set => SetValue(ResolvedTrackMinWidthProperty, value);
    }

    public static readonly DependencyProperty ResolvedTrackMinWidthProperty =
        ElementBase.Property<NSwitch, double>(nameof(ResolvedTrackMinWidthProperty), 40d);

    public double ResolvedThumbSize
    {
        get => (double)GetValue(ResolvedThumbSizeProperty);
        private set => SetValue(ResolvedThumbSizeProperty, value);
    }

    public static readonly DependencyProperty ResolvedThumbSizeProperty =
        ElementBase.Property<NSwitch, double>(nameof(ResolvedThumbSizeProperty), 18d);

    public double ResolvedLoadingIndicatorSize
    {
        get => (double)GetValue(ResolvedLoadingIndicatorSizeProperty);
        private set => SetValue(ResolvedLoadingIndicatorSizeProperty, value);
    }

    public static readonly DependencyProperty ResolvedLoadingIndicatorSizeProperty =
        ElementBase.Property<NSwitch, double>(nameof(ResolvedLoadingIndicatorSizeProperty), 10d);

    public CornerRadius ResolvedRailCornerRadius
    {
        get => (CornerRadius)GetValue(ResolvedRailCornerRadiusProperty);
        private set => SetValue(ResolvedRailCornerRadiusProperty, value);
    }

    public static readonly DependencyProperty ResolvedRailCornerRadiusProperty =
        ElementBase.Property<NSwitch, CornerRadius>(nameof(ResolvedRailCornerRadiusProperty), RoundCornerRadius);

    public CornerRadius ResolvedThumbCornerRadius
    {
        get => (CornerRadius)GetValue(ResolvedThumbCornerRadiusProperty);
        private set => SetValue(ResolvedThumbCornerRadiusProperty, value);
    }

    public static readonly DependencyProperty ResolvedThumbCornerRadiusProperty =
        ElementBase.Property<NSwitch, CornerRadius>(nameof(ResolvedThumbCornerRadiusProperty), RoundCornerRadius);

    public Thickness ResolvedThumbMargin
    {
        get => (Thickness)GetValue(ResolvedThumbMarginProperty);
        private set => SetValue(ResolvedThumbMarginProperty, value);
    }

    public static readonly DependencyProperty ResolvedThumbMarginProperty =
        ElementBase.Property<NSwitch, Thickness>(nameof(ResolvedThumbMarginProperty), new Thickness(2d));

    public Thickness ResolvedCheckedContentMargin
    {
        get => (Thickness)GetValue(ResolvedCheckedContentMarginProperty);
        private set => SetValue(ResolvedCheckedContentMarginProperty, value);
    }

    public static readonly DependencyProperty ResolvedCheckedContentMarginProperty =
        ElementBase.Property<NSwitch, Thickness>(nameof(ResolvedCheckedContentMarginProperty), new Thickness(8d, 0d, 24d, 0d));

    public Thickness ResolvedUncheckedContentMargin
    {
        get => (Thickness)GetValue(ResolvedUncheckedContentMarginProperty);
        private set => SetValue(ResolvedUncheckedContentMarginProperty, value);
    }

    public static readonly DependencyProperty ResolvedUncheckedContentMarginProperty =
        ElementBase.Property<NSwitch, Thickness>(nameof(ResolvedUncheckedContentMarginProperty), new Thickness(24d, 0d, 8d, 0d));

    public double ResolvedContentFontSize
    {
        get => (double)GetValue(ResolvedContentFontSizeProperty);
        private set => SetValue(ResolvedContentFontSizeProperty, value);
    }

    public static readonly DependencyProperty ResolvedContentFontSizeProperty =
        ElementBase.Property<NSwitch, double>(nameof(ResolvedContentFontSizeProperty), 12d);

    public FontFamily ResolvedContentFontFamily
    {
        get => (FontFamily)GetValue(ResolvedContentFontFamilyProperty);
        private set => SetValue(ResolvedContentFontFamilyProperty, value);
    }

    public static readonly DependencyProperty ResolvedContentFontFamilyProperty =
        ElementBase.Property<NSwitch, FontFamily>(nameof(ResolvedContentFontFamilyProperty), SystemFonts.MessageFontFamily);

    public FontWeight ResolvedContentFontWeight
    {
        get => (FontWeight)GetValue(ResolvedContentFontWeightProperty);
        private set => SetValue(ResolvedContentFontWeightProperty, value);
    }

    public static readonly DependencyProperty ResolvedContentFontWeightProperty =
        ElementBase.Property<NSwitch, FontWeight>(nameof(ResolvedContentFontWeightProperty), FontWeights.SemiBold);

    public Brush ResolvedRailBrush
    {
        get => (Brush)GetValue(ResolvedRailBrushProperty);
        private set => SetValue(ResolvedRailBrushProperty, value);
    }

    public static readonly DependencyProperty ResolvedRailBrushProperty =
        ElementBase.Property<NSwitch, Brush>(nameof(ResolvedRailBrushProperty), Brushes.Gray);

    public Brush ResolvedButtonBrush
    {
        get => (Brush)GetValue(ResolvedButtonBrushProperty);
        private set => SetValue(ResolvedButtonBrushProperty, value);
    }

    public static readonly DependencyProperty ResolvedButtonBrushProperty =
        ElementBase.Property<NSwitch, Brush>(nameof(ResolvedButtonBrushProperty), Brushes.White);

    public Brush ResolvedContentBrush
    {
        get => (Brush)GetValue(ResolvedContentBrushProperty);
        private set => SetValue(ResolvedContentBrushProperty, value);
    }

    public static readonly DependencyProperty ResolvedContentBrushProperty =
        ElementBase.Property<NSwitch, Brush>(nameof(ResolvedContentBrushProperty), Brushes.White);

    public Brush ResolvedThumbContentBrush
    {
        get => (Brush)GetValue(ResolvedThumbContentBrushProperty);
        private set => SetValue(ResolvedThumbContentBrushProperty, value);
    }

    public static readonly DependencyProperty ResolvedThumbContentBrushProperty =
        ElementBase.Property<NSwitch, Brush>(nameof(ResolvedThumbContentBrushProperty), Brushes.Green);

    public Brush ResolvedLoadingBrush
    {
        get => (Brush)GetValue(ResolvedLoadingBrushProperty);
        private set => SetValue(ResolvedLoadingBrushProperty, value);
    }

    public static readonly DependencyProperty ResolvedLoadingBrushProperty =
        ElementBase.Property<NSwitch, Brush>(nameof(ResolvedLoadingBrushProperty), Brushes.Green);

    public Brush ResolvedFocusRingBrush
    {
        get => (Brush)GetValue(ResolvedFocusRingBrushProperty);
        private set => SetValue(ResolvedFocusRingBrushProperty, value);
    }

    public static readonly DependencyProperty ResolvedFocusRingBrushProperty =
        ElementBase.Property<NSwitch, Brush>(nameof(ResolvedFocusRingBrushProperty), Brushes.Transparent);

    public static readonly RoutedEvent ValueChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(ValueChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<object>),
            typeof(NSwitch));

    public event RoutedPropertyChangedEventHandler<object> ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (railPart is not null)
        {
            railPart.SizeChanged -= HandleRailSizeChanged;
        }

        railPart = GetTemplateChild(RailPartName) as FrameworkElement;
        thumbTransformPart = GetTemplateChild(ThumbTransformPartName) as TranslateTransform;

        if (railPart is not null)
        {
            railPart.SizeChanged += HandleRailSizeChanged;
        }

        ScheduleThumbPositionRefresh();
    }

    protected override IEnumerator LogicalChildren
    {
        get
        {
            var children = new List<object>();
            var baseChildren = base.LogicalChildren;
            while (baseChildren.MoveNext())
            {
                if (baseChildren.Current is not null)
                {
                    children.Add(baseChildren.Current);
                }
            }

            AppendLogicalChild(children, CheckedContent);
            AppendLogicalChild(children, UncheckedContent);
            AppendLogicalChild(children, IconContent);
            AppendLogicalChild(children, CheckedIconContent);
            AppendLogicalChild(children, UncheckedIconContent);

            return children.GetEnumerator();
        }
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == IsEnabledProperty)
        {
            SyncDisabledStateFromIsEnabled();
            UpdateResolvedAppearance();
            return;
        }

        if (e.Property == IsMouseOverProperty || e.Property == IsPressedProperty)
        {
            UpdateResolvedAppearance();
            return;
        }

        if (e.Property == FontSizeProperty
            || e.Property == FontFamilyProperty
            || e.Property == FontWeightProperty
            || e.Property == WidthProperty
            || e.Property == HeightProperty
            || e.Property == MinWidthProperty)
        {
            UpdateResolvedState(false);
        }
    }

    protected override void OnToggle()
    {
        if (Loading || !IsEnabled)
        {
            return;
        }

        base.OnToggle();
    }

    private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NSwitch nSwitch)
        {
            nSwitch.UpdateResolvedState(false);
        }
    }

    private static void OnAppearancePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NSwitch nSwitch)
        {
            nSwitch.UpdateResolvedAppearance();
        }
    }

    private static void OnTypographyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NSwitch nSwitch)
        {
            nSwitch.UpdateResolvedState(false);
        }
    }

    private static void OnSupplementalContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NSwitch nSwitch)
        {
            return;
        }

        nSwitch.UpdateLogicalChildRegistration(e.OldValue, e.NewValue);
        nSwitch.UpdateResolvedContentState();
        nSwitch.ScheduleThumbPositionRefresh();
    }

    private static void OnDisabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NSwitch nSwitch)
        {
            return;
        }

        nSwitch.SyncIsEnabledState();
        nSwitch.UpdateResolvedAppearance();
    }

    private static void OnStateValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NSwitch nSwitch)
        {
            return;
        }

        if (!nSwitch.hasInitializedValue)
        {
            nSwitch.UpdateResolvedState(false);
            return;
        }

        if (e.Property == CheckedValueProperty && nSwitch.IsChecked == true)
        {
            nSwitch.SetCurrentValue(ValueProperty, nSwitch.CheckedValue);
            return;
        }

        if (e.Property == UncheckedValueProperty && nSwitch.IsChecked != true)
        {
            nSwitch.SetCurrentValue(ValueProperty, nSwitch.UncheckedValue);
            return;
        }

        nSwitch.UpdateResolvedState(false);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NSwitch nSwitch)
        {
            return;
        }

        if (!nSwitch.isSyncingIsChecked)
        {
            nSwitch.isSyncingValue = true;
            nSwitch.SetCurrentValue(IsCheckedProperty, ValueEquals(e.NewValue, nSwitch.CheckedValue));
            nSwitch.isSyncingValue = false;
            nSwitch.UpdateResolvedState(nSwitch.IsLoaded);
        }

        nSwitch.RaiseEvent(new RoutedPropertyChangedEventArgs<object>(e.OldValue, e.NewValue, ValueChangedEvent));
    }

    private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NSwitch nSwitch)
        {
            return;
        }

        if (!nSwitch.isSyncingValue)
        {
            nSwitch.isSyncingIsChecked = true;
            nSwitch.SetCurrentValue(ValueProperty, (bool)e.NewValue ? nSwitch.CheckedValue : nSwitch.UncheckedValue);
            nSwitch.isSyncingIsChecked = false;
        }

        nSwitch.UpdateResolvedState(nSwitch.IsLoaded);
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        ThemeManager.ThemeChanged += HandleThemeChanged;

        if (railPart is not null)
        {
            railPart.SizeChanged -= HandleRailSizeChanged;
            railPart.SizeChanged += HandleRailSizeChanged;
        }

        SyncIsEnabledState();
        InitializeValueIfNeeded();
        UpdateResolvedState(false);
        ScheduleThumbPositionRefresh();
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        ThemeManager.ThemeChanged -= HandleThemeChanged;

        if (railPart is not null)
        {
            railPart.SizeChanged -= HandleRailSizeChanged;
        }
    }

    private void HandleSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateThumbPosition(false);
    }

    private void HandleRailSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateThumbPosition(false);
    }

    private void HandleThemeChanged(ThemeMode mode)
    {
        UpdateResolvedState(false);
    }

    private void InitializeValueIfNeeded()
    {
        if (hasInitializedValue)
        {
            return;
        }

        hasInitializedValue = true;

        if (BindingOperations.GetBindingExpressionBase(this, ValueProperty) is not null
            || ReadLocalValue(ValueProperty) != DependencyProperty.UnsetValue)
        {
            isSyncingValue = true;
            SetCurrentValue(IsCheckedProperty, ValueEquals(Value, CheckedValue));
            isSyncingValue = false;
            return;
        }

        var initialValue = DefaultValue ?? (IsChecked == true ? CheckedValue : UncheckedValue);
        SetCurrentValue(ValueProperty, initialValue);
    }

    private void SyncIsEnabledState()
    {
        if (isSyncingDisabled)
        {
            return;
        }

        isSyncingDisabled = true;
        SetCurrentValue(IsEnabledProperty, !Disabled);
        isSyncingDisabled = false;
    }

    private void SyncDisabledStateFromIsEnabled()
    {
        if (isSyncingDisabled)
        {
            return;
        }

        isSyncingDisabled = true;
        SetCurrentValue(DisabledProperty, !IsEnabled);
        isSyncingDisabled = false;
    }

    private void UpdateResolvedState(bool animateThumb)
    {
        UpdateResolvedContentState();
        UpdateResolvedMetrics();
        UpdateResolvedAppearance();
        UpdateThumbPosition(animateThumb);
    }

    private void UpdateResolvedContentState()
    {
        HasCheckedContent = HasMeaningfulContent(CheckedContent);
        HasUncheckedContent = HasMeaningfulContent(UncheckedContent);

        DisplayThumbContent = IsChecked == true
            ? CheckedIconContent ?? IconContent
            : UncheckedIconContent ?? IconContent;

        HasThumbContent = HasMeaningfulContent(DisplayThumbContent);
    }

    private void UpdateResolvedMetrics()
    {
        var resolvedContentFontSize = IsPositiveFinite(ContentFontSize) ? ContentFontSize : FontSize;
        var resolvedContentFontFamily = ContentFontFamily ?? FontFamily;
        var resolvedContentFontWeight = ContentFontWeight;

        var baseTrackHeight = Size switch
        {
            NControlSize.Tiny => 18d,
            NControlSize.Small => 20d,
            NControlSize.Large => 26d,
            _ => 22d
        };

        var fontDrivenTrackHeight = Math.Ceiling(resolvedContentFontSize + 8d);
        var explicitTrackHeight = IsPositiveFinite(TrackHeight) ? TrackHeight : Height;
        var trackHeight = IsPositiveFinite(explicitTrackHeight)
            ? explicitTrackHeight
            : Math.Max(baseTrackHeight, fontDrivenTrackHeight);

        var explicitThumbSize = ThumbSize;
        var thumbSize = IsPositiveFinite(explicitThumbSize)
            ? explicitThumbSize
            : Math.Max(12d, trackHeight - 4d);

        thumbSize = Math.Max(10d, Math.Min(thumbSize, Math.Max(10d, trackHeight - 2d)));

        var explicitTrackWidth = IsPositiveFinite(TrackWidth) ? TrackWidth : Width;
        var minWidth = IsPositiveFinite(explicitTrackWidth)
            ? explicitTrackWidth
            : thumbSize * 2d + 4d;

        if (!IsPositiveFinite(explicitTrackWidth))
        {
            minWidth = Math.Max(minWidth, IsPositiveFinite(MinWidth) ? MinWidth : 0d);
        }

        var contentPadding = Math.Max(6d, Math.Round(trackHeight * 0.35d, 0));
        var oppositeReserve = thumbSize + contentPadding;
        var loadingIndicatorSize = Math.Max(8d, Math.Round(thumbSize * 0.55d * Math.Max(0.5d, LoadingScale), 0));

        ResolvedTrackWidth = IsPositiveFinite(explicitTrackWidth) ? explicitTrackWidth : double.NaN;
        ResolvedTrackHeight = trackHeight;
        ResolvedThumbSize = thumbSize;
        ResolvedTrackMinWidth = minWidth;
        ResolvedLoadingIndicatorSize = loadingIndicatorSize;
        ResolvedThumbMargin = new Thickness(2d);
        ResolvedCheckedContentMargin = new Thickness(contentPadding, 0d, oppositeReserve, 0d);
        ResolvedUncheckedContentMargin = new Thickness(oppositeReserve, 0d, contentPadding, 0d);
        ResolvedContentFontSize = resolvedContentFontSize;
        ResolvedContentFontFamily = resolvedContentFontFamily;
        ResolvedContentFontWeight = resolvedContentFontWeight;

        if (IsNonNegativeFinite(RailRadius))
        {
            ResolvedRailCornerRadius = new CornerRadius(RailRadius);
        }
        else if (Round)
        {
            ResolvedRailCornerRadius = new CornerRadius(trackHeight / 2d);
        }
        else
        {
            var railRadius = Size switch
            {
                NControlSize.Tiny => 2d,
                NControlSize.Small => 3d,
                NControlSize.Large => 4d,
                _ => 3d
            };

            ResolvedRailCornerRadius = new CornerRadius(railRadius);
        }

        if (IsNonNegativeFinite(ThumbRadius))
        {
            ResolvedThumbCornerRadius = new CornerRadius(ThumbRadius);
            return;
        }

        ResolvedThumbCornerRadius = Round
            ? new CornerRadius(thumbSize / 2d)
            : new CornerRadius(Math.Max(2d, ResolvedRailCornerRadius.TopLeft - 1d));
    }

    private void UpdateResolvedAppearance()
    {
        var checkedBaseBrush = CheckedRailBrush ?? GetResourceBrush("Primary.First.Brush", Brushes.SeaGreen);
        var uncheckedBaseBrush = UncheckedRailBrush ?? GetResourceBrush("Theme.Border.Strong.Brush", Brushes.Gray);

        ResolvedRailBrush = IsChecked == true
            ? ResolveCheckedRailBrush(checkedBaseBrush)
            : ResolveUncheckedRailBrush(uncheckedBaseBrush);

        ResolvedButtonBrush = IsChecked == true
            ? CheckedThumbBrush ?? CheckedButtonBrush ?? ThumbBrush ?? GetResourceBrush("Theme.Surface.0.Brush", Brushes.White)
            : UncheckedThumbBrush ?? UncheckedButtonBrush ?? ThumbBrush ?? GetResourceBrush("Theme.Surface.0.Brush", Brushes.White);

        ResolvedContentBrush = IsChecked == true
            ? CheckedContentBrush ?? GetResourceBrush("Theme.Text.Inverse.Brush", Brushes.White)
            : UncheckedContentBrush ?? GetResourceBrush("Theme.Text.Secondary.Brush", Brushes.DimGray);

        ResolvedThumbContentBrush = IsChecked == true
            ? CheckedIconBrush ?? checkedBaseBrush
            : UncheckedIconBrush ?? GetResourceBrush("Theme.Text.Secondary.Brush", Brushes.DimGray);

        ResolvedLoadingBrush = LoadingBrush ?? ResolvedThumbContentBrush;
        ResolvedFocusRingBrush = FocusRingBrush ?? CreateOpacityBrush(IsChecked == true ? checkedBaseBrush : GetResourceBrush("Info.First.Brush", Brushes.DeepSkyBlue), 0.25d);
    }

    private Brush ResolveCheckedRailBrush(Brush checkedBaseBrush)
    {
        if (IsPressed)
        {
            return CheckedRailPressedBrush ?? GetResourceBrush("Primary.Pressed.Brush", checkedBaseBrush);
        }

        if (IsMouseOver)
        {
            return CheckedRailHoverBrush ?? GetResourceBrush("Primary.Hover.Brush", checkedBaseBrush);
        }

        return checkedBaseBrush;
    }

    private Brush ResolveUncheckedRailBrush(Brush uncheckedBaseBrush)
    {
        if (IsPressed)
        {
            return UncheckedRailPressedBrush ?? GetResourceBrush("Theme.Fill.2Pressed.Brush", uncheckedBaseBrush);
        }

        if (IsMouseOver)
        {
            return UncheckedRailHoverBrush ?? GetResourceBrush("Theme.Fill.2Hover.Brush", uncheckedBaseBrush);
        }

        return uncheckedBaseBrush;
    }

    private void UpdateThumbPosition(bool animate)
    {
        if (thumbTransformPart is null)
        {
            return;
        }

        var target = GetThumbCheckedOffset();
        if (IsChecked != true)
        {
            target = 0d;
        }

        if (!animate || !IsLoaded)
        {
            thumbTransformPart.BeginAnimation(TranslateTransform.XProperty, null);
            thumbTransformPart.X = target;
            return;
        }

        thumbTransformPart.BeginAnimation(
            TranslateTransform.XProperty,
            new DoubleAnimation(target, ThumbAnimationDuration)
            {
                EasingFunction = ThumbAnimationEasing
            });
    }

    private double GetThumbCheckedOffset()
    {
        var width = railPart?.ActualWidth ?? double.NaN;
        if (!IsPositiveFinite(width))
        {
            width = ResolvedTrackWidth;
        }

        if (!IsPositiveFinite(width))
        {
            width = ActualWidth;
        }

        if (!IsPositiveFinite(width))
        {
            width = Math.Max(ResolvedTrackMinWidth, MinWidth);
        }

        return Math.Max(0d, width - ResolvedThumbSize - ResolvedThumbMargin.Left - ResolvedThumbMargin.Right);
    }

    private void ScheduleThumbPositionRefresh()
    {
        if (!IsLoaded)
        {
            return;
        }

        Dispatcher.BeginInvoke(UpdateDeferredThumbPosition, DispatcherPriority.Loaded);
    }

    private void UpdateDeferredThumbPosition()
    {
        UpdateThumbPosition(false);
    }

    private Brush GetResourceBrush(string key, Brush fallback)
    {
        return TryFindResource(key) as Brush ?? fallback;
    }

    private static Brush CreateOpacityBrush(Brush brush, double opacity)
    {
        var clone = brush.CloneCurrentValue();
        clone.Opacity = opacity;
        if (clone.CanFreeze)
        {
            clone.Freeze();
        }

        return clone;
    }

    private static bool ValueEquals(object? left, object? right)
    {
        return Equals(left, right);
    }

    private static bool IsPositiveFinite(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value) && value > 0d;
    }

    private static bool IsNonNegativeFinite(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0d;
    }

    private static bool HasMeaningfulContent(object? content)
    {
        return content switch
        {
            null => false,
            string text => !string.IsNullOrWhiteSpace(text),
            _ => true
        };
    }

    private static void AppendLogicalChild(ICollection<object> children, object? content)
    {
        if (content is null || children.Contains(content))
        {
            return;
        }

        switch (content)
        {
            case FrameworkElement element when element.Parent is not null:
            case FrameworkContentElement contentElement when contentElement.Parent is not null:
                children.Add(content);
                break;
        }
    }

    private void UpdateLogicalChildRegistration(object? oldContent, object? newContent)
    {
        TryRemoveLogicalChild(oldContent);
        TryAddLogicalChild(newContent);
    }

    private void TryAddLogicalChild(object? content)
    {
        if (!CanManageLogicalChild(content))
        {
            return;
        }

        switch (content)
        {
            case FrameworkElement element when element.Parent is null:
                AddLogicalChild(element);
                break;
            case FrameworkContentElement contentElement when contentElement.Parent is null:
                AddLogicalChild(contentElement);
                break;
        }
    }

    private void TryRemoveLogicalChild(object? content)
    {
        if (!CanManageLogicalChild(content) || IsStillReferenced(content))
        {
            return;
        }

        switch (content)
        {
            case FrameworkElement element when ReferenceEquals(element.Parent, this):
                RemoveLogicalChild(element);
                break;
            case FrameworkContentElement contentElement when ReferenceEquals(contentElement.Parent, this):
                RemoveLogicalChild(contentElement);
                break;
        }
    }

    private bool IsStillReferenced(object? content)
    {
        return ReferenceEquals(content, CheckedContent)
               || ReferenceEquals(content, UncheckedContent)
               || ReferenceEquals(content, IconContent)
               || ReferenceEquals(content, CheckedIconContent)
               || ReferenceEquals(content, UncheckedIconContent)
               || ReferenceEquals(content, Content);
    }

    private static bool CanManageLogicalChild(object? content)
    {
        return content is FrameworkElement or FrameworkContentElement;
    }
}
