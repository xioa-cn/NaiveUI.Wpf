using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public enum NColorPickerMode
{
    Rgb,
    Hex,
    Hsl,
    Hsv
}

public enum NColorPickerAction
{
    Confirm
}

public enum NColorPickerTriggerStyle
{
    Button,
    Input,
    Custom
}

public class NColorPicker : Control
{
    private const string PopupPartName = "PART_Popup";
    private const string SaturationValueCanvasPartName = "PART_SaturationValueCanvas";
    private const string HueSliderPartName = "PART_HueSlider";
    private const string AlphaSliderPartName = "PART_AlphaSlider";
    private const string ValueTextBoxPartName = "PART_ValueTextBox";
    private const string ModeSelectorPartName = "PART_ModeSelector";
    private const string ClearButtonPartName = "PART_ClearButton";
    private const string ConfirmButtonPartName = "PART_ConfirmButton";
    private const string TriggerButtonPartName = "PART_TriggerButton";
    private static readonly NColorPickerMode[] DefaultModes =
    [
        NColorPickerMode.Rgb,
        NColorPickerMode.Hex,
        NColorPickerMode.Hsl
    ];
    private static NColorPicker? openInstance;

    private Popup? popupPart;
    private Canvas? saturationValueCanvasPart;
    private Slider? hueSliderPart;
    private Slider? alphaSliderPart;
    private TextBox? valueTextBoxPart;
    private NSelect? modeSelectorPart;
    private Button? clearButtonPart;
    private Button? confirmButtonPart;
    private Button? triggerButtonPart;
    private Window? ownerWindow;
    private bool syncingTemplate;
    private bool updatingValueInternally;
    private bool draggingSaturationValue;
    private double hue;
    private double saturation;
    private double brightness;
    private double alpha = 1d;

    static NColorPicker()
    {
        ElementBase.DefaultStyle<NColorPicker>(DefaultStyleKeyProperty);
    }

    public NColorPicker()
    {
        Modes.CollectionChanged += HandleModesCollectionChanged;
        Swatches.CollectionChanged += HandleSwatchesCollectionChanged;
        Actions.CollectionChanged += HandleActionsCollectionChanged;
        RebuildModeOptions();

        SetColorFromString(DefaultValue, raiseValueChanged: false);
        UpdateResolvedMetrics();
        UpdateVisualState();
        UpdateTemplateStateFromColor();
    }

    public ObservableCollection<NColorPickerMode> Modes { get; } = [];

    public ObservableCollection<NSelectOption> ModeOptions
    {
        get => (ObservableCollection<NSelectOption>)GetValue(ModeOptionsProperty);
        private set => SetValue(ModeOptionsPropertyKey, value);
    }

    private static readonly DependencyPropertyKey ModeOptionsPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ModeOptions), typeof(ObservableCollection<NSelectOption>), typeof(NColorPicker), new PropertyMetadata(null));

    public static readonly DependencyProperty ModeOptionsProperty = ModeOptionsPropertyKey.DependencyProperty;

    public ObservableCollection<string> Swatches { get; } = [];

    public ObservableCollection<NColorPickerAction> Actions { get; } = [];

    public string? Value
    {
        get => (string?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(string),
            typeof(NColorPicker),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public string DefaultValue
    {
        get => (string)GetValue(DefaultValueProperty);
        set => SetValue(DefaultValueProperty, value);
    }

    public static readonly DependencyProperty DefaultValueProperty =
        ElementBase.Property<NColorPicker, string>(nameof(DefaultValueProperty), "rgb(0, 0, 0)");

    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public static readonly DependencyProperty IsDropDownOpenProperty =
        DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(NColorPicker),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDropDownOpenChanged));

    public bool DefaultShow
    {
        get => (bool)GetValue(DefaultShowProperty);
        set => SetValue(DefaultShowProperty, value);
    }

    public static readonly DependencyProperty DefaultShowProperty =
        ElementBase.Property<NColorPicker, bool>(nameof(DefaultShowProperty), false, OnDefaultShowChanged);

    public bool ShowAlpha
    {
        get => (bool)GetValue(ShowAlphaProperty);
        set => SetValue(ShowAlphaProperty, value);
    }

    public static readonly DependencyProperty ShowAlphaProperty =
        ElementBase.Property<NColorPicker, bool>(nameof(ShowAlphaProperty), true, OnVisualPropertyChanged);

    public bool ShowPreview
    {
        get => (bool)GetValue(ShowPreviewProperty);
        set => SetValue(ShowPreviewProperty, value);
    }

    public static readonly DependencyProperty ShowPreviewProperty =
        ElementBase.Property<NColorPicker, bool>(nameof(ShowPreviewProperty), false, OnVisualPropertyChanged);

    public bool ShowValueText
    {
        get => (bool)GetValue(ShowValueTextProperty);
        set => SetValue(ShowValueTextProperty, value);
    }

    public static readonly DependencyProperty ShowValueTextProperty =
        ElementBase.Property<NColorPicker, bool>(nameof(ShowValueTextProperty), false, OnVisualPropertyChanged);

    public bool Clearable
    {
        get => (bool)GetValue(ClearableProperty);
        set => SetValue(ClearableProperty, value);
    }

    public static readonly DependencyProperty ClearableProperty =
        ElementBase.Property<NColorPicker, bool>(nameof(ClearableProperty), false, OnVisualPropertyChanged);

    public bool Disabled
    {
        get => (bool)GetValue(DisabledProperty);
        set => SetValue(DisabledProperty, value);
    }

    public static readonly DependencyProperty DisabledProperty =
        ElementBase.Property<NColorPicker, bool>(nameof(DisabledProperty), false, OnVisualPropertyChanged);

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public static readonly DependencyProperty IsReadOnlyProperty =
        ElementBase.Property<NColorPicker, bool>(nameof(IsReadOnlyProperty), false, OnVisualPropertyChanged);

    public bool ShowArrow
    {
        get => (bool)GetValue(ShowArrowProperty);
        set => SetValue(ShowArrowProperty, value);
    }

    public static readonly DependencyProperty ShowArrowProperty =
        ElementBase.Property<NColorPicker, bool>(nameof(ShowArrowProperty), true, OnVisualPropertyChanged);

    public NColorPickerTriggerStyle TriggerStyle
    {
        get => (NColorPickerTriggerStyle)GetValue(TriggerStyleProperty);
        set => SetValue(TriggerStyleProperty, value);
    }

    public static readonly DependencyProperty TriggerStyleProperty =
        ElementBase.Property<NColorPicker, NColorPickerTriggerStyle>(nameof(TriggerStyleProperty), NColorPickerTriggerStyle.Button, OnVisualPropertyChanged);

    public object? TriggerContent
    {
        get => GetValue(TriggerContentProperty);
        set => SetValue(TriggerContentProperty, value);
    }

    public static readonly DependencyProperty TriggerContentProperty =
        ElementBase.Property<NColorPicker, object?>(nameof(TriggerContentProperty), null, OnVisualPropertyChanged);

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public static readonly DependencyProperty PlaceholderProperty =
        ElementBase.Property<NColorPicker, string>(nameof(PlaceholderProperty), "请选择颜色");

    public NColorPickerMode Mode
    {
        get => (NColorPickerMode)GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public static readonly DependencyProperty ModeProperty =
        DependencyProperty.Register(
            nameof(Mode),
            typeof(NColorPickerMode),
            typeof(NColorPicker),
            new FrameworkPropertyMetadata(NColorPickerMode.Rgb, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnModeChanged));

    public NSelectPlacement Placement
    {
        get => (NSelectPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public static readonly DependencyProperty PlacementProperty =
        ElementBase.Property<NColorPicker, NSelectPlacement>(nameof(PlacementProperty), NSelectPlacement.BottomStart, OnPopupPropertyChanged);

    public NSelectSize Size
    {
        get => (NSelectSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public static readonly DependencyProperty SizeProperty =
        ElementBase.Property<NColorPicker, NSelectSize>(nameof(SizeProperty), NSelectSize.Medium, OnVisualPropertyChanged);

    public double PanelWidth
    {
        get => (double)GetValue(PanelWidthProperty);
        set => SetValue(PanelWidthProperty, value);
    }

    public static readonly DependencyProperty PanelWidthProperty =
        ElementBase.Property<NColorPicker, double>(nameof(PanelWidthProperty), 260d, OnPopupPropertyChanged);

    public double SaturationValueHeight
    {
        get => (double)GetValue(SaturationValueHeightProperty);
        set => SetValue(SaturationValueHeightProperty, value);
    }

    public static readonly DependencyProperty SaturationValueHeightProperty =
        ElementBase.Property<NColorPicker, double>(nameof(SaturationValueHeightProperty), 150d, OnVisualPropertyChanged);

    public string DisplayValue
    {
        get => (string)GetValue(DisplayValueProperty);
        private set => SetValue(DisplayValuePropertyKey, value);
    }

    private static readonly DependencyPropertyKey DisplayValuePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(DisplayValue), typeof(string), typeof(NColorPicker), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty DisplayValueProperty = DisplayValuePropertyKey.DependencyProperty;

    public Brush SelectedBrush
    {
        get => (Brush)GetValue(SelectedBrushProperty);
        private set => SetValue(SelectedBrushPropertyKey, value);
    }

    private static readonly DependencyPropertyKey SelectedBrushPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(SelectedBrush), typeof(Brush), typeof(NColorPicker), new PropertyMetadata(Brushes.Transparent));

    public static readonly DependencyProperty SelectedBrushProperty = SelectedBrushPropertyKey.DependencyProperty;

    public Brush HueColorBrush
    {
        get => (Brush)GetValue(HueColorBrushProperty);
        private set => SetValue(HueColorBrushPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HueColorBrushPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HueColorBrush), typeof(Brush), typeof(NColorPicker), new PropertyMetadata(Brushes.Red));

    public static readonly DependencyProperty HueColorBrushProperty = HueColorBrushPropertyKey.DependencyProperty;

    public Brush AlphaTrackBrush
    {
        get => (Brush)GetValue(AlphaTrackBrushProperty);
        private set => SetValue(AlphaTrackBrushPropertyKey, value);
    }

    private static readonly DependencyPropertyKey AlphaTrackBrushPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(AlphaTrackBrush), typeof(Brush), typeof(NColorPicker), new PropertyMetadata(Brushes.Transparent));

    public static readonly DependencyProperty AlphaTrackBrushProperty = AlphaTrackBrushPropertyKey.DependencyProperty;

    public double SaturationValueThumbX
    {
        get => (double)GetValue(SaturationValueThumbXProperty);
        private set => SetValue(SaturationValueThumbXPropertyKey, value);
    }

    private static readonly DependencyPropertyKey SaturationValueThumbXPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(SaturationValueThumbX), typeof(double), typeof(NColorPicker), new PropertyMetadata(0d));

    public static readonly DependencyProperty SaturationValueThumbXProperty = SaturationValueThumbXPropertyKey.DependencyProperty;

    public double SaturationValueThumbY
    {
        get => (double)GetValue(SaturationValueThumbYProperty);
        private set => SetValue(SaturationValueThumbYPropertyKey, value);
    }

    private static readonly DependencyPropertyKey SaturationValueThumbYPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(SaturationValueThumbY), typeof(double), typeof(NColorPicker), new PropertyMetadata(0d));

    public static readonly DependencyProperty SaturationValueThumbYProperty = SaturationValueThumbYPropertyKey.DependencyProperty;

    public double ResolvedHeight
    {
        get => (double)GetValue(ResolvedHeightProperty);
        private set => SetValue(ResolvedHeightPropertyKey, value);
    }

    private static readonly DependencyPropertyKey ResolvedHeightPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ResolvedHeight), typeof(double), typeof(NColorPicker), new PropertyMetadata(34d));

    public static readonly DependencyProperty ResolvedHeightProperty = ResolvedHeightPropertyKey.DependencyProperty;

    public double ResolvedFontSize
    {
        get => (double)GetValue(ResolvedFontSizeProperty);
        private set => SetValue(ResolvedFontSizePropertyKey, value);
    }

    private static readonly DependencyPropertyKey ResolvedFontSizePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ResolvedFontSize), typeof(double), typeof(NColorPicker), new PropertyMetadata(14d));

    public static readonly DependencyProperty ResolvedFontSizeProperty = ResolvedFontSizePropertyKey.DependencyProperty;

    public Thickness ResolvedPadding
    {
        get => (Thickness)GetValue(ResolvedPaddingProperty);
        private set => SetValue(ResolvedPaddingPropertyKey, value);
    }

    private static readonly DependencyPropertyKey ResolvedPaddingPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ResolvedPadding), typeof(Thickness), typeof(NColorPicker), new PropertyMetadata(new Thickness(12, 0, 10, 0)));

    public static readonly DependencyProperty ResolvedPaddingProperty = ResolvedPaddingPropertyKey.DependencyProperty;

    public bool HasValue
    {
        get => (bool)GetValue(HasValueProperty);
        private set => SetValue(HasValuePropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasValuePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasValue), typeof(bool), typeof(NColorPicker), new PropertyMetadata(false));

    public static readonly DependencyProperty HasValueProperty = HasValuePropertyKey.DependencyProperty;

    public bool HasTriggerContent
    {
        get => (bool)GetValue(HasTriggerContentProperty);
        private set => SetValue(HasTriggerContentPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasTriggerContentPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasTriggerContent), typeof(bool), typeof(NColorPicker), new PropertyMetadata(false));

    public static readonly DependencyProperty HasTriggerContentProperty = HasTriggerContentPropertyKey.DependencyProperty;

    public bool HasActions
    {
        get => (bool)GetValue(HasActionsProperty);
        private set => SetValue(HasActionsPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasActionsPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasActions), typeof(bool), typeof(NColorPicker), new PropertyMetadata(false));

    public static readonly DependencyProperty HasActionsProperty = HasActionsPropertyKey.DependencyProperty;

    public bool HasSwatches
    {
        get => (bool)GetValue(HasSwatchesProperty);
        private set => SetValue(HasSwatchesPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasSwatchesPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasSwatches), typeof(bool), typeof(NColorPicker), new PropertyMetadata(false));

    public static readonly DependencyProperty HasSwatchesProperty = HasSwatchesPropertyKey.DependencyProperty;

    public bool IsInputTrigger
    {
        get => (bool)GetValue(IsInputTriggerProperty);
        private set => SetValue(IsInputTriggerPropertyKey, value);
    }

    private static readonly DependencyPropertyKey IsInputTriggerPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsInputTrigger), typeof(bool), typeof(NColorPicker), new PropertyMetadata(false));

    public static readonly DependencyProperty IsInputTriggerProperty = IsInputTriggerPropertyKey.DependencyProperty;

    public bool IsCustomTrigger
    {
        get => (bool)GetValue(IsCustomTriggerProperty);
        private set => SetValue(IsCustomTriggerPropertyKey, value);
    }

    private static readonly DependencyPropertyKey IsCustomTriggerPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsCustomTrigger), typeof(bool), typeof(NColorPicker), new PropertyMetadata(false));

    public static readonly DependencyProperty IsCustomTriggerProperty = IsCustomTriggerPropertyKey.DependencyProperty;

    public bool ShouldShowValueText
    {
        get => (bool)GetValue(ShouldShowValueTextProperty);
        private set => SetValue(ShouldShowValueTextPropertyKey, value);
    }

    private static readonly DependencyPropertyKey ShouldShowValueTextPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ShouldShowValueText), typeof(bool), typeof(NColorPicker), new PropertyMetadata(false));

    public static readonly DependencyProperty ShouldShowValueTextProperty = ShouldShowValueTextPropertyKey.DependencyProperty;

    public PlacementMode ResolvedPopupPlacement
    {
        get => (PlacementMode)GetValue(ResolvedPopupPlacementProperty);
        private set => SetValue(ResolvedPopupPlacementPropertyKey, value);
    }

    private static readonly DependencyPropertyKey ResolvedPopupPlacementPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ResolvedPopupPlacement), typeof(PlacementMode), typeof(NColorPicker), new PropertyMetadata(PlacementMode.Custom));

    public static readonly DependencyProperty ResolvedPopupPlacementProperty = ResolvedPopupPlacementPropertyKey.DependencyProperty;

    public static readonly RoutedEvent ValueChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(ValueChanged), RoutingStrategy.Bubble, typeof(EventHandler<NColorPickerValueChangedEventArgs>), typeof(NColorPicker));

    public static readonly RoutedEvent DropDownOpenChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(DropDownOpenChanged), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<bool>), typeof(NColorPicker));

    public static readonly RoutedEvent CompleteEvent =
        EventManager.RegisterRoutedEvent(nameof(Complete), RoutingStrategy.Bubble, typeof(EventHandler<NColorPickerCompleteEventArgs>), typeof(NColorPicker));

    public static readonly RoutedEvent ClearEvent =
        ElementBase.RoutedEvent<NColorPicker, RoutedEventHandler>(nameof(ClearEvent));

    public static readonly RoutedUICommand ClearCommand =
        ElementBase.Command<NColorPicker>(nameof(ClearCommand));

    public static readonly RoutedUICommand ConfirmCommand =
        ElementBase.Command<NColorPicker>(nameof(ConfirmCommand));

    public static readonly RoutedUICommand SelectSwatchCommand =
        ElementBase.Command<NColorPicker>(nameof(SelectSwatchCommand));

    public ICommand? ConfirmButtonCommand
    {
        get => (ICommand?)GetValue(ConfirmButtonCommandProperty);
        set => SetValue(ConfirmButtonCommandProperty, value);
    }

    public static readonly DependencyProperty ConfirmButtonCommandProperty =
        ElementBase.Property<NColorPicker, ICommand?>(nameof(ConfirmButtonCommandProperty), ConfirmCommand);

    public object? ConfirmButtonCommandParameter
    {
        get => GetValue(ConfirmButtonCommandParameterProperty);
        set => SetValue(ConfirmButtonCommandParameterProperty, value);
    }

    public static readonly DependencyProperty ConfirmButtonCommandParameterProperty =
        ElementBase.Property<NColorPicker, object?>(nameof(ConfirmButtonCommandParameterProperty), null);

    public static readonly RoutedEvent ConfirmButtonClickEvent =
        ElementBase.RoutedEvent<NColorPicker, RoutedEventHandler>(nameof(ConfirmButtonClickEvent));

    public event EventHandler<NColorPickerValueChangedEventArgs> ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    public event RoutedPropertyChangedEventHandler<bool> DropDownOpenChanged
    {
        add => AddHandler(DropDownOpenChangedEvent, value);
        remove => RemoveHandler(DropDownOpenChangedEvent, value);
    }

    public event EventHandler<NColorPickerCompleteEventArgs> Complete
    {
        add => AddHandler(CompleteEvent, value);
        remove => RemoveHandler(CompleteEvent, value);
    }

    public event RoutedEventHandler ConfirmButtonClick
    {
        add => AddHandler(ConfirmButtonClickEvent, value);
        remove => RemoveHandler(ConfirmButtonClickEvent, value);
    }

    public event RoutedEventHandler Clear
    {
        add => AddHandler(ClearEvent, value);
        remove => RemoveHandler(ClearEvent, value);
    }

    public override void OnApplyTemplate()
    {
        DetachTemplatePartEvents();
        base.OnApplyTemplate();

        popupPart = GetTemplateChild(PopupPartName) as Popup;
        saturationValueCanvasPart = GetTemplateChild(SaturationValueCanvasPartName) as Canvas;
        hueSliderPart = GetTemplateChild(HueSliderPartName) as Slider;
        alphaSliderPart = GetTemplateChild(AlphaSliderPartName) as Slider;
        valueTextBoxPart = GetTemplateChild(ValueTextBoxPartName) as TextBox;
        modeSelectorPart = GetTemplateChild(ModeSelectorPartName) as NSelect;
        clearButtonPart = GetTemplateChild(ClearButtonPartName) as Button;
        confirmButtonPart = GetTemplateChild(ConfirmButtonPartName) as Button;
        triggerButtonPart = GetTemplateChild(TriggerButtonPartName) as Button;

        AttachTemplatePartEvents();
        UpdatePopupMetrics();
        UpdateTemplateStateFromColor();
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;
        CommandBindings.Add(new CommandBinding(ClearCommand, ExecuteClearCommand, CanExecuteClearCommand));
        CommandBindings.Add(new CommandBinding(ConfirmCommand, ExecuteConfirmCommand, CanExecuteConfirmCommand));
        CommandBindings.Add(new CommandBinding(SelectSwatchCommand, ExecuteSelectSwatchCommand, CanExecuteSelectSwatchCommand));
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (Disabled || IsReadOnly)
        {
            return;
        }

        if (e.Key == Key.Escape && IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
            return;
        }

        if (e.Key is Key.Enter or Key.Space && !IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, true);
            e.Handled = true;
        }
    }

    public void ClearSelection()
    {
        if (Disabled || IsReadOnly)
        {
            return;
        }

        var oldValue = Value;
        updatingValueInternally = true;
        try
        {
            SetCurrentValue(ValueProperty, null);
        }
        finally
        {
            updatingValueInternally = false;
        }

        HasValue = false;
        DisplayValue = string.Empty;
        SelectedBrush = Brushes.Transparent;
        valueTextBoxPart?.SetCurrentValue(TextBox.TextProperty, string.Empty);
        RaiseEvent(new RoutedEventArgs(ClearEvent, this));
        RaiseEvent(new NColorPickerValueChangedEventArgs(ValueChangedEvent, this, oldValue, null));
    }

    public void Confirm()
    {
        RaiseComplete();
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NColorPicker colorPicker)
        {
            return;
        }

        var oldValue = e.OldValue as string;
        var newValue = e.NewValue as string;

        if (!colorPicker.updatingValueInternally)
        {
            colorPicker.SetColorFromString(newValue, raiseValueChanged: false);
        }

        colorPicker.UpdateTemplateStateFromColor();
        colorPicker.RaiseEvent(new NColorPickerValueChangedEventArgs(ValueChangedEvent, colorPicker, oldValue, newValue));
    }

    private static void OnDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NColorPicker colorPicker)
        {
            return;
        }

        var nextValue = (bool)e.NewValue;
        if (nextValue)
        {
            if (colorPicker.Disabled || colorPicker.IsReadOnly)
            {
                colorPicker.SetCurrentValue(IsDropDownOpenProperty, false);
                return;
            }

            if (openInstance is not null && !ReferenceEquals(openInstance, colorPicker))
            {
                openInstance.SetCurrentValue(IsDropDownOpenProperty, false);
            }

            openInstance = colorPicker;
            colorPicker.UpdatePopupMetrics();
        }
        else if (ReferenceEquals(openInstance, colorPicker))
        {
            openInstance = null;
        }

        colorPicker.RaiseEvent(new RoutedPropertyChangedEventArgs<bool>((bool)e.OldValue, nextValue, DropDownOpenChangedEvent));
    }

    private static void OnDefaultShowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NColorPicker colorPicker && (bool)e.NewValue)
        {
            colorPicker.SetCurrentValue(IsDropDownOpenProperty, true);
        }
    }

    private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NColorPicker colorPicker)
        {
            return;
        }

        if (colorPicker.Modes.Count > 0)
        {
            colorPicker.EnsureModeAllowed();
            colorPicker.CommitCurrentColor(raiseComplete: false);
        }
        else
        {
            colorPicker.UpdateTemplateStateFromColor();
        }
    }

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NColorPicker colorPicker)
        {
            return;
        }

        if (e.Property == ShowAlphaProperty && (bool)e.NewValue == false)
        {
            colorPicker.alpha = 1d;
            colorPicker.CommitCurrentColor(raiseComplete: false);
        }

        colorPicker.UpdateResolvedMetrics();
        colorPicker.UpdateVisualState();
        colorPicker.UpdateTemplateStateFromColor();
        if (colorPicker.Disabled || colorPicker.IsReadOnly)
        {
            colorPicker.SetCurrentValue(IsDropDownOpenProperty, false);
        }
    }

    private static void OnPopupPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NColorPicker colorPicker)
        {
            colorPicker.UpdatePopupMetrics();
        }
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        EnsureModeAllowed();
        UpdateTemplateStateFromColor();

        ownerWindow = Window.GetWindow(this);
        if (ownerWindow is not null)
        {
            ownerWindow.Deactivated += HandleOwnerWindowDeactivated;
        }
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        if (ownerWindow is not null)
        {
            ownerWindow.Deactivated -= HandleOwnerWindowDeactivated;
            ownerWindow = null;
        }
    }

    private void HandleOwnerWindowDeactivated(object? sender, EventArgs e)
    {
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private void ExecuteClearCommand(object sender, ExecutedRoutedEventArgs e)
    {
        ClearSelection();
        e.Handled = true;
    }

    private void CanExecuteClearCommand(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = Clearable && HasValue && !Disabled && !IsReadOnly;
        e.Handled = true;
    }

    private void ExecuteConfirmCommand(object sender, ExecutedRoutedEventArgs e)
    {
        Confirm();
        e.Handled = true;
    }

    private void CanExecuteConfirmCommand(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = !Disabled && !IsReadOnly && HasValue;
        e.Handled = true;
    }

    private void ExecuteSelectSwatchCommand(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Parameter is string value && TryParseColor(value, out var parsed))
        {
            ApplyColor(parsed, preserveMode: true);
            CommitCurrentColor(raiseComplete: true);
        }

        e.Handled = true;
    }

    private void CanExecuteSelectSwatchCommand(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = !Disabled && !IsReadOnly && e.Parameter is string;
        e.Handled = true;
    }

    private void HandleModesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildModeOptions();
        UpdateVisualState();
    }

    private void HandleSwatchesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateVisualState();
    }

    private void HandleActionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateVisualState();
    }

    private void AttachTemplatePartEvents()
    {
        if (triggerButtonPart is not null)
        {
            triggerButtonPart.Click += HandleTriggerButtonClick;
        }

        if (saturationValueCanvasPart is not null)
        {
            saturationValueCanvasPart.MouseLeftButtonDown += HandleSaturationValueMouseLeftButtonDown;
            saturationValueCanvasPart.MouseMove += HandleSaturationValueMouseMove;
            saturationValueCanvasPart.MouseLeftButtonUp += HandleSaturationValueMouseLeftButtonUp;
            saturationValueCanvasPart.SizeChanged += HandleSaturationValueSizeChanged;
        }

        if (hueSliderPart is not null)
        {
            hueSliderPart.ValueChanged += HandleHueSliderValueChanged;
        }

        if (alphaSliderPart is not null)
        {
            alphaSliderPart.ValueChanged += HandleAlphaSliderValueChanged;
        }

        if (valueTextBoxPart is not null)
        {
            valueTextBoxPart.LostKeyboardFocus += HandleValueTextBoxLostKeyboardFocus;
            valueTextBoxPart.KeyDown += HandleValueTextBoxKeyDown;
        }

        if (modeSelectorPart is not null)
        {
            modeSelectorPart.SelectionChanged += HandleModeSelectorSelectionChanged;
        }

        if (clearButtonPart is not null)
        {
            clearButtonPart.Click += HandleClearButtonClick;
        }

        if (confirmButtonPart is not null)
        {
            confirmButtonPart.Click += HandleConfirmButtonClick;
        }
    }

    private void DetachTemplatePartEvents()
    {
        if (triggerButtonPart is not null)
        {
            triggerButtonPart.Click -= HandleTriggerButtonClick;
        }

        if (saturationValueCanvasPart is not null)
        {
            saturationValueCanvasPart.MouseLeftButtonDown -= HandleSaturationValueMouseLeftButtonDown;
            saturationValueCanvasPart.MouseMove -= HandleSaturationValueMouseMove;
            saturationValueCanvasPart.MouseLeftButtonUp -= HandleSaturationValueMouseLeftButtonUp;
            saturationValueCanvasPart.SizeChanged -= HandleSaturationValueSizeChanged;
        }

        if (hueSliderPart is not null)
        {
            hueSliderPart.ValueChanged -= HandleHueSliderValueChanged;
        }

        if (alphaSliderPart is not null)
        {
            alphaSliderPart.ValueChanged -= HandleAlphaSliderValueChanged;
        }

        if (valueTextBoxPart is not null)
        {
            valueTextBoxPart.LostKeyboardFocus -= HandleValueTextBoxLostKeyboardFocus;
            valueTextBoxPart.KeyDown -= HandleValueTextBoxKeyDown;
        }

        if (modeSelectorPart is not null)
        {
            modeSelectorPart.SelectionChanged -= HandleModeSelectorSelectionChanged;
        }

        if (clearButtonPart is not null)
        {
            clearButtonPart.Click -= HandleClearButtonClick;
        }

        if (confirmButtonPart is not null)
        {
            confirmButtonPart.Click -= HandleConfirmButtonClick;
        }
    }

    private void HandleTriggerButtonClick(object sender, RoutedEventArgs e)
    {
        if (Disabled || IsReadOnly)
        {
            return;
        }

        SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
        e.Handled = true;
    }

    private void HandleSaturationValueMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (Disabled || IsReadOnly || saturationValueCanvasPart is null)
        {
            return;
        }

        draggingSaturationValue = true;
        saturationValueCanvasPart.CaptureMouse();
        UpdateSaturationValueFromPoint(e.GetPosition(saturationValueCanvasPart), commit: true);
        e.Handled = true;
    }

    private void HandleSaturationValueMouseMove(object sender, MouseEventArgs e)
    {
        if (!draggingSaturationValue || saturationValueCanvasPart is null)
        {
            return;
        }

        UpdateSaturationValueFromPoint(e.GetPosition(saturationValueCanvasPart), commit: true);
        e.Handled = true;
    }

    private void HandleSaturationValueMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!draggingSaturationValue)
        {
            return;
        }

        draggingSaturationValue = false;
        saturationValueCanvasPart?.ReleaseMouseCapture();
        RaiseCompleteIfImmediate();
        e.Handled = true;
    }

    private void HandleSaturationValueSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateThumbPosition();
    }

    private void HandleHueSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (syncingTemplate || Disabled || IsReadOnly)
        {
            return;
        }

        hue = Clamp(e.NewValue, 0d, 360d);
        CommitCurrentColor(raiseComplete: false);
    }

    private void HandleAlphaSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (syncingTemplate || Disabled || IsReadOnly)
        {
            return;
        }

        alpha = Clamp(e.NewValue / 100d, 0d, 1d);
        CommitCurrentColor(raiseComplete: false);
    }

    private void HandleValueTextBoxLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        CommitTextBoxValue();
    }

    private void HandleValueTextBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            CommitTextBoxValue();
            e.Handled = true;
        }
    }

    private void HandleModeSelectorSelectionChanged(object? sender, NSelectSelectionChangedEventArgs e)
    {
        if (syncingTemplate || e.NewValue is not NColorPickerMode nextMode)
        {
            return;
        }

        SetCurrentValue(ModeProperty, nextMode);
    }

    private void HandleClearButtonClick(object sender, RoutedEventArgs e)
    {
        ClearSelection();
        e.Handled = true;
    }

    private void HandleConfirmButtonClick(object sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(ConfirmButtonClickEvent, this));
    }

    private void CommitTextBoxValue()
    {
        if (valueTextBoxPart is null || Disabled || IsReadOnly)
        {
            return;
        }

        var text = valueTextBoxPart.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            if (Clearable)
            {
                ClearSelection();
            }

            return;
        }

        if (TryParseColor(text, out var parsed))
        {
            ApplyColor(parsed, preserveMode: true);
            CommitCurrentColor(raiseComplete: true);
            return;
        }

        UpdateTemplateStateFromColor();
    }

    private void UpdateSaturationValueFromPoint(Point point, bool commit)
    {
        var width = Math.Max(1d, saturationValueCanvasPart?.ActualWidth ?? 1d);
        var height = Math.Max(1d, saturationValueCanvasPart?.ActualHeight ?? 1d);
        saturation = Clamp(point.X / width, 0d, 1d);
        brightness = 1d - Clamp(point.Y / height, 0d, 1d);
        if (commit)
        {
            CommitCurrentColor(raiseComplete: false);
        }
        else
        {
            UpdateTemplateStateFromColor();
        }
    }

    private void SetColorFromString(string? value, bool raiseValueChanged)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            HasValue = false;
            DisplayValue = string.Empty;
            SelectedBrush = Brushes.Transparent;
            return;
        }

        if (!TryParseColor(value, out var parsed))
        {
            if (!HasValue)
            {
                ApplyColor(ParseColor("#000000"), preserveMode: true);
            }

            return;
        }

        ApplyColor(parsed, preserveMode: true);
        if (raiseValueChanged)
        {
            CommitCurrentColor(raiseComplete: false);
        }
    }

    private void ApplyColor(ColorState color, bool preserveMode)
    {
        hue = color.Hue;
        saturation = color.Saturation;
        brightness = color.Value;
        alpha = ShowAlpha ? color.Alpha : 1d;
        if (!preserveMode)
        {
            SetCurrentValue(ModeProperty, color.Mode);
        }
    }

    private void CommitCurrentColor(bool raiseComplete)
    {
        EnsureModeAllowed();
        var nextValue = FormatCurrentColor();
        var oldValue = Value;

        updatingValueInternally = true;
        try
        {
            SetCurrentValue(ValueProperty, nextValue);
        }
        finally
        {
            updatingValueInternally = false;
        }

        UpdateTemplateStateFromColor();

        if (!string.Equals(oldValue, nextValue, StringComparison.Ordinal))
        {
            RaiseEvent(new NColorPickerValueChangedEventArgs(ValueChangedEvent, this, oldValue, nextValue));
        }

        if (raiseComplete)
        {
            RaiseComplete();
        }
    }

    private void RaiseCompleteIfImmediate()
    {
        if (!HasActions)
        {
            RaiseComplete();
        }
    }

    private void RaiseComplete()
    {
        if (!string.IsNullOrWhiteSpace(Value))
        {
            RaiseEvent(new NColorPickerCompleteEventArgs(CompleteEvent, this, Value));
        }
    }

    private void EnsureModeAllowed()
    {
        var modes = GetEffectiveModes();

        if (!modes.Contains(Mode))
        {
            SetCurrentValue(ModeProperty, modes[0]);
        }
    }

    private string FormatCurrentColor()
    {
        var color = ToRgb(NormalizeHue(hue), saturation, brightness, alpha);
        return Mode switch
        {
            NColorPickerMode.Hex => FormatHex(color, ShowAlpha),
            NColorPickerMode.Hsl => FormatHsl(color, ShowAlpha),
            NColorPickerMode.Hsv => FormatHsv(NormalizeHue(hue), saturation, brightness, alpha, ShowAlpha),
            _ => FormatRgb(color, ShowAlpha)
        };
    }

    private void UpdateTemplateStateFromColor()
    {
        syncingTemplate = true;
        try
        {
            var hasValue = !string.IsNullOrWhiteSpace(Value);
            HasValue = hasValue;
            DisplayValue = hasValue ? FormatCurrentColor() : string.Empty;
            var color = ToRgb(NormalizeHue(hue), saturation, brightness, ShowAlpha ? alpha : 1d);
            SelectedBrush = CreateFrozenBrush(color);
            HueColorBrush = CreateFrozenBrush(ToRgb(NormalizeHue(hue), 1d, 1d, 1d));
            AlphaTrackBrush = CreateAlphaTrackBrush(color);
            UpdateThumbPosition();

            if (hueSliderPart is not null)
            {
                hueSliderPart.Value = hue;
            }

            if (alphaSliderPart is not null)
            {
                alphaSliderPart.Value = alpha * 100d;
            }

            if (valueTextBoxPart is not null)
            {
                valueTextBoxPart.Text = DisplayValue;
            }

            if (modeSelectorPart is not null)
            {
                modeSelectorPart.ItemsSource = ModeOptions;
                modeSelectorPart.SelectedValue = Mode;
            }
        }
        finally
        {
            syncingTemplate = false;
        }
    }

    private void UpdateThumbPosition()
    {
        var width = Math.Max(1d, saturationValueCanvasPart?.ActualWidth ?? Math.Max(1d, PanelWidth - 28d));
        var height = Math.Max(1d, saturationValueCanvasPart?.ActualHeight ?? SaturationValueHeight);
        SaturationValueThumbX = saturation * width - 6d;
        SaturationValueThumbY = (1d - brightness) * height - 6d;
    }

    private void UpdateResolvedMetrics()
    {
        var (height, fontSize, padding) = Size switch
        {
            NSelectSize.Tiny => (22d, 12d, new Thickness(4)),
            NSelectSize.Small => (28d, 13d, new Thickness(5)),
            NSelectSize.Large => (40d, 15d, new Thickness(7)),
            _ => (34d, 14d, new Thickness(6))
        };

        ResolvedHeight = height;
        ResolvedFontSize = fontSize;
        ResolvedPadding = padding;
    }

    private void UpdateVisualState()
    {
        HasTriggerContent = TriggerContent is not null;
        HasActions = Actions.Contains(NColorPickerAction.Confirm);
        HasSwatches = Swatches.Count > 0;
        IsInputTrigger = TriggerStyle == NColorPickerTriggerStyle.Input;
        IsCustomTrigger = TriggerStyle == NColorPickerTriggerStyle.Custom && TriggerContent is not null;
        ShouldShowValueText = HasValue && (ShowValueText || TriggerStyle == NColorPickerTriggerStyle.Input);
    }

    private void RebuildModeOptions()
    {
        var options = new ObservableCollection<NSelectOption>();
        foreach (var mode in GetEffectiveModes())
        {
            var option = new NSelectOption
            {
                Label = FormatModeLabel(mode),
                Value = mode
            };
            option.Source = option;
            options.Add(option);
        }

        ModeOptions = options;
    }

    private IReadOnlyList<NColorPickerMode> GetEffectiveModes()
    {
        return Modes.Count > 0 ? Modes : DefaultModes;
    }

    private void UpdatePopupMetrics()
    {
        ResolvedPopupPlacement = PlacementMode.Custom;
        if (popupPart is not null)
        {
            popupPart.CustomPopupPlacementCallback = PlacePopup;
        }
    }

    private CustomPopupPlacement[] PlacePopup(Size popupSize, Size targetSize, Point offset)
    {
        var gap = 6d;
        var x = Placement switch
        {
            NSelectPlacement.Bottom => (targetSize.Width - popupSize.Width) / 2d,
            NSelectPlacement.BottomEnd => targetSize.Width - popupSize.Width,
            NSelectPlacement.Top => (targetSize.Width - popupSize.Width) / 2d,
            NSelectPlacement.TopEnd => targetSize.Width - popupSize.Width,
            _ => 0d
        };

        var y = Placement switch
        {
            NSelectPlacement.Top or NSelectPlacement.TopStart or NSelectPlacement.TopEnd => -popupSize.Height - gap,
            _ => targetSize.Height + gap
        };

        return [new CustomPopupPlacement(new Point(x + offset.X, y + offset.Y), PopupPrimaryAxis.Vertical)];
    }

    private Brush CreateAlphaTrackBrush(Color color)
    {
        var transparent = Color.FromArgb(0, color.R, color.G, color.B);
        var opaque = Color.FromArgb(255, color.R, color.G, color.B);
        var brush = new LinearGradientBrush(transparent, opaque, 0d);
        brush.Freeze();
        return brush;
    }

    private static SolidColorBrush CreateFrozenBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private static bool TryParseColor(string? rawValue, out ColorState color)
    {
        color = default;
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return false;
        }

        try
        {
            color = ParseColor(rawValue);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static ColorState ParseColor(string rawValue)
    {
        var value = rawValue.Trim();
        if (value.StartsWith('#'))
        {
            return ParseHex(value);
        }

        if (value.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
        {
            return ParseRgb(value);
        }

        if (value.StartsWith("hsl", StringComparison.OrdinalIgnoreCase))
        {
            return ParseHsl(value);
        }

        if (value.StartsWith("hsv", StringComparison.OrdinalIgnoreCase))
        {
            return ParseHsv(value);
        }

        var converted = ColorConverter.ConvertFromString(value);
        if (converted is Color mediaColor)
        {
            return FromRgb(mediaColor, NColorPickerMode.Hex);
        }

        throw new FormatException("Unsupported color value.");
    }

    private static ColorState ParseHex(string value)
    {
        var hex = value.TrimStart('#');
        if (hex.Length == 3 || hex.Length == 4)
        {
            hex = string.Concat(hex.Select(ch => $"{ch}{ch}"));
        }

        if (hex.Length is not (6 or 8) || !Regex.IsMatch(hex, "^[0-9a-fA-F]+$"))
        {
            throw new FormatException("Invalid hex color.");
        }

        var r = byte.Parse(hex[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var a = hex.Length == 8 ? byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) : (byte)255;
        return FromRgb(Color.FromArgb(a, r, g, b), NColorPickerMode.Hex);
    }

    private static ColorState ParseRgb(string value)
    {
        var parts = ExtractFunctionParts(value);
        if (parts.Length < 3)
        {
            throw new FormatException("Invalid rgb color.");
        }

        var r = ParseByte(parts[0]);
        var g = ParseByte(parts[1]);
        var b = ParseByte(parts[2]);
        var a = parts.Length > 3 ? ParseAlpha(parts[3]) : 1d;
        return FromRgb(ToColor(r, g, b, a), NColorPickerMode.Rgb);
    }

    private static ColorState ParseHsl(string value)
    {
        var parts = ExtractFunctionParts(value);
        if (parts.Length < 3)
        {
            throw new FormatException("Invalid hsl color.");
        }

        var h = ParseDouble(parts[0]);
        var s = ParsePercent(parts[1]);
        var l = ParsePercent(parts[2]);
        var a = parts.Length > 3 ? ParseAlpha(parts[3]) : 1d;
        var rgb = HslToRgb(h, s, l, a);
        return FromRgb(rgb, NColorPickerMode.Hsl);
    }

    private static ColorState ParseHsv(string value)
    {
        var parts = ExtractFunctionParts(value);
        if (parts.Length < 3)
        {
            throw new FormatException("Invalid hsv color.");
        }

        var h = NormalizeHue(ParseDouble(parts[0]));
        var s = ParsePercent(parts[1]);
        var v = ParsePercent(parts[2]);
        var a = parts.Length > 3 ? ParseAlpha(parts[3]) : 1d;
        return new ColorState(h, s, v, a, NColorPickerMode.Hsv);
    }

    private static string[] ExtractFunctionParts(string value)
    {
        var start = value.IndexOf('(');
        var end = value.LastIndexOf(')');
        if (start < 0 || end <= start)
        {
            throw new FormatException("Invalid color function.");
        }

        return value[(start + 1)..end]
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }

    private static byte ParseByte(string value)
    {
        if (value.EndsWith('%'))
        {
            return (byte)Math.Round(Clamp(ParsePercent(value) * 255d, 0d, 255d));
        }

        return (byte)Math.Round(Clamp(ParseDouble(value), 0d, 255d));
    }

    private static double ParseAlpha(string value)
    {
        return value.Trim().EndsWith('%') ? ParsePercent(value) : Clamp(ParseDouble(value), 0d, 1d);
    }

    private static double ParsePercent(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.EndsWith('%'))
        {
            trimmed = trimmed[..^1];
        }

        return Clamp(ParseDouble(trimmed) / 100d, 0d, 1d);
    }

    private static double ParseDouble(string value)
    {
        return double.Parse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);
    }

    private static ColorState FromRgb(Color color, NColorPickerMode mode)
    {
        RgbToHsv(color, out var h, out var s, out var v);
        return new ColorState(h, s, v, color.A / 255d, mode);
    }

    private static Color ToRgb(double h, double s, double v, double a)
    {
        h = NormalizeHue(h);
        s = Clamp(s, 0d, 1d);
        v = Clamp(v, 0d, 1d);
        var c = v * s;
        var x = c * (1d - Math.Abs(h / 60d % 2d - 1d));
        var m = v - c;

        var (rp, gp, bp) = h switch
        {
            < 60d => (c, x, 0d),
            < 120d => (x, c, 0d),
            < 180d => (0d, c, x),
            < 240d => (0d, x, c),
            < 300d => (x, 0d, c),
            _ => (c, 0d, x)
        };

        return ToColor((rp + m) * 255d, (gp + m) * 255d, (bp + m) * 255d, a);
    }

    private static Color ToColor(double r, double g, double b, double a)
    {
        return Color.FromArgb(
            (byte)Math.Round(Clamp(a, 0d, 1d) * 255d),
            (byte)Math.Round(Clamp(r, 0d, 255d)),
            (byte)Math.Round(Clamp(g, 0d, 255d)),
            (byte)Math.Round(Clamp(b, 0d, 255d)));
    }

    private static Color HslToRgb(double h, double s, double l, double a)
    {
        h = NormalizeHue(h);
        s = Clamp(s, 0d, 1d);
        l = Clamp(l, 0d, 1d);
        var c = (1d - Math.Abs(2d * l - 1d)) * s;
        var x = c * (1d - Math.Abs(h / 60d % 2d - 1d));
        var m = l - c / 2d;

        var (rp, gp, bp) = h switch
        {
            < 60d => (c, x, 0d),
            < 120d => (x, c, 0d),
            < 180d => (0d, c, x),
            < 240d => (0d, x, c),
            < 300d => (x, 0d, c),
            _ => (c, 0d, x)
        };

        return ToColor((rp + m) * 255d, (gp + m) * 255d, (bp + m) * 255d, a);
    }

    private static void RgbToHsv(Color color, out double h, out double s, out double v)
    {
        var r = color.R / 255d;
        var g = color.G / 255d;
        var b = color.B / 255d;
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;

        h = delta == 0d
            ? 0d
            : max == r
                ? 60d * (((g - b) / delta) % 6d)
                : max == g
                    ? 60d * ((b - r) / delta + 2d)
                    : 60d * ((r - g) / delta + 4d);
        if (h < 0d)
        {
            h += 360d;
        }

        s = max == 0d ? 0d : delta / max;
        v = max;
    }

    private static void RgbToHsl(Color color, out double h, out double s, out double l)
    {
        var r = color.R / 255d;
        var g = color.G / 255d;
        var b = color.B / 255d;
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;
        l = (max + min) / 2d;

        h = delta == 0d
            ? 0d
            : max == r
                ? 60d * (((g - b) / delta) % 6d)
                : max == g
                    ? 60d * ((b - r) / delta + 2d)
                    : 60d * ((r - g) / delta + 4d);
        if (h < 0d)
        {
            h += 360d;
        }

        s = delta == 0d ? 0d : delta / (1d - Math.Abs(2d * l - 1d));
    }

    private static string FormatHex(Color color, bool includeAlpha)
    {
        return includeAlpha && color.A < 255
            ? $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}"
            : $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    private static string FormatRgb(Color color, bool includeAlpha)
    {
        return includeAlpha && color.A < 255
            ? string.Create(CultureInfo.InvariantCulture, $"rgba({color.R}, {color.G}, {color.B}, {Math.Round(color.A / 255d, 2)})")
            : string.Create(CultureInfo.InvariantCulture, $"rgb({color.R}, {color.G}, {color.B})");
    }

    private static string FormatHsl(Color color, bool includeAlpha)
    {
        RgbToHsl(color, out var h, out var s, out var l);
        return includeAlpha && color.A < 255
            ? string.Create(CultureInfo.InvariantCulture, $"hsla({Math.Round(h)}, {Math.Round(s * 100d)}%, {Math.Round(l * 100d)}%, {Math.Round(color.A / 255d, 2)})")
            : string.Create(CultureInfo.InvariantCulture, $"hsl({Math.Round(h)}, {Math.Round(s * 100d)}%, {Math.Round(l * 100d)}%)");
    }

    private static string FormatHsv(double h, double s, double v, double a, bool includeAlpha)
    {
        return includeAlpha && a < 1d
            ? string.Create(CultureInfo.InvariantCulture, $"hsva({Math.Round(h)}, {Math.Round(s * 100d)}%, {Math.Round(v * 100d)}%, {Math.Round(a, 2)})")
            : string.Create(CultureInfo.InvariantCulture, $"hsv({Math.Round(h)}, {Math.Round(s * 100d)}%, {Math.Round(v * 100d)}%)");
    }

    private static string FormatModeLabel(NColorPickerMode mode)
    {
        return mode switch
        {
            NColorPickerMode.Hex => "HEX",
            NColorPickerMode.Hsl => "HSL",
            NColorPickerMode.Hsv => "HSV",
            _ => "RGB"
        };
    }

    private static double NormalizeHue(double value)
    {
        var hue = value % 360d;
        return hue < 0d ? hue + 360d : hue;
    }

    private static double Clamp(double value, double min, double max)
    {
        return Math.Max(min, Math.Min(max, value));
    }

    private readonly record struct ColorState(double Hue, double Saturation, double Value, double Alpha, NColorPickerMode Mode);
}

public sealed class NColorPickerValueChangedEventArgs : RoutedEventArgs
{
    public NColorPickerValueChangedEventArgs(RoutedEvent routedEvent, object source, string? oldValue, string? newValue)
        : base(routedEvent, source)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }

    public string? OldValue { get; }

    public string? NewValue { get; }
}

public sealed class NColorPickerCompleteEventArgs : RoutedEventArgs
{
    public NColorPickerCompleteEventArgs(RoutedEvent routedEvent, object source, string value)
        : base(routedEvent, source)
    {
        Value = value;
    }

    public string Value { get; }
}
