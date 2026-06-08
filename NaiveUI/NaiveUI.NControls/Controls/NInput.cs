using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public enum NInputType
{
    Text,
    Password,
    Textarea
}

public enum NInputPasswordShowMode
{
    Click,
    MouseDown
}

[TemplatePart(Name = TextBoxPartName, Type = typeof(TextBox))]
[TemplatePart(Name = PasswordBoxPartName, Type = typeof(PasswordBox))]
[TemplatePart(Name = PasswordToggleButtonPartName, Type = typeof(ButtonBase))]
[TemplatePart(Name = ResizeThumbPartName, Type = typeof(Thumb))]
public class NInput : Control
{
    private const string TextBoxPartName = "PART_TextBox";
    private const string PasswordBoxPartName = "PART_PasswordBox";
    private const string PasswordToggleButtonPartName = "PART_PasswordToggleButton";
    private const string ResizeThumbPartName = "PART_ResizeThumb";
    private const double TinyHeight = 22d;
    private const double SmallHeight = 28d;
    private const double MediumHeight = 34d;
    private const double LargeHeight = 40d;
    private const double TextareaLineHeight = 22d;

    private TextBox? textBoxPart;
    private ScrollViewer? textBoxScrollViewerPart;
    private PasswordBox? passwordBoxPart;
    private ButtonBase? passwordToggleButtonPart;
    private Thumb? resizeThumbPart;
    private bool syncingText;

    static NInput()
    {
        ElementBase.DefaultStyle<NInput>(DefaultStyleKeyProperty);
        FocusableProperty.OverrideMetadata(typeof(NInput), new FrameworkPropertyMetadata(true));
    }

    public NInput()
    {
        CommandBindings.Add(new CommandBinding(ClearCommand, HandleClearCommand, CanExecuteClearCommand));
        CommandBindings.Add(new CommandBinding(TogglePasswordVisibilityCommand, HandleTogglePasswordVisibilityCommand, CanExecuteTogglePasswordVisibilityCommand));
        Loaded += HandleLoaded;
        UpdateResolvedMetrics();
        UpdateTextState();
        UpdatePasswordIconState();
    }

    public static readonly RoutedUICommand ClearCommand = ElementBase.Command<NInput>(nameof(ClearCommand));

    public static readonly RoutedUICommand TogglePasswordVisibilityCommand = ElementBase.Command<NInput>(nameof(TogglePasswordVisibilityCommand));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(NInput),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public static readonly DependencyProperty PlaceholderProperty =
        ElementBase.Property<NInput, string>(nameof(PlaceholderProperty), string.Empty);

    public NInputType Type
    {
        get => (NInputType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public static readonly DependencyProperty TypeProperty =
        ElementBase.Property<NInput, NInputType>(nameof(TypeProperty), NInputType.Text, OnInputKindChanged);

    public NControlSize Size
    {
        get => (NControlSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public static readonly DependencyProperty SizeProperty =
        ElementBase.Property<NInput, NControlSize>(nameof(SizeProperty), NControlSize.Medium, OnMetricsPropertyChanged);

    public NSelectStatus Status
    {
        get => (NSelectStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public static readonly DependencyProperty StatusProperty =
        ElementBase.Property<NInput, NSelectStatus>(nameof(StatusProperty), NSelectStatus.Default);

    public bool IsInvalid
    {
        get => (bool)GetValue(IsInvalidProperty);
        set => SetValue(IsInvalidProperty, value);
    }

    public static readonly DependencyProperty IsInvalidProperty =
        ElementBase.Property<NInput, bool>(nameof(IsInvalidProperty), false);

    public bool Disabled
    {
        get => (bool)GetValue(DisabledProperty);
        set => SetValue(DisabledProperty, value);
    }

    public static readonly DependencyProperty DisabledProperty =
        ElementBase.Property<NInput, bool>(nameof(DisabledProperty), false, OnDisabledChanged);

    public bool Clearable
    {
        get => (bool)GetValue(ClearableProperty);
        set => SetValue(ClearableProperty, value);
    }

    public static readonly DependencyProperty ClearableProperty =
        ElementBase.Property<NInput, bool>(nameof(ClearableProperty), false, OnTextStatePropertyChanged);

    public bool Loading
    {
        get => (bool)GetValue(LoadingProperty);
        set => SetValue(LoadingProperty, value);
    }

    public static readonly DependencyProperty LoadingProperty =
        ElementBase.Property<NInput, bool>(nameof(LoadingProperty), false, OnTextStatePropertyChanged);

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public static readonly DependencyProperty IsReadOnlyProperty =
        ElementBase.Property<NInput, bool>(nameof(IsReadOnlyProperty), false, OnTextStatePropertyChanged);

    public bool Round
    {
        get => (bool)GetValue(RoundProperty);
        set => SetValue(RoundProperty, value);
    }

    public static readonly DependencyProperty RoundProperty =
        ElementBase.Property<NInput, bool>(nameof(RoundProperty), false, OnMetricsPropertyChanged);

    public bool ShowCount
    {
        get => (bool)GetValue(ShowCountProperty);
        set => SetValue(ShowCountProperty, value);
    }

    public static readonly DependencyProperty ShowCountProperty =
        ElementBase.Property<NInput, bool>(nameof(ShowCountProperty), false, OnTextStatePropertyChanged);

    public int MaxLength
    {
        get => (int)GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    public static readonly DependencyProperty MaxLengthProperty =
        ElementBase.Property<NInput, int>(nameof(MaxLengthProperty), 0, OnTextStatePropertyChanged);

    public int MinLength
    {
        get => (int)GetValue(MinLengthProperty);
        set => SetValue(MinLengthProperty, value);
    }

    public static readonly DependencyProperty MinLengthProperty =
        ElementBase.Property<NInput, int>(nameof(MinLengthProperty), 0);

    public int Rows
    {
        get => (int)GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }

    public static readonly DependencyProperty RowsProperty =
        ElementBase.Property<NInput, int>(nameof(RowsProperty), 3, OnMetricsPropertyChanged);

    public bool Autosize
    {
        get => (bool)GetValue(AutosizeProperty);
        set => SetValue(AutosizeProperty, value);
    }

    public static readonly DependencyProperty AutosizeProperty =
        ElementBase.Property<NInput, bool>(nameof(AutosizeProperty), false, OnMetricsPropertyChanged);

    public int MinRows
    {
        get => (int)GetValue(MinRowsProperty);
        set => SetValue(MinRowsProperty, value);
    }

    public static readonly DependencyProperty MinRowsProperty =
        ElementBase.Property<NInput, int>(nameof(MinRowsProperty), 1, OnMetricsPropertyChanged);

    public int MaxRows
    {
        get => (int)GetValue(MaxRowsProperty);
        set => SetValue(MaxRowsProperty, value);
    }

    public static readonly DependencyProperty MaxRowsProperty =
        ElementBase.Property<NInput, int>(nameof(MaxRowsProperty), 0, OnMetricsPropertyChanged);

    public bool Resizable
    {
        get => (bool)GetValue(ResizableProperty);
        set => SetValue(ResizableProperty, value);
    }

    public static readonly DependencyProperty ResizableProperty =
        ElementBase.Property<NInput, bool>(nameof(ResizableProperty), true);

    public object? PrefixContent
    {
        get => GetValue(PrefixContentProperty);
        set => SetValue(PrefixContentProperty, value);
    }

    public static readonly DependencyProperty PrefixContentProperty =
        ElementBase.Property<NInput, object?>(nameof(PrefixContentProperty), null, OnContentPropertyChanged);

    public object? SuffixContent
    {
        get => GetValue(SuffixContentProperty);
        set => SetValue(SuffixContentProperty, value);
    }

    public static readonly DependencyProperty SuffixContentProperty =
        ElementBase.Property<NInput, object?>(nameof(SuffixContentProperty), null, OnContentPropertyChanged);

    public bool ShowPasswordToggle
    {
        get => (bool)GetValue(ShowPasswordToggleProperty);
        set => SetValue(ShowPasswordToggleProperty, value);
    }

    public static readonly DependencyProperty ShowPasswordToggleProperty =
        ElementBase.Property<NInput, bool>(nameof(ShowPasswordToggleProperty), true);

    public object? PasswordVisibleIconContent
    {
        get => GetValue(PasswordVisibleIconContentProperty);
        set => SetValue(PasswordVisibleIconContentProperty, value);
    }

    public static readonly DependencyProperty PasswordVisibleIconContentProperty =
        ElementBase.Property<NInput, object?>(nameof(PasswordVisibleIconContentProperty), null, OnPasswordIconPropertyChanged);

    public object? PasswordHiddenIconContent
    {
        get => GetValue(PasswordHiddenIconContentProperty);
        set => SetValue(PasswordHiddenIconContentProperty, value);
    }

    public static readonly DependencyProperty PasswordHiddenIconContentProperty =
        ElementBase.Property<NInput, object?>(nameof(PasswordHiddenIconContentProperty), null, OnPasswordIconPropertyChanged);

    public NInputPasswordShowMode ShowPasswordOn
    {
        get => (NInputPasswordShowMode)GetValue(ShowPasswordOnProperty);
        set => SetValue(ShowPasswordOnProperty, value);
    }

    public static readonly DependencyProperty ShowPasswordOnProperty =
        ElementBase.Property<NInput, NInputPasswordShowMode>(nameof(ShowPasswordOnProperty), NInputPasswordShowMode.Click);

    public bool IsPasswordVisible
    {
        get => (bool)GetValue(IsPasswordVisibleProperty);
        set => SetValue(IsPasswordVisibleProperty, value);
    }

    public static readonly DependencyProperty IsPasswordVisibleProperty =
        ElementBase.Property<NInput, bool>(nameof(IsPasswordVisibleProperty), false, OnPasswordVisibleChanged);

    public bool HasText
    {
        get => (bool)GetValue(HasTextProperty);
        private set => SetValue(HasTextPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasTextPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasText), typeof(bool), typeof(NInput), new PropertyMetadata(false));

    public static readonly DependencyProperty HasTextProperty = HasTextPropertyKey.DependencyProperty;

    public bool IsEditorReadOnly
    {
        get => (bool)GetValue(IsEditorReadOnlyProperty);
        private set => SetValue(IsEditorReadOnlyPropertyKey, value);
    }

    private static readonly DependencyPropertyKey IsEditorReadOnlyPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsEditorReadOnly), typeof(bool), typeof(NInput), new PropertyMetadata(false));

    public static readonly DependencyProperty IsEditorReadOnlyProperty = IsEditorReadOnlyPropertyKey.DependencyProperty;

    public bool CanClear
    {
        get => (bool)GetValue(CanClearProperty);
        private set => SetValue(CanClearPropertyKey, value);
    }

    private static readonly DependencyPropertyKey CanClearPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(CanClear), typeof(bool), typeof(NInput), new PropertyMetadata(false));

    public static readonly DependencyProperty CanClearProperty = CanClearPropertyKey.DependencyProperty;

    public string CountText
    {
        get => (string)GetValue(CountTextProperty);
        private set => SetValue(CountTextPropertyKey, value);
    }

    private static readonly DependencyPropertyKey CountTextPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(CountText), typeof(string), typeof(NInput), new PropertyMetadata("0"));

    public static readonly DependencyProperty CountTextProperty = CountTextPropertyKey.DependencyProperty;

    public bool HasPrefix
    {
        get => (bool)GetValue(HasPrefixProperty);
        private set => SetValue(HasPrefixPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasPrefixPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasPrefix), typeof(bool), typeof(NInput), new PropertyMetadata(false));

    public static readonly DependencyProperty HasPrefixProperty = HasPrefixPropertyKey.DependencyProperty;

    public bool HasSuffix
    {
        get => (bool)GetValue(HasSuffixProperty);
        private set => SetValue(HasSuffixPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasSuffixPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasSuffix), typeof(bool), typeof(NInput), new PropertyMetadata(false));

    public static readonly DependencyProperty HasSuffixProperty = HasSuffixPropertyKey.DependencyProperty;

    public object? ResolvedPasswordIconContent
    {
        get => GetValue(ResolvedPasswordIconContentProperty);
        private set => SetValue(ResolvedPasswordIconContentPropertyKey, value);
    }

    private static readonly DependencyPropertyKey ResolvedPasswordIconContentPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ResolvedPasswordIconContent), typeof(object), typeof(NInput), new PropertyMetadata(null));

    public static readonly DependencyProperty ResolvedPasswordIconContentProperty = ResolvedPasswordIconContentPropertyKey.DependencyProperty;

    public bool HasCustomPasswordIcon
    {
        get => (bool)GetValue(HasCustomPasswordIconProperty);
        private set => SetValue(HasCustomPasswordIconPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasCustomPasswordIconPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasCustomPasswordIcon), typeof(bool), typeof(NInput), new PropertyMetadata(false));

    public static readonly DependencyProperty HasCustomPasswordIconProperty = HasCustomPasswordIconPropertyKey.DependencyProperty;

    public double ResolvedHeight
    {
        get => (double)GetValue(ResolvedHeightProperty);
        private set => SetValue(ResolvedHeightPropertyKey, value);
    }

    private static readonly DependencyPropertyKey ResolvedHeightPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ResolvedHeight), typeof(double), typeof(NInput), new PropertyMetadata(MediumHeight));

    public static readonly DependencyProperty ResolvedHeightProperty = ResolvedHeightPropertyKey.DependencyProperty;

    public double ResolvedTextareaMinHeight
    {
        get => (double)GetValue(ResolvedTextareaMinHeightProperty);
        private set => SetValue(ResolvedTextareaMinHeightPropertyKey, value);
    }

    private static readonly DependencyPropertyKey ResolvedTextareaMinHeightPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ResolvedTextareaMinHeight), typeof(double), typeof(NInput), new PropertyMetadata(76d));

    public static readonly DependencyProperty ResolvedTextareaMinHeightProperty = ResolvedTextareaMinHeightPropertyKey.DependencyProperty;

    public double ResolvedTextareaMaxHeight
    {
        get => (double)GetValue(ResolvedTextareaMaxHeightProperty);
        private set => SetValue(ResolvedTextareaMaxHeightPropertyKey, value);
    }

    private static readonly DependencyPropertyKey ResolvedTextareaMaxHeightPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ResolvedTextareaMaxHeight), typeof(double), typeof(NInput), new PropertyMetadata(double.PositiveInfinity));

    public static readonly DependencyProperty ResolvedTextareaMaxHeightProperty = ResolvedTextareaMaxHeightPropertyKey.DependencyProperty;

    public CornerRadius ResolvedCornerRadius
    {
        get => (CornerRadius)GetValue(ResolvedCornerRadiusProperty);
        private set => SetValue(ResolvedCornerRadiusPropertyKey, value);
    }

    private static readonly DependencyPropertyKey ResolvedCornerRadiusPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ResolvedCornerRadius), typeof(CornerRadius), typeof(NInput), new PropertyMetadata(new CornerRadius(3)));

    public static readonly DependencyProperty ResolvedCornerRadiusProperty = ResolvedCornerRadiusPropertyKey.DependencyProperty;

    public event RoutedEventHandler Clear
    {
        add => AddHandler(ClearEvent, value);
        remove => RemoveHandler(ClearEvent, value);
    }

    public static readonly RoutedEvent ClearEvent =
        EventManager.RegisterRoutedEvent(nameof(Clear), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NInput));

    public event RoutedPropertyChangedEventHandler<string> TextChanged
    {
        add => AddHandler(TextChangedEvent, value);
        remove => RemoveHandler(TextChangedEvent, value);
    }

    public static readonly RoutedEvent TextChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(TextChanged), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<string>), typeof(NInput));

    public override void OnApplyTemplate()
    {
        DetachTemplateParts();
        base.OnApplyTemplate();

        textBoxPart = GetTemplateChild(TextBoxPartName) as TextBox;
        passwordBoxPart = GetTemplateChild(PasswordBoxPartName) as PasswordBox;
        passwordToggleButtonPart = GetTemplateChild(PasswordToggleButtonPartName) as ButtonBase;
        resizeThumbPart = GetTemplateChild(ResizeThumbPartName) as Thumb;

        if (textBoxPart is not null)
        {
            textBoxPart.TextChanged += HandleInnerTextChanged;
            textBoxPart.Text = Text;
            textBoxPart.ApplyTemplate();
            textBoxScrollViewerPart = textBoxPart.Template.FindName("PART_ContentHost", textBoxPart) as ScrollViewer;
        }

        if (passwordBoxPart is not null)
        {
            passwordBoxPart.PasswordChanged += HandlePasswordChanged;
            passwordBoxPart.Password = Text;
        }

        if (passwordToggleButtonPart is not null)
        {
            passwordToggleButtonPart.PreviewMouseLeftButtonDown += HandlePasswordToggleMouseDown;
            passwordToggleButtonPart.PreviewMouseLeftButtonUp += HandlePasswordToggleMouseUp;
            passwordToggleButtonPart.LostMouseCapture += HandlePasswordToggleLostMouseCapture;
        }

        if (resizeThumbPart is not null)
        {
            resizeThumbPart.DragDelta += HandleResizeThumbDragDelta;
        }

        SyncTemplateText();
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == IsEnabledProperty)
        {
            UpdateTextState();
        }
    }

    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnGotKeyboardFocus(e);

        if (e.OriginalSource == this)
        {
            FocusInnerEditor();
        }
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        if (Disabled)
        {
            IsEnabled = false;
        }
    }

    private void HandleInnerTextChanged(object sender, TextChangedEventArgs e)
    {
        if (syncingText || textBoxPart is null)
        {
            return;
        }

        if (IsEditorReadOnly)
        {
            SyncTemplateText();
            return;
        }

        SetCurrentValue(TextProperty, textBoxPart.Text);
        BringTextCaretIntoView();
    }

    private void HandlePasswordChanged(object sender, RoutedEventArgs e)
    {
        if (syncingText || passwordBoxPart is null)
        {
            return;
        }

        if (IsEditorReadOnly)
        {
            SyncTemplateText();
            return;
        }

        SetCurrentValue(TextProperty, passwordBoxPart.Password);
    }

    private void HandlePasswordToggleMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (ShowPasswordOn != NInputPasswordShowMode.MouseDown || Type != NInputType.Password || !ShowPasswordToggle || !IsEnabled || Loading)
        {
            return;
        }

        SetCurrentValue(IsPasswordVisibleProperty, true);
    }

    private void HandlePasswordToggleMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (ShowPasswordOn != NInputPasswordShowMode.MouseDown)
        {
            return;
        }

        SetCurrentValue(IsPasswordVisibleProperty, false);
    }

    private void HandlePasswordToggleLostMouseCapture(object sender, MouseEventArgs e)
    {
        if (ShowPasswordOn == NInputPasswordShowMode.MouseDown)
        {
            SetCurrentValue(IsPasswordVisibleProperty, false);
        }
    }

    private void DetachTemplateParts()
    {
        if (textBoxPart is not null)
        {
            textBoxPart.TextChanged -= HandleInnerTextChanged;
        }

        if (passwordBoxPart is not null)
        {
            passwordBoxPart.PasswordChanged -= HandlePasswordChanged;
        }

        if (passwordToggleButtonPart is not null)
        {
            passwordToggleButtonPart.PreviewMouseLeftButtonDown -= HandlePasswordToggleMouseDown;
            passwordToggleButtonPart.PreviewMouseLeftButtonUp -= HandlePasswordToggleMouseUp;
            passwordToggleButtonPart.LostMouseCapture -= HandlePasswordToggleLostMouseCapture;
        }

        if (resizeThumbPart is not null)
        {
            resizeThumbPart.DragDelta -= HandleResizeThumbDragDelta;
        }

        textBoxPart = null;
        textBoxScrollViewerPart = null;
        passwordBoxPart = null;
        passwordToggleButtonPart = null;
        resizeThumbPart = null;
    }

    private void HandleResizeThumbDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (Type != NInputType.Textarea || !Resizable || !IsEnabled || Loading)
        {
            return;
        }

        var currentWidth = double.IsNaN(Width) ? ActualWidth : Width;
        var currentHeight = double.IsNaN(Height) ? ActualHeight : Height;
        var minWidth = Math.Max(MinWidth, 1d);
        var minHeight = Math.Max(MinHeight, ResolvedTextareaMinHeight);
        var maxWidth = double.IsPositiveInfinity(MaxWidth) ? double.PositiveInfinity : Math.Max(minWidth, MaxWidth);
        var maxHeight = double.IsPositiveInfinity(MaxHeight) ? double.PositiveInfinity : Math.Max(minHeight, MaxHeight);

        var resizedWidth = Math.Clamp(currentWidth + e.HorizontalChange, minWidth, maxWidth);
        var resizedHeight = Math.Clamp(currentHeight + e.VerticalChange, minHeight, maxHeight);

        SetCurrentValue(WidthProperty, resizedWidth);
        SetCurrentValue(HeightProperty, resizedHeight);
        e.Handled = true;
    }

    private void FocusInnerEditor()
    {
        if (Type == NInputType.Password && !IsPasswordVisible && passwordBoxPart is not null)
        {
            passwordBoxPart.Focus();
            return;
        }

        textBoxPart?.Focus();
    }

    private void SyncTemplateText()
    {
        syncingText = true;

        try
        {
            if (textBoxPart is not null && textBoxPart.Text != Text)
            {
                textBoxPart.Text = Text;
            }

            if (passwordBoxPart is not null && passwordBoxPart.Password != Text)
            {
                passwordBoxPart.Password = Text;
            }
        }
        finally
        {
            syncingText = false;
        }
    }

    private void BringTextCaretIntoView()
    {
        if (textBoxPart is null || Type == NInputType.Textarea || textBoxPart.SelectionLength > 0)
        {
            return;
        }

        if (textBoxPart.CaretIndex < textBoxPart.Text.Length)
        {
            return;
        }

        textBoxPart.CaretIndex = textBoxPart.Text.Length;
        textBoxPart.Dispatcher.BeginInvoke(() =>
        {
            if (textBoxPart is not null && Type != NInputType.Textarea && textBoxPart.SelectionLength == 0 && textBoxPart.CaretIndex >= textBoxPart.Text.Length)
            {
                textBoxPart.CaretIndex = textBoxPart.Text.Length;
                textBoxPart.ScrollToHorizontalOffset(double.MaxValue);
                textBoxPart.ScrollToEnd();
                textBoxScrollViewerPart?.ScrollToRightEnd();
            }
        }, DispatcherPriority.ContextIdle);
    }

    private void ClearText()
    {
        if (IsEditorReadOnly || !IsEnabled)
        {
            return;
        }

        SetCurrentValue(TextProperty, string.Empty);
        RaiseEvent(new RoutedEventArgs(ClearEvent, this));
        FocusInnerEditor();
    }

    private void UpdateTextState()
    {
        var text = Text ?? string.Empty;
        HasText = text.Length > 0;
        IsEditorReadOnly = IsReadOnly || Loading;
        CanClear = Clearable && IsEnabled && !IsEditorReadOnly && text.Length > 0;
        CountText = MaxLength > 0 ? $"{text.Length} / {MaxLength}" : text.Length.ToString();
        CommandManager.InvalidateRequerySuggested();
    }

    private void UpdateResolvedMetrics()
    {
        ResolvedHeight = Size switch
        {
            NControlSize.Tiny => TinyHeight,
            NControlSize.Small => SmallHeight,
            NControlSize.Large => LargeHeight,
            _ => MediumHeight
        };

        ResolvedCornerRadius = Round ? new CornerRadius(999) : new CornerRadius(3);

        var rows = Math.Max(1, Rows);
        var minRows = Autosize ? Math.Max(1, MinRows) : rows;
        var maxRows = Autosize && MaxRows > 0 ? Math.Max(minRows, MaxRows) : rows;
        var verticalPadding = Size switch
        {
            NControlSize.Tiny => 8d,
            NControlSize.Small => 10d,
            NControlSize.Large => 16d,
            _ => 12d
        };

        ResolvedTextareaMinHeight = minRows * TextareaLineHeight + verticalPadding;
        ResolvedTextareaMaxHeight = maxRows * TextareaLineHeight + verticalPadding;
    }

    private void UpdateContentState()
    {
        HasPrefix = PrefixContent is not null;
        HasSuffix = SuffixContent is not null;
    }

    private void UpdatePasswordIconState()
    {
        var icon = IsPasswordVisible ? PasswordVisibleIconContent : PasswordHiddenIconContent;
        ResolvedPasswordIconContent = icon;
        HasCustomPasswordIcon = icon is not null;
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var input = (NInput)d;
        var oldValue = e.OldValue as string ?? string.Empty;
        var newValue = e.NewValue as string ?? string.Empty;

        input.SyncTemplateText();
        input.UpdateTextState();

        if (oldValue != newValue)
        {
            input.RaiseEvent(new RoutedPropertyChangedEventArgs<string>(oldValue, newValue, TextChangedEvent));
        }
    }

    private static void OnInputKindChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var input = (NInput)d;

        if ((NInputType)e.NewValue != NInputType.Password)
        {
            input.SetCurrentValue(IsPasswordVisibleProperty, false);
        }

        input.UpdateResolvedMetrics();
        input.SyncTemplateText();
        input.UpdatePasswordIconState();
    }

    private static void OnPasswordVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var input = (NInput)d;
        input.SyncTemplateText();
        input.UpdatePasswordIconState();
    }

    private static void OnMetricsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((NInput)d).UpdateResolvedMetrics();
    }

    private static void OnTextStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((NInput)d).UpdateTextState();
    }

    private static void OnContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((NInput)d).UpdateContentState();
    }

    private static void OnPasswordIconPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((NInput)d).UpdatePasswordIconState();
    }

    private static void OnDisabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var input = (NInput)d;
        input.IsEnabled = !((bool)e.NewValue);
        input.UpdateTextState();
    }

    private void CanExecuteClearCommand(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = CanClear;
        e.Handled = true;
    }

    private void HandleClearCommand(object sender, ExecutedRoutedEventArgs e)
    {
        ClearText();
        e.Handled = true;
    }

    private void CanExecuteTogglePasswordVisibilityCommand(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = Type == NInputType.Password && ShowPasswordToggle && IsEnabled && !Loading;
        e.Handled = true;
    }

    private void HandleTogglePasswordVisibilityCommand(object sender, ExecutedRoutedEventArgs e)
    {
        if (ShowPasswordOn == NInputPasswordShowMode.MouseDown)
        {
            e.Handled = true;
            return;
        }

        SetCurrentValue(IsPasswordVisibleProperty, !IsPasswordVisible);
        FocusInnerEditor();
        e.Handled = true;
    }
}
