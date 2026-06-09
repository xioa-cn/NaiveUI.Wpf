using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public enum NSliderTooltipPlacement
{
    Top,
    Bottom,
    Left,
    Right
}

public sealed class NSliderMark : DependencyObject
{
    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty ValueProperty =
        ElementBase.Property<NSliderMark, double>(nameof(ValueProperty), 0d);

    public object? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static readonly DependencyProperty LabelProperty =
        ElementBase.Property<NSliderMark, object?>(nameof(LabelProperty), null);
}

public sealed class NSliderMarkCollection : ObservableCollection<NSliderMark>
{
}

[TemplatePart(Name = RailCanvasPartName, Type = typeof(Canvas))]
[TemplatePart(Name = RailPartName, Type = typeof(Border))]
[TemplatePart(Name = FillPartName, Type = typeof(Border))]
[TemplatePart(Name = StartThumbPartName, Type = typeof(Thumb))]
[TemplatePart(Name = EndThumbPartName, Type = typeof(Thumb))]
[TemplatePart(Name = StartIndicatorPartName, Type = typeof(Border))]
[TemplatePart(Name = EndIndicatorPartName, Type = typeof(Border))]
[TemplatePart(Name = StartIndicatorContentPartName, Type = typeof(ContentPresenter))]
[TemplatePart(Name = EndIndicatorContentPartName, Type = typeof(ContentPresenter))]
public class NSlider : Control
{
    private const string RailCanvasPartName = "PART_RailCanvas";
    private const string RailPartName = "PART_Rail";
    private const string FillPartName = "PART_Fill";
    private const string StartThumbPartName = "PART_StartThumb";
    private const string EndThumbPartName = "PART_EndThumb";
    private const string StartIndicatorPartName = "PART_StartIndicator";
    private const string EndIndicatorPartName = "PART_EndIndicator";
    private const string StartIndicatorContentPartName = "PART_StartIndicatorContent";
    private const string EndIndicatorContentPartName = "PART_EndIndicatorContent";

    private readonly List<FrameworkElement> markElements = [];
    private Canvas? railCanvasPart;
    private Border? railPart;
    private Border? fillPart;
    private Thumb? startThumbPart;
    private Thumb? endThumbPart;
    private Border? startIndicatorPart;
    private Border? endIndicatorPart;
    private ContentPresenter? startIndicatorContentPart;
    private ContentPresenter? endIndicatorContentPart;
    private int activeThumbIndex;
    private bool isDragging;
    private bool isHoveringStart;
    private bool isHoveringEnd;
    private bool isSyncingDisabled;
    private bool isSyncingValues;

    static NSlider()
    {
        ElementBase.DefaultStyle<NSlider>(DefaultStyleKeyProperty);
        FocusableProperty.OverrideMetadata(typeof(NSlider), new FrameworkPropertyMetadata(true));
    }

    public NSlider()
    {
        Marks = [];
        Marks.CollectionChanged += HandleMarksCollectionChanged;
        Loaded += HandleLoaded;
        SizeChanged += HandleSizeChanged;
        IsEnabledChanged += HandleIsEnabledChanged;
    }

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(NSlider),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValuePropertyChanged, CoerceValue));

    public double DefaultValue
    {
        get => (double)GetValue(DefaultValueProperty);
        set => SetValue(DefaultValueProperty, value);
    }

    public static readonly DependencyProperty DefaultValueProperty =
        ElementBase.Property<NSlider, double>(nameof(DefaultValueProperty), 0d);

    public bool Range
    {
        get => (bool)GetValue(RangeProperty);
        set => SetValue(RangeProperty, value);
    }

    public static readonly DependencyProperty RangeProperty =
        ElementBase.Property<NSlider, bool>(nameof(RangeProperty), false, OnRangePropertyChanged);

    public double RangeStart
    {
        get => (double)GetValue(RangeStartProperty);
        set => SetValue(RangeStartProperty, value);
    }

    public static readonly DependencyProperty RangeStartProperty =
        DependencyProperty.Register(
            nameof(RangeStart),
            typeof(double),
            typeof(NSlider),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnRangeValuePropertyChanged, CoerceValue));

    public double RangeEnd
    {
        get => (double)GetValue(RangeEndProperty);
        set => SetValue(RangeEndProperty, value);
    }

    public static readonly DependencyProperty RangeEndProperty =
        DependencyProperty.Register(
            nameof(RangeEnd),
            typeof(double),
            typeof(NSlider),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnRangeValuePropertyChanged, CoerceValue));

    public double DefaultRangeStart
    {
        get => (double)GetValue(DefaultRangeStartProperty);
        set => SetValue(DefaultRangeStartProperty, value);
    }

    public static readonly DependencyProperty DefaultRangeStartProperty =
        ElementBase.Property<NSlider, double>(nameof(DefaultRangeStartProperty), 0d);

    public double DefaultRangeEnd
    {
        get => (double)GetValue(DefaultRangeEndProperty);
        set => SetValue(DefaultRangeEndProperty, value);
    }

    public static readonly DependencyProperty DefaultRangeEndProperty =
        ElementBase.Property<NSlider, double>(nameof(DefaultRangeEndProperty), 0d);

    public double Min
    {
        get => (double)GetValue(MinProperty);
        set => SetValue(MinProperty, value);
    }

    public static readonly DependencyProperty MinProperty =
        ElementBase.Property<NSlider, double>(nameof(MinProperty), 0d, OnBoundsPropertyChanged);

    public double Max
    {
        get => (double)GetValue(MaxProperty);
        set => SetValue(MaxProperty, value);
    }

    public static readonly DependencyProperty MaxProperty =
        ElementBase.Property<NSlider, double>(nameof(MaxProperty), 100d, OnBoundsPropertyChanged);

    public object Step
    {
        get => GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    public static readonly DependencyProperty StepProperty =
        ElementBase.Property<NSlider, object>(nameof(StepProperty), 1d, OnLayoutPropertyChanged);

    public bool Keyboard
    {
        get => (bool)GetValue(KeyboardProperty);
        set => SetValue(KeyboardProperty, value);
    }

    public static readonly DependencyProperty KeyboardProperty =
        ElementBase.Property<NSlider, bool>(nameof(KeyboardProperty), true);

    public bool Tooltip
    {
        get => (bool)GetValue(TooltipProperty);
        set => SetValue(TooltipProperty, value);
    }

    public static readonly DependencyProperty TooltipProperty =
        ElementBase.Property<NSlider, bool>(nameof(TooltipProperty), true, OnLayoutPropertyChanged);

    public bool? ShowTooltip
    {
        get => (bool?)GetValue(ShowTooltipProperty);
        set => SetValue(ShowTooltipProperty, value);
    }

    public static readonly DependencyProperty ShowTooltipProperty =
        ElementBase.Property<NSlider, bool?>(nameof(ShowTooltipProperty), null, OnLayoutPropertyChanged);

    public bool Vertical
    {
        get => (bool)GetValue(VerticalProperty);
        set => SetValue(VerticalProperty, value);
    }

    public static readonly DependencyProperty VerticalProperty =
        ElementBase.Property<NSlider, bool>(nameof(VerticalProperty), false, OnLayoutPropertyChanged);

    public bool Reverse
    {
        get => (bool)GetValue(ReverseProperty);
        set => SetValue(ReverseProperty, value);
    }

    public static readonly DependencyProperty ReverseProperty =
        ElementBase.Property<NSlider, bool>(nameof(ReverseProperty), false, OnLayoutPropertyChanged);

    public bool Disabled
    {
        get => (bool)GetValue(DisabledProperty);
        set => SetValue(DisabledProperty, value);
    }

    public static readonly DependencyProperty DisabledProperty =
        ElementBase.Property<NSlider, bool>(nameof(DisabledProperty), false, OnDisabledPropertyChanged);

    public NSliderTooltipPlacement Placement
    {
        get => (NSliderTooltipPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public static readonly DependencyProperty PlacementProperty =
        ElementBase.Property<NSlider, NSliderTooltipPlacement>(nameof(PlacementProperty), NSliderTooltipPlacement.Top, OnLayoutPropertyChanged);

    public NSliderMarkCollection Marks
    {
        get => (NSliderMarkCollection)GetValue(MarksProperty);
        set => SetValue(MarksProperty, value);
    }

    public static readonly DependencyProperty MarksProperty =
        DependencyProperty.Register(
            nameof(Marks),
            typeof(NSliderMarkCollection),
            typeof(NSlider),
            new PropertyMetadata(null, OnMarksPropertyChanged));

    public string? FormatString
    {
        get => (string?)GetValue(FormatStringProperty);
        set => SetValue(FormatStringProperty, value);
    }

    public static readonly DependencyProperty FormatStringProperty =
        ElementBase.Property<NSlider, string?>(nameof(FormatStringProperty), null, OnLayoutPropertyChanged);

    public string TooltipPrefix
    {
        get => (string)GetValue(TooltipPrefixProperty);
        set => SetValue(TooltipPrefixProperty, value);
    }

    public static readonly DependencyProperty TooltipPrefixProperty =
        ElementBase.Property<NSlider, string>(nameof(TooltipPrefixProperty), string.Empty, OnLayoutPropertyChanged);

    public string TooltipSuffix
    {
        get => (string)GetValue(TooltipSuffixProperty);
        set => SetValue(TooltipSuffixProperty, value);
    }

    public static readonly DependencyProperty TooltipSuffixProperty =
        ElementBase.Property<NSlider, string>(nameof(TooltipSuffixProperty), string.Empty, OnLayoutPropertyChanged);

    public Func<double, string>? FormatTooltip
    {
        get => (Func<double, string>?)GetValue(FormatTooltipProperty);
        set => SetValue(FormatTooltipProperty, value);
    }

    public static readonly DependencyProperty FormatTooltipProperty =
        ElementBase.Property<NSlider, Func<double, string>?>(nameof(FormatTooltipProperty), null, OnLayoutPropertyChanged);

    public DataTemplate? IndicatorTemplate
    {
        get => (DataTemplate?)GetValue(IndicatorTemplateProperty);
        set => SetValue(IndicatorTemplateProperty, value);
    }

    public static readonly DependencyProperty IndicatorTemplateProperty =
        ElementBase.Property<NSlider, DataTemplate?>(nameof(IndicatorTemplateProperty), null, OnLayoutPropertyChanged);

    public DataTemplate? MarkTemplate
    {
        get => (DataTemplate?)GetValue(MarkTemplateProperty);
        set => SetValue(MarkTemplateProperty, value);
    }

    public static readonly DependencyProperty MarkTemplateProperty =
        ElementBase.Property<NSlider, DataTemplate?>(nameof(MarkTemplateProperty), null, OnLayoutPropertyChanged);

    public DataTemplate? MarkDotTemplate
    {
        get => (DataTemplate?)GetValue(MarkDotTemplateProperty);
        set => SetValue(MarkDotTemplateProperty, value);
    }

    public static readonly DependencyProperty MarkDotTemplateProperty =
        ElementBase.Property<NSlider, DataTemplate?>(nameof(MarkDotTemplateProperty), null, OnLayoutPropertyChanged);

    public DataTemplate? ThumbTemplate
    {
        get => (DataTemplate?)GetValue(ThumbTemplateProperty);
        set => SetValue(ThumbTemplateProperty, value);
    }

    public static readonly DependencyProperty ThumbTemplateProperty =
        ElementBase.Property<NSlider, DataTemplate?>(nameof(ThumbTemplateProperty), null, OnThumbTemplatePropertyChanged);

    public bool HasCustomThumbTemplate
    {
        get => (bool)GetValue(HasCustomThumbTemplateProperty);
        private set => SetValue(HasCustomThumbTemplatePropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasCustomThumbTemplatePropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(HasCustomThumbTemplate),
            typeof(bool),
            typeof(NSlider),
            new PropertyMetadata(false));

    public static readonly DependencyProperty HasCustomThumbTemplateProperty = HasCustomThumbTemplatePropertyKey.DependencyProperty;

    public double RailSize
    {
        get => (double)GetValue(RailSizeProperty);
        set => SetValue(RailSizeProperty, value);
    }

    public static readonly DependencyProperty RailSizeProperty =
        ElementBase.Property<NSlider, double>(nameof(RailSizeProperty), 4d, OnRailSizePropertyChanged);

    public double RailHeight
    {
        get => (double)GetValue(RailHeightProperty);
        set => SetValue(RailHeightProperty, value);
    }

    public static readonly DependencyProperty RailHeightProperty =
        ElementBase.Property<NSlider, double>(nameof(RailHeightProperty), 4d, OnLayoutPropertyChanged);

    public double RailWidthVertical
    {
        get => (double)GetValue(RailWidthVerticalProperty);
        set => SetValue(RailWidthVerticalProperty, value);
    }

    public static readonly DependencyProperty RailWidthVerticalProperty =
        ElementBase.Property<NSlider, double>(nameof(RailWidthVerticalProperty), 4d, OnLayoutPropertyChanged);

    public double HandleSize
    {
        get => (double)GetValue(HandleSizeProperty);
        set => SetValue(HandleSizeProperty, value);
    }

    public static readonly DependencyProperty HandleSizeProperty =
        ElementBase.Property<NSlider, double>(nameof(HandleSizeProperty), 18d, OnLayoutPropertyChanged);

    public double DotWidth
    {
        get => (double)GetValue(DotWidthProperty);
        set => SetValue(DotWidthProperty, value);
    }

    public static readonly DependencyProperty DotWidthProperty =
        ElementBase.Property<NSlider, double>(nameof(DotWidthProperty), 8d, OnLayoutPropertyChanged);

    public double DotHeight
    {
        get => (double)GetValue(DotHeightProperty);
        set => SetValue(DotHeightProperty, value);
    }

    public static readonly DependencyProperty DotHeightProperty =
        ElementBase.Property<NSlider, double>(nameof(DotHeightProperty), 8d, OnLayoutPropertyChanged);

    public double DotBorderRadius
    {
        get => (double)GetValue(DotBorderRadiusProperty);
        set => SetValue(DotBorderRadiusProperty, value);
    }

    public static readonly DependencyProperty DotBorderRadiusProperty =
        ElementBase.Property<NSlider, double>(nameof(DotBorderRadiusProperty), 4d, OnLayoutPropertyChanged);

    public double MarkFontSize
    {
        get => (double)GetValue(MarkFontSizeProperty);
        set => SetValue(MarkFontSizeProperty, value);
    }

    public static readonly DependencyProperty MarkFontSizeProperty =
        ElementBase.Property<NSlider, double>(nameof(MarkFontSizeProperty), 12d, OnLayoutPropertyChanged);

    public Brush? RailBrush
    {
        get => (Brush?)GetValue(RailBrushProperty);
        set => SetValue(RailBrushProperty, value);
    }

    public static readonly DependencyProperty RailBrushProperty =
        ElementBase.Property<NSlider, Brush?>(nameof(RailBrushProperty), null);

    public Brush? RailHoverBrush
    {
        get => (Brush?)GetValue(RailHoverBrushProperty);
        set => SetValue(RailHoverBrushProperty, value);
    }

    public static readonly DependencyProperty RailHoverBrushProperty =
        ElementBase.Property<NSlider, Brush?>(nameof(RailHoverBrushProperty), null);

    public Brush? RailDisabledBrush
    {
        get => (Brush?)GetValue(RailDisabledBrushProperty);
        set => SetValue(RailDisabledBrushProperty, value);
    }

    public static readonly DependencyProperty RailDisabledBrushProperty =
        ElementBase.Property<NSlider, Brush?>(nameof(RailDisabledBrushProperty), null);

    public Brush? FillBrush
    {
        get => (Brush?)GetValue(FillBrushProperty);
        set => SetValue(FillBrushProperty, value);
    }

    public static readonly DependencyProperty FillBrushProperty =
        ElementBase.Property<NSlider, Brush?>(nameof(FillBrushProperty), null);

    public Brush? FillHoverBrush
    {
        get => (Brush?)GetValue(FillHoverBrushProperty);
        set => SetValue(FillHoverBrushProperty, value);
    }

    public static readonly DependencyProperty FillHoverBrushProperty =
        ElementBase.Property<NSlider, Brush?>(nameof(FillHoverBrushProperty), null);

    public Brush? FillDisabledBrush
    {
        get => (Brush?)GetValue(FillDisabledBrushProperty);
        set => SetValue(FillDisabledBrushProperty, value);
    }

    public static readonly DependencyProperty FillDisabledBrushProperty =
        ElementBase.Property<NSlider, Brush?>(nameof(FillDisabledBrushProperty), null);

    public Brush? HandleBrush
    {
        get => (Brush?)GetValue(HandleBrushProperty);
        set => SetValue(HandleBrushProperty, value);
    }

    public static readonly DependencyProperty HandleBrushProperty =
        ElementBase.Property<NSlider, Brush?>(nameof(HandleBrushProperty), null);

    public Brush? HandleBorderBrush
    {
        get => (Brush?)GetValue(HandleBorderBrushProperty);
        set => SetValue(HandleBorderBrushProperty, value);
    }

    public static readonly DependencyProperty HandleBorderBrushProperty =
        ElementBase.Property<NSlider, Brush?>(nameof(HandleBorderBrushProperty), null);

    public Brush? HandleDisabledBrush
    {
        get => (Brush?)GetValue(HandleDisabledBrushProperty);
        set => SetValue(HandleDisabledBrushProperty, value);
    }

    public static readonly DependencyProperty HandleDisabledBrushProperty =
        ElementBase.Property<NSlider, Brush?>(nameof(HandleDisabledBrushProperty), null);

    public Brush? HandleDisabledBorderBrush
    {
        get => (Brush?)GetValue(HandleDisabledBorderBrushProperty);
        set => SetValue(HandleDisabledBorderBrushProperty, value);
    }

    public static readonly DependencyProperty HandleDisabledBorderBrushProperty =
        ElementBase.Property<NSlider, Brush?>(nameof(HandleDisabledBorderBrushProperty), null);

    public Brush? DotBrush
    {
        get => (Brush?)GetValue(DotBrushProperty);
        set => SetValue(DotBrushProperty, value);
    }

    public static readonly DependencyProperty DotBrushProperty =
        ElementBase.Property<NSlider, Brush?>(nameof(DotBrushProperty), null, OnLayoutPropertyChanged);

    public Brush? DotActiveBrush
    {
        get => (Brush?)GetValue(DotActiveBrushProperty);
        set => SetValue(DotActiveBrushProperty, value);
    }

    public static readonly DependencyProperty DotActiveBrushProperty =
        ElementBase.Property<NSlider, Brush?>(nameof(DotActiveBrushProperty), null, OnLayoutPropertyChanged);

    public Brush? DotBorderBrush
    {
        get => (Brush?)GetValue(DotBorderBrushProperty);
        set => SetValue(DotBorderBrushProperty, value);
    }

    public static readonly DependencyProperty DotBorderBrushProperty =
        ElementBase.Property<NSlider, Brush?>(nameof(DotBorderBrushProperty), null, OnLayoutPropertyChanged);

    public Brush? DotActiveBorderBrush
    {
        get => (Brush?)GetValue(DotActiveBorderBrushProperty);
        set => SetValue(DotActiveBorderBrushProperty, value);
    }

    public static readonly DependencyProperty DotActiveBorderBrushProperty =
        ElementBase.Property<NSlider, Brush?>(nameof(DotActiveBorderBrushProperty), null, OnLayoutPropertyChanged);

    public Brush? DotDisabledBrush
    {
        get => (Brush?)GetValue(DotDisabledBrushProperty);
        set => SetValue(DotDisabledBrushProperty, value);
    }

    public static readonly DependencyProperty DotDisabledBrushProperty =
        ElementBase.Property<NSlider, Brush?>(nameof(DotDisabledBrushProperty), null, OnLayoutPropertyChanged);

    public Brush? DotDisabledBorderBrush
    {
        get => (Brush?)GetValue(DotDisabledBorderBrushProperty);
        set => SetValue(DotDisabledBorderBrushProperty, value);
    }

    public static readonly DependencyProperty DotDisabledBorderBrushProperty =
        ElementBase.Property<NSlider, Brush?>(nameof(DotDisabledBorderBrushProperty), null, OnLayoutPropertyChanged);

    public Brush? MarkForeground
    {
        get => (Brush?)GetValue(MarkForegroundProperty);
        set => SetValue(MarkForegroundProperty, value);
    }

    public static readonly DependencyProperty MarkForegroundProperty =
        ElementBase.Property<NSlider, Brush?>(nameof(MarkForegroundProperty), null, OnLayoutPropertyChanged);

    public Brush? MarkDisabledForeground
    {
        get => (Brush?)GetValue(MarkDisabledForegroundProperty);
        set => SetValue(MarkDisabledForegroundProperty, value);
    }

    public static readonly DependencyProperty MarkDisabledForegroundProperty =
        ElementBase.Property<NSlider, Brush?>(nameof(MarkDisabledForegroundProperty), null, OnLayoutPropertyChanged);

    public Brush? IndicatorBrush
    {
        get => (Brush?)GetValue(IndicatorBrushProperty);
        set => SetValue(IndicatorBrushProperty, value);
    }

    public static readonly DependencyProperty IndicatorBrushProperty =
        ElementBase.Property<NSlider, Brush?>(nameof(IndicatorBrushProperty), null);

    public Brush? IndicatorForeground
    {
        get => (Brush?)GetValue(IndicatorForegroundProperty);
        set => SetValue(IndicatorForegroundProperty, value);
    }

    public static readonly DependencyProperty IndicatorForegroundProperty =
        ElementBase.Property<NSlider, Brush?>(nameof(IndicatorForegroundProperty), null);

    public double IndicatorBorderRadius
    {
        get => (double)GetValue(IndicatorBorderRadiusProperty);
        set => SetValue(IndicatorBorderRadiusProperty, value);
    }

    public static readonly DependencyProperty IndicatorBorderRadiusProperty =
        ElementBase.Property<NSlider, double>(nameof(IndicatorBorderRadiusProperty), 3d);

    public double DisabledOpacity
    {
        get => (double)GetValue(DisabledOpacityProperty);
        set => SetValue(DisabledOpacityProperty, value);
    }

    public static readonly DependencyProperty DisabledOpacityProperty =
        ElementBase.Property<NSlider, double>(nameof(DisabledOpacityProperty), 0.5d);

    public static readonly RoutedEvent ValueChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(ValueChanged), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), typeof(NSlider));

    public event RoutedPropertyChangedEventHandler<double> ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    public static readonly RoutedEvent RangeValueChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(RangeValueChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NSlider));

    public event RoutedEventHandler RangeValueChanged
    {
        add => AddHandler(RangeValueChangedEvent, value);
        remove => RemoveHandler(RangeValueChangedEvent, value);
    }

    public static readonly RoutedEvent DragStartedEvent =
        EventManager.RegisterRoutedEvent(nameof(DragStarted), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NSlider));

    public event RoutedEventHandler DragStarted
    {
        add => AddHandler(DragStartedEvent, value);
        remove => RemoveHandler(DragStartedEvent, value);
    }

    public static readonly RoutedEvent DragCompletedEvent =
        EventManager.RegisterRoutedEvent(nameof(DragCompleted), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NSlider));

    public event RoutedEventHandler DragCompleted
    {
        add => AddHandler(DragCompletedEvent, value);
        remove => RemoveHandler(DragCompletedEvent, value);
    }

    public override void OnApplyTemplate()
    {
        DetachTemplateEvents();
        base.OnApplyTemplate();

        railCanvasPart = GetTemplateChild(RailCanvasPartName) as Canvas;
        railPart = GetTemplateChild(RailPartName) as Border;
        fillPart = GetTemplateChild(FillPartName) as Border;
        startThumbPart = GetTemplateChild(StartThumbPartName) as Thumb;
        endThumbPart = GetTemplateChild(EndThumbPartName) as Thumb;
        startIndicatorPart = GetTemplateChild(StartIndicatorPartName) as Border;
        endIndicatorPart = GetTemplateChild(EndIndicatorPartName) as Border;
        startIndicatorContentPart = GetTemplateChild(StartIndicatorContentPartName) as ContentPresenter;
        endIndicatorContentPart = GetTemplateChild(EndIndicatorContentPartName) as ContentPresenter;

        AttachTemplateEvents();
        UpdateLayoutState();
    }

    public void Reset()
    {
        if (Range)
        {
            SetCurrentValue(RangeStartProperty, SanitizeValue(DefaultRangeStart, RangeStart));
            SetCurrentValue(RangeEndProperty, SanitizeValue(DefaultRangeEnd, RangeEnd));
            NormalizeRangeValues();
            return;
        }

        SetCurrentValue(ValueProperty, SanitizeValue(DefaultValue, Value));
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (!Keyboard || Disabled || !IsEnabled)
        {
            return;
        }

        var ratio = e.Key switch
        {
            Key.Up => Vertical && Reverse ? -1 : 1,
            Key.Right => !Vertical && Reverse ? -1 : 1,
            Key.Down => Vertical && Reverse ? 1 : -1,
            Key.Left => !Vertical && Reverse ? 1 : -1,
            _ => 0
        };

        if (ratio == 0)
        {
            return;
        }

        e.Handled = true;
        var targetIndex = Range ? activeThumbIndex : 0;
        var currentValue = targetIndex == 0 ? GetStartValue() : GetEndValue();
        SetHandleValue(targetIndex, SanitizeSteppingValue(currentValue, ratio));
    }

    private static object CoerceValue(DependencyObject d, object baseValue)
    {
        if (d is not NSlider slider || baseValue is not double value)
        {
            return baseValue;
        }

        return slider.ClampValue(value);
    }

    private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NSlider slider || slider.isSyncingValues)
        {
            return;
        }

        var oldValue = (double)e.OldValue;
        var newValue = slider.SanitizeValue((double)e.NewValue, oldValue);
        if (!SliderDoubleUtil.AreClose(newValue, (double)e.NewValue))
        {
            slider.SetCurrentValue(ValueProperty, newValue);
            return;
        }

        slider.RaiseEvent(new RoutedPropertyChangedEventArgs<double>(oldValue, newValue, ValueChangedEvent));
        slider.UpdateLayoutState();
    }

    private static void OnRangeValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NSlider slider || slider.isSyncingValues)
        {
            return;
        }

        var oldValue = (double)e.OldValue;
        var newValue = slider.SanitizeValue((double)e.NewValue, oldValue);
        if (!SliderDoubleUtil.AreClose(newValue, (double)e.NewValue))
        {
            slider.SetCurrentValue(e.Property, newValue);
            return;
        }

        slider.NormalizeRangeValues();
        slider.RaiseEvent(new RoutedEventArgs(RangeValueChangedEvent));
        slider.UpdateLayoutState();
    }

    private static void OnRangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NSlider slider)
        {
            slider.NormalizeRangeValues();
            slider.UpdateLayoutState();
        }
    }

    private static void OnBoundsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NSlider slider)
        {
            slider.CoerceValue(ValueProperty);
            slider.CoerceValue(RangeStartProperty);
            slider.CoerceValue(RangeEndProperty);
            slider.NormalizeRangeValues();
            slider.UpdateLayoutState();
        }
    }

    private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NSlider slider)
        {
            slider.UpdateLayoutState();
        }
    }

    private static void OnThumbTemplatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NSlider slider)
        {
            slider.HasCustomThumbTemplate = e.NewValue is DataTemplate;
            slider.UpdateLayoutState();
        }
    }

    private static void OnRailSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NSlider slider)
        {
            return;
        }

        var oldSize = e.OldValue is double oldValue ? oldValue : 4d;
        var nextSize = e.NewValue is double newValue ? Math.Max(0d, newValue) : 4d;
        if (!SliderDoubleUtil.AreClose(nextSize, slider.RailSize))
        {
            slider.SetCurrentValue(RailSizeProperty, nextSize);
            return;
        }

        if (slider.ReadLocalValue(RailHeightProperty) == DependencyProperty.UnsetValue
            || SliderDoubleUtil.AreClose(slider.RailHeight, oldSize))
        {
            slider.SetCurrentValue(RailHeightProperty, nextSize);
        }

        if (slider.ReadLocalValue(RailWidthVerticalProperty) == DependencyProperty.UnsetValue
            || SliderDoubleUtil.AreClose(slider.RailWidthVertical, oldSize))
        {
            slider.SetCurrentValue(RailWidthVerticalProperty, nextSize);
        }

        slider.UpdateLayoutState();
    }

    private static void OnDisabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NSlider slider || slider.isSyncingDisabled)
        {
            return;
        }

        slider.isSyncingDisabled = true;
        slider.SetCurrentValue(IsEnabledProperty, !(bool)e.NewValue);
        slider.isSyncingDisabled = false;
        slider.UpdateLayoutState();
    }

    private static void OnMarksPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NSlider slider)
        {
            return;
        }

        if (e.OldValue is INotifyCollectionChanged oldCollection)
        {
            oldCollection.CollectionChanged -= slider.HandleMarksCollectionChanged;
        }

        if (e.NewValue is INotifyCollectionChanged newCollection)
        {
            newCollection.CollectionChanged += slider.HandleMarksCollectionChanged;
        }

        slider.UpdateLayoutState();
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        if (ReadLocalValue(ValueProperty) == DependencyProperty.UnsetValue
            && SliderDoubleUtil.AreClose(Value, 0d)
            && !SliderDoubleUtil.AreClose(DefaultValue, 0d))
        {
            SetCurrentValue(ValueProperty, SanitizeValue(DefaultValue, Value));
        }

        UpdateLayoutState();
    }

    private void HandleSizeChanged(object sender, SizeChangedEventArgs e) => UpdateLayoutState();

    private void HandleIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (isSyncingDisabled)
        {
            return;
        }

        isSyncingDisabled = true;
        SetCurrentValue(DisabledProperty, !(bool)e.NewValue);
        isSyncingDisabled = false;
        UpdateLayoutState();
    }

    private void HandleMarksCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => UpdateLayoutState();

    private void AttachTemplateEvents()
    {
        if (railCanvasPart is not null)
        {
            railCanvasPart.MouseLeftButtonDown += HandleRailMouseLeftButtonDown;
        }

        AttachThumbEvents(startThumbPart, 0);
        AttachThumbEvents(endThumbPart, 1);
    }

    private void DetachTemplateEvents()
    {
        if (railCanvasPart is not null)
        {
            railCanvasPart.MouseLeftButtonDown -= HandleRailMouseLeftButtonDown;
        }

        DetachThumbEvents(startThumbPart, 0);
        DetachThumbEvents(endThumbPart, 1);
    }

    private void AttachThumbEvents(Thumb? thumb, int index)
    {
        if (thumb is null)
        {
            return;
        }

        thumb.DragStarted += index == 0 ? HandleStartThumbDragStarted : HandleEndThumbDragStarted;
        thumb.DragDelta += index == 0 ? HandleStartThumbDragDelta : HandleEndThumbDragDelta;
        thumb.DragCompleted += index == 0 ? HandleStartThumbDragCompleted : HandleEndThumbDragCompleted;
        thumb.GotKeyboardFocus += index == 0 ? HandleStartThumbGotKeyboardFocus : HandleEndThumbGotKeyboardFocus;
        thumb.MouseEnter += index == 0 ? HandleStartThumbMouseEnter : HandleEndThumbMouseEnter;
        thumb.MouseLeave += index == 0 ? HandleStartThumbMouseLeave : HandleEndThumbMouseLeave;
    }

    private void DetachThumbEvents(Thumb? thumb, int index)
    {
        if (thumb is null)
        {
            return;
        }

        thumb.DragStarted -= index == 0 ? HandleStartThumbDragStarted : HandleEndThumbDragStarted;
        thumb.DragDelta -= index == 0 ? HandleStartThumbDragDelta : HandleEndThumbDragDelta;
        thumb.DragCompleted -= index == 0 ? HandleStartThumbDragCompleted : HandleEndThumbDragCompleted;
        thumb.GotKeyboardFocus -= index == 0 ? HandleStartThumbGotKeyboardFocus : HandleEndThumbGotKeyboardFocus;
        thumb.MouseEnter -= index == 0 ? HandleStartThumbMouseEnter : HandleEndThumbMouseEnter;
        thumb.MouseLeave -= index == 0 ? HandleStartThumbMouseLeave : HandleEndThumbMouseLeave;
    }

    private void HandleRailMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (Disabled || !IsEnabled || railCanvasPart is null)
        {
            return;
        }

        Focus();
        var point = e.GetPosition(railCanvasPart);
        var value = SanitizeValue(PointToValue(point), Value);
        var targetIndex = 0;

        if (Range)
        {
            var startDistance = Math.Abs(value - RangeStart);
            var endDistance = Math.Abs(value - RangeEnd);
            targetIndex = endDistance < startDistance ? 1 : 0;
        }

        activeThumbIndex = targetIndex;
        SetHandleValue(targetIndex, value);
        FocusThumb(targetIndex);
        e.Handled = true;
    }

    private void HandleStartThumbDragStarted(object sender, DragStartedEventArgs e) => StartDrag(0);

    private void HandleEndThumbDragStarted(object sender, DragStartedEventArgs e) => StartDrag(1);

    private void HandleStartThumbDragDelta(object sender, DragDeltaEventArgs e) => DragThumb(0, e);

    private void HandleEndThumbDragDelta(object sender, DragDeltaEventArgs e) => DragThumb(1, e);

    private void HandleStartThumbDragCompleted(object sender, DragCompletedEventArgs e) => CompleteDrag();

    private void HandleEndThumbDragCompleted(object sender, DragCompletedEventArgs e) => CompleteDrag();

    private void HandleStartThumbGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        activeThumbIndex = 0;
        UpdateLayoutState();
    }

    private void HandleEndThumbGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        activeThumbIndex = 1;
        UpdateLayoutState();
    }

    private void HandleStartThumbMouseEnter(object sender, MouseEventArgs e)
    {
        isHoveringStart = true;
        UpdateTooltipVisibility();
    }

    private void HandleStartThumbMouseLeave(object sender, MouseEventArgs e)
    {
        isHoveringStart = false;
        UpdateTooltipVisibility();
    }

    private void HandleEndThumbMouseEnter(object sender, MouseEventArgs e)
    {
        isHoveringEnd = true;
        UpdateTooltipVisibility();
    }

    private void HandleEndThumbMouseLeave(object sender, MouseEventArgs e)
    {
        isHoveringEnd = false;
        UpdateTooltipVisibility();
    }

    private void StartDrag(int index)
    {
        activeThumbIndex = index;
        isDragging = true;
        RaiseEvent(new RoutedEventArgs(DragStartedEvent));
        UpdateLayoutState();
    }

    private void DragThumb(int index, DragDeltaEventArgs e)
    {
        if (railCanvasPart is null)
        {
            return;
        }

        var delta = Vertical ? -e.VerticalChange : e.HorizontalChange;
        if (Reverse)
        {
            delta = -delta;
        }

        var rangePixels = GetRailLength();
        if (rangePixels <= 0)
        {
            return;
        }

        var valueDelta = delta / rangePixels * (Max - Min);
        var currentValue = index == 0 ? GetStartValue() : GetEndValue();
        SetHandleValue(index, SanitizeValue(currentValue + valueDelta, currentValue));
    }

    private void CompleteDrag()
    {
        isDragging = false;
        RaiseEvent(new RoutedEventArgs(DragCompletedEvent));
        UpdateLayoutState();
    }

    private void FocusThumb(int index)
    {
        if (index == 0)
        {
            startThumbPart?.Focus();
            return;
        }

        endThumbPart?.Focus();
    }

    private void SetHandleValue(int index, double value)
    {
        if (!Range)
        {
            SetCurrentValue(ValueProperty, value);
            return;
        }

        if (index == 0)
        {
            SetCurrentValue(RangeStartProperty, Math.Min(value, RangeEnd));
            return;
        }

        SetCurrentValue(RangeEndProperty, Math.Max(value, RangeStart));
    }

    private double SanitizeSteppingValue(double currentValue, int ratio)
    {
        if (IsStepMark())
        {
            return GetClosestMark(currentValue, ratio) ?? currentValue;
        }

        var step = GetNumericStep();
        if (step <= 0)
        {
            return currentValue;
        }

        return SanitizeValue(currentValue + step * ratio, currentValue);
    }

    private double SanitizeValue(double value, double currentValue)
    {
        if (IsStepMark())
        {
            return GetClosestMark(value, 0) ?? ClampValue(value);
        }

        var step = GetNumericStep();
        if (step <= 0)
        {
            return ClampValue(value);
        }

        var precision = GetStepPrecision(step);
        var rounded = Math.Round((value - Min) / step) * step + Min;
        return ClampValue(Math.Round(rounded, precision));
    }

    private double? GetClosestMark(double value, int direction)
    {
        if (Marks.Count == 0)
        {
            return null;
        }

        var values = Marks.Select(static mark => mark.Value).Where(static markValue => !double.IsNaN(markValue)).ToList();
        if (direction != 0)
        {
            values = values.Where(markValue => (markValue - value) * direction > 0).ToList();
        }

        return values.Count == 0 ? null : values.OrderBy(markValue => Math.Abs(markValue - value)).First();
    }

    private double ClampValue(double value)
    {
        var min = Math.Min(Min, Max);
        var max = Math.Max(Min, Max);
        return Math.Min(max, Math.Max(min, value));
    }

    private void NormalizeRangeValues()
    {
        if (!Range || isSyncingValues)
        {
            return;
        }

        var start = ClampValue(RangeStart);
        var end = ClampValue(RangeEnd);
        if (start > end)
        {
            (start, end) = (end, start);
        }

        isSyncingValues = true;
        SetCurrentValue(RangeStartProperty, start);
        SetCurrentValue(RangeEndProperty, end);
        isSyncingValues = false;
    }

    private double GetStartValue() => Range ? RangeStart : Value;

    private double GetEndValue() => Range ? RangeEnd : Value;

    private bool IsStepMark()
    {
        return Step is string stepString && string.Equals(stepString, "mark", StringComparison.OrdinalIgnoreCase);
    }

    private double GetNumericStep()
    {
        return Step switch
        {
            double doubleValue => doubleValue,
            int intValue => intValue,
            decimal decimalValue => (double)decimalValue,
            string stringValue when double.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) => parsed,
            string stringValue when double.TryParse(stringValue, NumberStyles.Float, CultureInfo.CurrentCulture, out var parsed) => parsed,
            _ => 1d
        };
    }

    private static int GetStepPrecision(double step)
    {
        var stepString = step.ToString(CultureInfo.InvariantCulture);
        var decimalIndex = stepString.IndexOf('.', StringComparison.Ordinal);
        return decimalIndex < 0 ? 0 : stepString.Length - decimalIndex - 1;
    }

    private double ValueToPercent(double value)
    {
        var range = Max - Min;
        if (SliderDoubleUtil.AreClose(range, 0d))
        {
            return 0d;
        }

        return (ClampValue(value) - Min) / range;
    }

    private double PointToValue(Point point)
    {
        var length = GetRailLength();
        if (length <= 0)
        {
            return Min;
        }

        var offset = Vertical ? ActualHeight - point.Y : point.X;
        var percent = Math.Min(1d, Math.Max(0d, offset / length));
        if (Reverse)
        {
            percent = 1d - percent;
        }

        return Min + (Max - Min) * percent;
    }

    private double GetRailLength() => Vertical ? ActualHeight : ActualWidth;

    private void UpdateLayoutState()
    {
        if (railCanvasPart is null)
        {
            return;
        }

        UpdateRail();
        UpdateFill();
        UpdateThumb(startThumbPart, GetStartValue(), 0);
        UpdateThumb(endThumbPart, GetEndValue(), 1);
        UpdateIndicators();
        RebuildMarks();
    }

    private void UpdateRail()
    {
        if (railPart is null)
        {
            return;
        }

        if (Vertical)
        {
            var thickness = GetRailThickness();
            railPart.CornerRadius = CreateRailCornerRadius();
            railPart.Background = GetRailBackground();
            railPart.Width = thickness;
            railPart.Height = ActualHeight;
            Canvas.SetLeft(railPart, Math.Max(0d, (ActualWidth - thickness) / 2d));
            Canvas.SetTop(railPart, 0d);
            return;
        }

        var railHeight = GetRailThickness();
        railPart.CornerRadius = CreateRailCornerRadius();
        railPart.Background = GetRailBackground();
        railPart.Width = ActualWidth;
        railPart.Height = railHeight;
        Canvas.SetLeft(railPart, 0d);
        Canvas.SetTop(railPart, Math.Max(0d, (ActualHeight - railHeight) / 2d));
    }

    private void UpdateFill()
    {
        if (fillPart is null)
        {
            return;
        }

        var startPercent = Range ? ValueToPercent(Math.Min(RangeStart, RangeEnd)) : ValueToPercent(Min);
        var endPercent = Range ? ValueToPercent(Math.Max(RangeStart, RangeEnd)) : ValueToPercent(Value);
        if (Reverse)
        {
            (startPercent, endPercent) = (1d - endPercent, 1d - startPercent);
        }

        if (Vertical)
        {
            var length = ActualHeight;
            var thickness = GetRailThickness();
            var bottom = length * startPercent;
            var height = Math.Max(0d, length * (endPercent - startPercent));
            fillPart.CornerRadius = CreateRailCornerRadius();
            fillPart.Background = GetFillBackground();
            fillPart.Width = thickness;
            fillPart.Height = height;
            Canvas.SetLeft(fillPart, Math.Max(0d, (ActualWidth - thickness) / 2d));
            Canvas.SetTop(fillPart, Math.Max(0d, length - bottom - height));
            return;
        }

        var railHeight = GetRailThickness();
        fillPart.CornerRadius = CreateRailCornerRadius();
        fillPart.Background = GetFillBackground();
        fillPart.Width = Math.Max(0d, ActualWidth * (endPercent - startPercent));
        fillPart.Height = railHeight;
        Canvas.SetLeft(fillPart, ActualWidth * startPercent);
        Canvas.SetTop(fillPart, Math.Max(0d, (ActualHeight - railHeight) / 2d));
    }

    private double GetRailThickness() => Math.Max(0d, Vertical ? RailWidthVertical : RailHeight);

    private CornerRadius CreateRailCornerRadius()
    {
        var radius = GetRailThickness() / 2d;
        return new CornerRadius(radius);
    }

    private void UpdateThumb(Thumb? thumb, double value, int index)
    {
        if (thumb is null)
        {
            return;
        }

        thumb.Width = HandleSize;
        thumb.Height = HandleSize;
        thumb.Tag = value;
        thumb.Visibility = !Range && index == 1 ? Visibility.Collapsed : Visibility.Visible;

        var center = GetHandleCenter(value);
        Canvas.SetLeft(thumb, center.X - HandleSize / 2d);
        Canvas.SetTop(thumb, center.Y - HandleSize / 2d);
        Panel.SetZIndex(thumb, index == activeThumbIndex ? 50 : 40);
    }

    private Point GetHandleCenter(double value)
    {
        var percent = ValueToPercent(value);
        if (Reverse)
        {
            percent = 1d - percent;
        }

        return Vertical
            ? new Point(ActualWidth / 2d, ActualHeight - ActualHeight * percent)
            : new Point(ActualWidth * percent, ActualHeight / 2d);
    }

    private void UpdateIndicators()
    {
        UpdateIndicator(startIndicatorPart, startIndicatorContentPart, GetStartValue(), 0);
        UpdateIndicator(endIndicatorPart, endIndicatorContentPart, GetEndValue(), 1);
    }

    private void UpdateIndicator(Border? indicator, ContentPresenter? contentPresenter, double value, int index)
    {
        if (indicator is null || contentPresenter is null)
        {
            return;
        }

        var shouldShow = index == 0 ? ShouldShowTooltip(0) : Range && ShouldShowTooltip(1);
        indicator.Visibility = shouldShow ? Visibility.Visible : Visibility.Collapsed;
        if (!shouldShow)
        {
            return;
        }

        contentPresenter.Content = value;
        contentPresenter.ContentTemplate = IndicatorTemplate;
        contentPresenter.ContentStringFormat = null;
        if (IndicatorTemplate is null)
        {
            contentPresenter.Content = FormatTooltipValue(value);
        }

        indicator.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var size = indicator.DesiredSize;
        var center = GetHandleCenter(value);

        var left = Placement switch
        {
            NSliderTooltipPlacement.Left => center.X - HandleSize / 2d - size.Width - 8d,
            NSliderTooltipPlacement.Right => center.X + HandleSize / 2d + 8d,
            _ => center.X - size.Width / 2d
        };

        var top = Placement switch
        {
            NSliderTooltipPlacement.Bottom => center.Y + HandleSize / 2d + 8d,
            NSliderTooltipPlacement.Left or NSliderTooltipPlacement.Right => center.Y - size.Height / 2d,
            _ => center.Y - HandleSize / 2d - size.Height - 8d
        };

        Canvas.SetLeft(indicator, left);
        Canvas.SetTop(indicator, top);
        Panel.SetZIndex(indicator, index == activeThumbIndex ? 80 : 70);
    }

    private void UpdateTooltipVisibility()
    {
        UpdateIndicators();
    }

    private bool ShouldShowTooltip(int index)
    {
        if (!Tooltip)
        {
            return false;
        }

        if (ShowTooltip.HasValue)
        {
            return ShowTooltip.Value;
        }

        return index == 0
            ? isHoveringStart || isDragging && activeThumbIndex == 0
            : isHoveringEnd || isDragging && activeThumbIndex == 1;
    }

    private string FormatTooltipValue(double value)
    {
        var content = FormatTooltip?.Invoke(value)
            ?? (!string.IsNullOrWhiteSpace(FormatString)
                ? value.ToString(FormatString, CultureInfo.CurrentCulture)
                : value.ToString(CultureInfo.CurrentCulture));
        return $"{TooltipPrefix}{content}{TooltipSuffix}";
    }

    private void RebuildMarks()
    {
        if (railCanvasPart is null)
        {
            return;
        }

        foreach (var element in markElements)
        {
            railCanvasPart.Children.Remove(element);
        }

        markElements.Clear();

        foreach (var mark in Marks)
        {
            AddMarkDot(mark);
            AddMarkLabel(mark);
        }
    }

    private void AddMarkDot(NSliderMark mark)
    {
        if (railCanvasPart is null)
        {
            return;
        }

        var active = IsMarkActive(mark.Value);
        var dot = MarkDotTemplate is null
            ? CreateDefaultMarkDot(active)
            : new ContentPresenter
            {
                Content = mark,
                ContentTemplate = MarkDotTemplate,
                IsHitTestVisible = false
            };

        dot.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var width = GetElementWidth(dot, DotWidth);
        var height = GetElementHeight(dot, DotHeight);
        var center = GetHandleCenter(mark.Value);
        Canvas.SetLeft(dot, center.X - width / 2d);
        Canvas.SetTop(dot, center.Y - height / 2d);
        Panel.SetZIndex(dot, 20);
        railCanvasPart.Children.Add(dot);
        markElements.Add(dot);
    }

    private FrameworkElement CreateDefaultMarkDot(bool active)
    {
        var disabled = Disabled || !IsEnabled;
        return new Border
        {
            Width = DotWidth,
            Height = DotHeight,
            Background = disabled
                ? DotDisabledBrush ?? GetBrush("Theme.Fill.2.Brush", Brushes.LightGray)
                : active ? DotActiveBrush ?? GetBrush("Primary.First.Brush", Brushes.ForestGreen) : DotBrush ?? GetBrush("Theme.Surface.0.Brush", Brushes.White),
            BorderBrush = disabled
                ? DotDisabledBorderBrush ?? GetBrush("Theme.Border.Strong.Brush", Brushes.LightGray)
                : active ? DotActiveBorderBrush ?? GetBrush("Primary.First.Brush", Brushes.ForestGreen) : DotBorderBrush ?? GetBrush("Theme.Border.Strong.Brush", Brushes.LightGray),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(DotBorderRadius),
            IsHitTestVisible = false
        };
    }

    private void AddMarkLabel(NSliderMark mark)
    {
        if (railCanvasPart is null || mark.Label is null)
        {
            return;
        }

        FrameworkElement label = new ContentPresenter
        {
            Content = mark,
            ContentTemplate = MarkTemplate,
            IsHitTestVisible = false
        };

        if (MarkTemplate is null)
        {
            label = new TextBlock
            {
                Text = mark.Label.ToString(),
                FontSize = MarkFontSize,
                Foreground = GetMarkForeground(),
                IsHitTestVisible = false
            };
        }

        label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        var center = GetHandleCenter(mark.Value);
        if (Vertical)
        {
            Canvas.SetLeft(label, center.X + HandleSize);
            Canvas.SetTop(label, center.Y - GetElementHeight(label, label.DesiredSize.Height) / 2d);
        }
        else
        {
            Canvas.SetLeft(label, center.X - GetElementWidth(label, label.DesiredSize.Width) / 2d);
            Canvas.SetTop(label, center.Y + HandleSize);
        }

        Panel.SetZIndex(label, 10);
        railCanvasPart.Children.Add(label);
        markElements.Add(label);
    }

    private Brush GetRailBackground()
    {
        if (Disabled || !IsEnabled)
        {
            return RailDisabledBrush ?? RailBrush ?? GetBrush("Theme.Fill.2.Brush", Brushes.LightGray);
        }

        return IsMouseOver ? RailHoverBrush ?? RailBrush ?? GetBrush("Theme.Fill.2Hover.Brush", Brushes.LightGray) : RailBrush ?? GetBrush("Theme.Fill.2.Brush", Brushes.LightGray);
    }

    private Brush GetFillBackground()
    {
        if (Disabled || !IsEnabled)
        {
            return FillDisabledBrush ?? FillBrush ?? GetBrush("Primary.First.Brush", Brushes.ForestGreen);
        }

        return IsMouseOver ? FillHoverBrush ?? FillBrush ?? GetBrush("Primary.Hover.Brush", Brushes.ForestGreen) : FillBrush ?? GetBrush("Primary.First.Brush", Brushes.ForestGreen);
    }

    private Brush GetMarkForeground()
    {
        if (Disabled || !IsEnabled)
        {
            return MarkDisabledForeground ?? GetBrush("Theme.Text.Tertiary.Brush", Brushes.Gray);
        }

        return MarkForeground ?? GetBrush("Theme.Text.Secondary.Brush", Brushes.Gray);
    }

    private static double GetElementWidth(FrameworkElement element, double fallback)
    {
        if (element.Width > 0 && !double.IsNaN(element.Width))
        {
            return element.Width;
        }

        return element.DesiredSize.Width > 0 ? element.DesiredSize.Width : fallback;
    }

    private static double GetElementHeight(FrameworkElement element, double fallback)
    {
        if (element.Height > 0 && !double.IsNaN(element.Height))
        {
            return element.Height;
        }

        return element.DesiredSize.Height > 0 ? element.DesiredSize.Height : fallback;
    }

    private bool IsMarkActive(double markValue)
    {
        if (Range)
        {
            var start = Math.Min(RangeStart, RangeEnd);
            var end = Math.Max(RangeStart, RangeEnd);
            return markValue >= start && markValue <= end;
        }

        return markValue <= Value;
    }

    private Brush GetBrush(string key, Brush fallback)
    {
        return TryFindResource(key) as Brush ?? fallback;
    }
}

internal static class SliderDoubleUtil
{
    internal static bool AreClose(double value1, double value2)
    {
        if (value1 == value2)
        {
            return true;
        }

        var tolerance = (Math.Abs(value1) + Math.Abs(value2) + 10d) * double.Epsilon;
        var difference = value1 - value2;
        return -tolerance < difference && tolerance > difference;
    }
}
