using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public enum NAutoCompleteTrigger
{
    Focus,
    Input,
    Manual
}

public enum NAutoCompleteMatchMode
{
    Contains,
    StartsWith
}

[ContentProperty(nameof(Options))]
public class NAutoComplete : Control
{
    private const string PopupPartName = "PART_Popup";
    private const string TextBoxPartName = "PART_TextBox";
    private const string ListBoxPartName = "PART_ListBox";
    private const string ClearButtonPartName = "PART_ClearButton";
    private const string ArrowButtonPartName = "PART_ArrowButton";
    private static NAutoComplete? openInstance;

    private Popup? popupPart;
    private TextBox? textBoxPart;
    private ListBox? listBoxPart;
    private Button? clearButtonPart;
    private Button? arrowButtonPart;
    private Window? ownerWindow;
    private INotifyCollectionChanged? itemsSourceNotifier;
    private bool syncingSuggestionSelection;
    private bool syncingTextBoxText;
    private bool committingSelection;
    private bool suppressAutoOpen;
    private bool textUpdateOriginatedFromInnerTextBox;

    static NAutoComplete()
    {
        ElementBase.DefaultStyle<NAutoComplete>(DefaultStyleKeyProperty);
    }

    public NAutoComplete()
    {
        Options.CollectionChanged += HandleOptionsCollectionChanged;
        FilteredOptions = [];
        UpdateResolvedMetrics();
        UpdatePopupMetrics();
        UpdateVisualState();
        RefreshOptions();
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ObservableCollection<NSelectOption> Options { get; } = [];

    public ObservableCollection<NSelectOption> FilteredOptions
    {
        get => (ObservableCollection<NSelectOption>)GetValue(FilteredOptionsProperty);
        private set => SetValue(FilteredOptionsPropertyKey, value);
    }

    private static readonly DependencyPropertyKey FilteredOptionsPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(FilteredOptions), typeof(ObservableCollection<NSelectOption>), typeof(NAutoComplete), new PropertyMetadata(null));

    public static readonly DependencyProperty FilteredOptionsProperty = FilteredOptionsPropertyKey.DependencyProperty;

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty ItemsSourceProperty =
        ElementBase.Property<NAutoComplete, IEnumerable?>(nameof(ItemsSourceProperty), null, OnItemsChanged);

    public string DisplayMemberPath
    {
        get => (string)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    public static readonly DependencyProperty DisplayMemberPathProperty =
        ElementBase.Property<NAutoComplete, string>(nameof(DisplayMemberPathProperty), string.Empty, OnItemsChanged);

    public string ValueMemberPath
    {
        get => (string)GetValue(ValueMemberPathProperty);
        set => SetValue(ValueMemberPathProperty, value);
    }

    public static readonly DependencyProperty ValueMemberPathProperty =
        ElementBase.Property<NAutoComplete, string>(nameof(ValueMemberPathProperty), string.Empty, OnItemsChanged);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(NAutoComplete),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(object),
            typeof(NAutoComplete),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        private set => SetValue(SelectedItemPropertyKey, value);
    }

    private static readonly DependencyPropertyKey SelectedItemPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(SelectedItem), typeof(object), typeof(NAutoComplete), new PropertyMetadata(null));

    public static readonly DependencyProperty SelectedItemProperty = SelectedItemPropertyKey.DependencyProperty;

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public static readonly DependencyProperty PlaceholderProperty =
        ElementBase.Property<NAutoComplete, string>(nameof(PlaceholderProperty), string.Empty);

    public bool Clearable
    {
        get => (bool)GetValue(ClearableProperty);
        set => SetValue(ClearableProperty, value);
    }

    public static readonly DependencyProperty ClearableProperty =
        ElementBase.Property<NAutoComplete, bool>(nameof(ClearableProperty), false, OnVisualPropertyChanged);

    public bool Loading
    {
        get => (bool)GetValue(LoadingProperty);
        set => SetValue(LoadingProperty, value);
    }

    public static readonly DependencyProperty LoadingProperty =
        ElementBase.Property<NAutoComplete, bool>(nameof(LoadingProperty), false, OnVisualPropertyChanged);

    public bool SelectOnEnter
    {
        get => (bool)GetValue(SelectOnEnterProperty);
        set => SetValue(SelectOnEnterProperty, value);
    }

    public static readonly DependencyProperty SelectOnEnterProperty =
        ElementBase.Property<NAutoComplete, bool>(nameof(SelectOnEnterProperty), true);

    public bool CloseOnSelect
    {
        get => (bool)GetValue(CloseOnSelectProperty);
        set => SetValue(CloseOnSelectProperty, value);
    }

    public static readonly DependencyProperty CloseOnSelectProperty =
        ElementBase.Property<NAutoComplete, bool>(nameof(CloseOnSelectProperty), true);

    public bool BlurAfterSelect
    {
        get => (bool)GetValue(BlurAfterSelectProperty);
        set => SetValue(BlurAfterSelectProperty, value);
    }

    public static readonly DependencyProperty BlurAfterSelectProperty =
        ElementBase.Property<NAutoComplete, bool>(nameof(BlurAfterSelectProperty), false);

    public bool ClearAfterSelect
    {
        get => (bool)GetValue(ClearAfterSelectProperty);
        set => SetValue(ClearAfterSelectProperty, value);
    }

    public static readonly DependencyProperty ClearAfterSelectProperty =
        ElementBase.Property<NAutoComplete, bool>(nameof(ClearAfterSelectProperty), false);

    public bool UpdateTextOnSelect
    {
        get => (bool)GetValue(UpdateTextOnSelectProperty);
        set => SetValue(UpdateTextOnSelectProperty, value);
    }

    public static readonly DependencyProperty UpdateTextOnSelectProperty =
        ElementBase.Property<NAutoComplete, bool>(nameof(UpdateTextOnSelectProperty), true);

    public bool OpenOnFocus
    {
        get => (bool)GetValue(OpenOnFocusProperty);
        set => SetValue(OpenOnFocusProperty, value);
    }

    public static readonly DependencyProperty OpenOnFocusProperty =
        ElementBase.Property<NAutoComplete, bool>(nameof(OpenOnFocusProperty), false);

    public bool OpenOnInput
    {
        get => (bool)GetValue(OpenOnInputProperty);
        set => SetValue(OpenOnInputProperty, value);
    }

    public static readonly DependencyProperty OpenOnInputProperty =
        ElementBase.Property<NAutoComplete, bool>(nameof(OpenOnInputProperty), true);

    public bool ShowArrow
    {
        get => (bool)GetValue(ShowArrowProperty);
        set => SetValue(ShowArrowProperty, value);
    }

    public static readonly DependencyProperty ShowArrowProperty =
        ElementBase.Property<NAutoComplete, bool>(nameof(ShowArrowProperty), false);

    public bool ShowEmptyWhenNoMatch
    {
        get => (bool)GetValue(ShowEmptyWhenNoMatchProperty);
        set => SetValue(ShowEmptyWhenNoMatchProperty, value);
    }

    public static readonly DependencyProperty ShowEmptyWhenNoMatchProperty =
        ElementBase.Property<NAutoComplete, bool>(nameof(ShowEmptyWhenNoMatchProperty), true, OnVisualPropertyChanged);

    public bool IgnoreCase
    {
        get => (bool)GetValue(IgnoreCaseProperty);
        set => SetValue(IgnoreCaseProperty, value);
    }

    public static readonly DependencyProperty IgnoreCaseProperty =
        ElementBase.Property<NAutoComplete, bool>(nameof(IgnoreCaseProperty), true, OnItemsChanged);

    public bool SelectFirstOptionOnOpen
    {
        get => (bool)GetValue(SelectFirstOptionOnOpenProperty);
        set => SetValue(SelectFirstOptionOnOpenProperty, value);
    }

    public static readonly DependencyProperty SelectFirstOptionOnOpenProperty =
        ElementBase.Property<NAutoComplete, bool>(nameof(SelectFirstOptionOnOpenProperty), false);

    public bool EnableInternalFilter
    {
        get => (bool)GetValue(EnableInternalFilterProperty);
        set => SetValue(EnableInternalFilterProperty, value);
    }

    public static readonly DependencyProperty EnableInternalFilterProperty =
        ElementBase.Property<NAutoComplete, bool>(nameof(EnableInternalFilterProperty), false, OnItemsChanged);

    public bool ShowCompletionHint
    {
        get => (bool)GetValue(ShowCompletionHintProperty);
        set => SetValue(ShowCompletionHintProperty, value);
    }

    public static readonly DependencyProperty ShowCompletionHintProperty =
        ElementBase.Property<NAutoComplete, bool>(nameof(ShowCompletionHintProperty), true, OnVisualPropertyChanged);

    public bool AcceptSuggestionOnTab
    {
        get => (bool)GetValue(AcceptSuggestionOnTabProperty);
        set => SetValue(AcceptSuggestionOnTabProperty, value);
    }

    public static readonly DependencyProperty AcceptSuggestionOnTabProperty =
        ElementBase.Property<NAutoComplete, bool>(nameof(AcceptSuggestionOnTabProperty), true);

    public Func<string, bool>? GetShow
    {
        get => (Func<string, bool>?)GetValue(GetShowProperty);
        set => SetValue(GetShowProperty, value);
    }

    public static readonly DependencyProperty GetShowProperty =
        ElementBase.Property<NAutoComplete, Func<string, bool>?>(nameof(GetShowProperty), null, OnVisualPropertyChanged);

    public NAutoCompleteTrigger Trigger
    {
        get => (NAutoCompleteTrigger)GetValue(TriggerProperty);
        set => SetValue(TriggerProperty, value);
    }

    public static readonly DependencyProperty TriggerProperty =
        ElementBase.Property<NAutoComplete, NAutoCompleteTrigger>(nameof(TriggerProperty), NAutoCompleteTrigger.Input);

    public NAutoCompleteMatchMode MatchMode
    {
        get => (NAutoCompleteMatchMode)GetValue(MatchModeProperty);
        set => SetValue(MatchModeProperty, value);
    }

    public static readonly DependencyProperty MatchModeProperty =
        ElementBase.Property<NAutoComplete, NAutoCompleteMatchMode>(nameof(MatchModeProperty), NAutoCompleteMatchMode.Contains, OnItemsChanged);

    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public static readonly DependencyProperty IsDropDownOpenProperty =
        DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(NAutoComplete),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDropDownOpenChanged));

    public NSelectSize Size
    {
        get => (NSelectSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public static readonly DependencyProperty SizeProperty =
        ElementBase.Property<NAutoComplete, NSelectSize>(nameof(SizeProperty), NSelectSize.Medium, OnVisualPropertyChanged);

    public NSelectStatus Status
    {
        get => (NSelectStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public static readonly DependencyProperty StatusProperty =
        ElementBase.Property<NAutoComplete, NSelectStatus>(nameof(StatusProperty), NSelectStatus.Default);

    public bool IsInvalid
    {
        get => (bool)GetValue(IsInvalidProperty);
        set => SetValue(IsInvalidProperty, value);
    }

    public static readonly DependencyProperty IsInvalidProperty =
        ElementBase.Property<NAutoComplete, bool>(nameof(IsInvalidProperty), false);

    public NSelectPlacement Placement
    {
        get => (NSelectPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public static readonly DependencyProperty PlacementProperty =
        ElementBase.Property<NAutoComplete, NSelectPlacement>(nameof(PlacementProperty), NSelectPlacement.BottomStart, OnPopupPropertyChanged);

    public double MaxDropDownHeight
    {
        get => (double)GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    public static readonly DependencyProperty MaxDropDownHeightProperty =
        ElementBase.Property<NAutoComplete, double>(nameof(MaxDropDownHeightProperty), 280d);

    public double DropDownWidth
    {
        get => (double)GetValue(DropDownWidthProperty);
        set => SetValue(DropDownWidthProperty, value);
    }

    public static readonly DependencyProperty DropDownWidthProperty =
        ElementBase.Property<NAutoComplete, double>(nameof(DropDownWidthProperty), double.NaN, OnPopupPropertyChanged);

    public object? PrefixContent
    {
        get => GetValue(PrefixContentProperty);
        set => SetValue(PrefixContentProperty, value);
    }

    public static readonly DependencyProperty PrefixContentProperty =
        ElementBase.Property<NAutoComplete, object?>(nameof(PrefixContentProperty), null);

    public object? SuffixContent
    {
        get => GetValue(SuffixContentProperty);
        set => SetValue(SuffixContentProperty, value);
    }

    public static readonly DependencyProperty SuffixContentProperty =
        ElementBase.Property<NAutoComplete, object?>(nameof(SuffixContentProperty), null);

    public object? ArrowContent
    {
        get => GetValue(ArrowContentProperty);
        set => SetValue(ArrowContentProperty, value);
    }

    public static readonly DependencyProperty ArrowContentProperty =
        ElementBase.Property<NAutoComplete, object?>(nameof(ArrowContentProperty), null);

    public object? ClearIconContent
    {
        get => GetValue(ClearIconContentProperty);
        set => SetValue(ClearIconContentProperty, value);
    }

    public static readonly DependencyProperty ClearIconContentProperty =
        ElementBase.Property<NAutoComplete, object?>(nameof(ClearIconContentProperty), null);

    public object? HeaderContent
    {
        get => GetValue(HeaderContentProperty);
        set => SetValue(HeaderContentProperty, value);
    }

    public static readonly DependencyProperty HeaderContentProperty =
        ElementBase.Property<NAutoComplete, object?>(nameof(HeaderContentProperty), null, OnVisualPropertyChanged);

    public object? ActionContent
    {
        get => GetValue(ActionContentProperty);
        set => SetValue(ActionContentProperty, value);
    }

    public static readonly DependencyProperty ActionContentProperty =
        ElementBase.Property<NAutoComplete, object?>(nameof(ActionContentProperty), null, OnVisualPropertyChanged);

    public object? NoDataContent
    {
        get => GetValue(NoDataContentProperty);
        set => SetValue(NoDataContentProperty, value);
    }

    public static readonly DependencyProperty NoDataContentProperty =
        ElementBase.Property<NAutoComplete, object?>(nameof(NoDataContentProperty), "暂无匹配项");

    public object? LoadingContent
    {
        get => GetValue(LoadingContentProperty);
        set => SetValue(LoadingContentProperty, value);
    }

    public static readonly DependencyProperty LoadingContentProperty =
        ElementBase.Property<NAutoComplete, object?>(nameof(LoadingContentProperty), "加载中");

    public DataTemplate? OptionTemplate
    {
        get => (DataTemplate?)GetValue(OptionTemplateProperty);
        set => SetValue(OptionTemplateProperty, value);
    }

    public static readonly DependencyProperty OptionTemplateProperty =
        ElementBase.Property<NAutoComplete, DataTemplate?>(nameof(OptionTemplateProperty), null);

    public DataTemplate? SelectionBoxTemplate
    {
        get => (DataTemplate?)GetValue(SelectionBoxTemplateProperty);
        set => SetValue(SelectionBoxTemplateProperty, value);
    }

    public static readonly DependencyProperty SelectionBoxTemplateProperty =
        ElementBase.Property<NAutoComplete, DataTemplate?>(nameof(SelectionBoxTemplateProperty), null);

    public double ResolvedHeight
    {
        get => (double)GetValue(ResolvedHeightProperty);
        private set => SetValue(ResolvedHeightPropertyKey, value);
    }

    private static readonly DependencyPropertyKey ResolvedHeightPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ResolvedHeight), typeof(double), typeof(NAutoComplete), new PropertyMetadata(34d));

    public static readonly DependencyProperty ResolvedHeightProperty = ResolvedHeightPropertyKey.DependencyProperty;

    public double ResolvedFontSize
    {
        get => (double)GetValue(ResolvedFontSizeProperty);
        private set => SetValue(ResolvedFontSizePropertyKey, value);
    }

    private static readonly DependencyPropertyKey ResolvedFontSizePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ResolvedFontSize), typeof(double), typeof(NAutoComplete), new PropertyMetadata(14d));

    public static readonly DependencyProperty ResolvedFontSizeProperty = ResolvedFontSizePropertyKey.DependencyProperty;

    public double EffectiveFontSize
    {
        get => (double)GetValue(EffectiveFontSizeProperty);
        private set => SetValue(EffectiveFontSizePropertyKey, value);
    }

    private static readonly DependencyPropertyKey EffectiveFontSizePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(EffectiveFontSize), typeof(double), typeof(NAutoComplete), new PropertyMetadata(14d));

    public static readonly DependencyProperty EffectiveFontSizeProperty = EffectiveFontSizePropertyKey.DependencyProperty;

    public Thickness ResolvedPadding
    {
        get => (Thickness)GetValue(ResolvedPaddingProperty);
        private set => SetValue(ResolvedPaddingPropertyKey, value);
    }

    private static readonly DependencyPropertyKey ResolvedPaddingPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ResolvedPadding), typeof(Thickness), typeof(NAutoComplete), new PropertyMetadata(new Thickness(12, 0, 8, 0)));

    public static readonly DependencyProperty ResolvedPaddingProperty = ResolvedPaddingPropertyKey.DependencyProperty;

    public double ResolvedOptionHeight
    {
        get => (double)GetValue(ResolvedOptionHeightProperty);
        private set => SetValue(ResolvedOptionHeightPropertyKey, value);
    }

    private static readonly DependencyPropertyKey ResolvedOptionHeightPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ResolvedOptionHeight), typeof(double), typeof(NAutoComplete), new PropertyMetadata(34d));

    public static readonly DependencyProperty ResolvedOptionHeightProperty = ResolvedOptionHeightPropertyKey.DependencyProperty;

    public bool HasFilteredOptions
    {
        get => (bool)GetValue(HasFilteredOptionsProperty);
        private set => SetValue(HasFilteredOptionsPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasFilteredOptionsPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasFilteredOptions), typeof(bool), typeof(NAutoComplete), new PropertyMetadata(false));

    public static readonly DependencyProperty HasFilteredOptionsProperty = HasFilteredOptionsPropertyKey.DependencyProperty;

    public bool HasHeaderContent
    {
        get => (bool)GetValue(HasHeaderContentProperty);
        private set => SetValue(HasHeaderContentPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasHeaderContentPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasHeaderContent), typeof(bool), typeof(NAutoComplete), new PropertyMetadata(false));

    public static readonly DependencyProperty HasHeaderContentProperty = HasHeaderContentPropertyKey.DependencyProperty;

    public bool HasActionContent
    {
        get => (bool)GetValue(HasActionContentProperty);
        private set => SetValue(HasActionContentPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasActionContentPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasActionContent), typeof(bool), typeof(NAutoComplete), new PropertyMetadata(false));

    public static readonly DependencyProperty HasActionContentProperty = HasActionContentPropertyKey.DependencyProperty;

    public bool HasPrefixContent
    {
        get => (bool)GetValue(HasPrefixContentProperty);
        private set => SetValue(HasPrefixContentPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasPrefixContentPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasPrefixContent), typeof(bool), typeof(NAutoComplete), new PropertyMetadata(false));

    public static readonly DependencyProperty HasPrefixContentProperty = HasPrefixContentPropertyKey.DependencyProperty;

    public bool HasSuffixContent
    {
        get => (bool)GetValue(HasSuffixContentProperty);
        private set => SetValue(HasSuffixContentPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasSuffixContentPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasSuffixContent), typeof(bool), typeof(NAutoComplete), new PropertyMetadata(false));

    public static readonly DependencyProperty HasSuffixContentProperty = HasSuffixContentPropertyKey.DependencyProperty;

    public bool HasArrowContent
    {
        get => (bool)GetValue(HasArrowContentProperty);
        private set => SetValue(HasArrowContentPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasArrowContentPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasArrowContent), typeof(bool), typeof(NAutoComplete), new PropertyMetadata(false));

    public static readonly DependencyProperty HasArrowContentProperty = HasArrowContentPropertyKey.DependencyProperty;

    public bool HasClearIconContent
    {
        get => (bool)GetValue(HasClearIconContentProperty);
        private set => SetValue(HasClearIconContentPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasClearIconContentPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasClearIconContent), typeof(bool), typeof(NAutoComplete), new PropertyMetadata(false));

    public static readonly DependencyProperty HasClearIconContentProperty = HasClearIconContentPropertyKey.DependencyProperty;

    public bool CanClear
    {
        get => (bool)GetValue(CanClearProperty);
        private set => SetValue(CanClearPropertyKey, value);
    }

    private static readonly DependencyPropertyKey CanClearPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(CanClear), typeof(bool), typeof(NAutoComplete), new PropertyMetadata(false));

    public static readonly DependencyProperty CanClearProperty = CanClearPropertyKey.DependencyProperty;

    public bool HasTrailingContent
    {
        get => (bool)GetValue(HasTrailingContentProperty);
        private set => SetValue(HasTrailingContentPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasTrailingContentPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasTrailingContent), typeof(bool), typeof(NAutoComplete), new PropertyMetadata(false));

    public static readonly DependencyProperty HasTrailingContentProperty = HasTrailingContentPropertyKey.DependencyProperty;

    public bool ShowPlaceholder
    {
        get => (bool)GetValue(ShowPlaceholderProperty);
        private set => SetValue(ShowPlaceholderPropertyKey, value);
    }

    private static readonly DependencyPropertyKey ShowPlaceholderPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ShowPlaceholder), typeof(bool), typeof(NAutoComplete), new PropertyMetadata(true));

    public static readonly DependencyProperty ShowPlaceholderProperty = ShowPlaceholderPropertyKey.DependencyProperty;

    public string CompletionSuffix
    {
        get => (string)GetValue(CompletionSuffixProperty);
        private set => SetValue(CompletionSuffixPropertyKey, value);
    }

    private static readonly DependencyPropertyKey CompletionSuffixPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(CompletionSuffix), typeof(string), typeof(NAutoComplete), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty CompletionSuffixProperty = CompletionSuffixPropertyKey.DependencyProperty;

    public bool HasCompletionSuffix
    {
        get => (bool)GetValue(HasCompletionSuffixProperty);
        private set => SetValue(HasCompletionSuffixPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasCompletionSuffixPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasCompletionSuffix), typeof(bool), typeof(NAutoComplete), new PropertyMetadata(false));

    public static readonly DependencyProperty HasCompletionSuffixProperty = HasCompletionSuffixPropertyKey.DependencyProperty;

    public double CompletionSuffixOffset
    {
        get => (double)GetValue(CompletionSuffixOffsetProperty);
        private set => SetValue(CompletionSuffixOffsetPropertyKey, value);
    }

    private static readonly DependencyPropertyKey CompletionSuffixOffsetPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(CompletionSuffixOffset), typeof(double), typeof(NAutoComplete), new PropertyMetadata(0d));

    public static readonly DependencyProperty CompletionSuffixOffsetProperty = CompletionSuffixOffsetPropertyKey.DependencyProperty;

    public PlacementMode ResolvedPopupPlacement
    {
        get => (PlacementMode)GetValue(ResolvedPopupPlacementProperty);
        private set => SetValue(ResolvedPopupPlacementPropertyKey, value);
    }

    private static readonly DependencyPropertyKey ResolvedPopupPlacementPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ResolvedPopupPlacement), typeof(PlacementMode), typeof(NAutoComplete), new PropertyMetadata(PlacementMode.Bottom));

    public static readonly DependencyProperty ResolvedPopupPlacementProperty = ResolvedPopupPlacementPropertyKey.DependencyProperty;

    public double ResolvedDropDownWidth
    {
        get => (double)GetValue(ResolvedDropDownWidthProperty);
        private set => SetValue(ResolvedDropDownWidthPropertyKey, value);
    }

    private static readonly DependencyPropertyKey ResolvedDropDownWidthPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ResolvedDropDownWidth), typeof(double), typeof(NAutoComplete), new PropertyMetadata(0d));

    public static readonly DependencyProperty ResolvedDropDownWidthProperty = ResolvedDropDownWidthPropertyKey.DependencyProperty;

    public static readonly RoutedEvent ValueChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(ValueChanged), RoutingStrategy.Bubble, typeof(EventHandler<NAutoCompleteValueChangedEventArgs>), typeof(NAutoComplete));

    public static readonly RoutedEvent OptionSelectedEvent =
        EventManager.RegisterRoutedEvent(nameof(OptionSelected), RoutingStrategy.Bubble, typeof(EventHandler<NAutoCompleteOptionSelectedEventArgs>), typeof(NAutoComplete));

    public static readonly RoutedEvent ClearEvent =
        ElementBase.RoutedEvent<NAutoComplete, RoutedEventHandler>(nameof(ClearEvent));

    public static readonly RoutedUICommand ClearCommand =
        ElementBase.Command<NAutoComplete>(nameof(ClearCommand));

    public event EventHandler<NAutoCompleteValueChangedEventArgs> ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    public event EventHandler<NAutoCompleteOptionSelectedEventArgs> OptionSelected
    {
        add => AddHandler(OptionSelectedEvent, value);
        remove => RemoveHandler(OptionSelectedEvent, value);
    }

    public event RoutedEventHandler Clear
    {
        add => AddHandler(ClearEvent, value);
        remove => RemoveHandler(ClearEvent, value);
    }

    public override void OnApplyTemplate()
    {
        if (listBoxPart is not null)
        {
            listBoxPart.SelectionChanged -= HandleListBoxSelectionChanged;
            listBoxPart.PreviewMouseLeftButtonDown -= HandleListBoxPreviewMouseLeftButtonDown;
            listBoxPart.PreviewMouseWheel -= HandleListBoxPreviewMouseWheel;
        }

        if (clearButtonPart is not null)
        {
            clearButtonPart.Click -= HandleClearButtonClick;
        }

        if (arrowButtonPart is not null)
        {
            arrowButtonPart.Click -= HandleArrowButtonClick;
        }

        if (textBoxPart is not null)
        {
            textBoxPart.TextChanged -= HandleInnerTextBoxTextChanged;
            textBoxPart.GotKeyboardFocus -= HandleTextBoxGotKeyboardFocus;
            textBoxPart.LostKeyboardFocus -= HandleTextBoxLostKeyboardFocus;
            textBoxPart.SelectionChanged -= HandleTextBoxSelectionChanged;
            textBoxPart.SizeChanged -= HandleTextBoxSizeChanged;
        }

        base.OnApplyTemplate();

        popupPart = GetTemplateChild(PopupPartName) as Popup;
        textBoxPart = GetTemplateChild(TextBoxPartName) as TextBox;
        listBoxPart = GetTemplateChild(ListBoxPartName) as ListBox;
        clearButtonPart = GetTemplateChild(ClearButtonPartName) as Button;
        arrowButtonPart = GetTemplateChild(ArrowButtonPartName) as Button;

        if (textBoxPart is not null)
        {
            textBoxPart.TextChanged += HandleInnerTextBoxTextChanged;
            textBoxPart.GotKeyboardFocus += HandleTextBoxGotKeyboardFocus;
            textBoxPart.LostKeyboardFocus += HandleTextBoxLostKeyboardFocus;
            textBoxPart.SelectionChanged += HandleTextBoxSelectionChanged;
            textBoxPart.SizeChanged += HandleTextBoxSizeChanged;
        }

        if (clearButtonPart is not null)
        {
            clearButtonPart.Click += HandleClearButtonClick;
        }

        if (arrowButtonPart is not null)
        {
            arrowButtonPart.Click += HandleArrowButtonClick;
        }

        if (listBoxPart is not null)
        {
            listBoxPart.SelectionChanged += HandleListBoxSelectionChanged;
            listBoxPart.PreviewMouseLeftButtonDown += HandleListBoxPreviewMouseLeftButtonDown;
            listBoxPart.PreviewMouseWheel += HandleListBoxPreviewMouseWheel;
        }

        UpdatePopupMetrics();
        UpdateTextBoxText();
        SyncListBoxSelection();
        SelectFirstSuggestionIfNeeded();
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;
        AttachItemsSourceCollectionChanged(ItemsSource);
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        UpdatePopupMetrics();
    }

    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnGotKeyboardFocus(e);

        if (e.NewFocus == this && textBoxPart is not null)
        {
            textBoxPart.Focus();
            textBoxPart.Select(textBoxPart.Text.Length, 0);
        }
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);

        if (!IsEnabled || textBoxPart is null)
        {
            return;
        }

        if (IsDescendantOfTextBox(e.OriginalSource as DependencyObject))
        {
            return;
        }

        if (!textBoxPart.IsKeyboardFocusWithin)
        {
            textBoxPart.Focus();
        }
    }

    protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
    {
        base.OnPreviewMouseWheel(e);

        if (e.Handled || !IsDropDownOpen)
        {
            return;
        }

        ScrollDropDown(e);
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (e.Key == Key.Down)
        {
            EnsureDropDownOpenForKeyboard();
            MoveSuggestion(1);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Up)
        {
            EnsureDropDownOpenForKeyboard();
            MoveSuggestion(-1);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Enter && SelectOnEnter && IsDropDownOpen)
        {
            if (TryCommitHighlightedOption())
            {
                e.Handled = true;
            }

            return;
        }

        if (e.Key == Key.Tab && AcceptSuggestionOnTab && TryAcceptCompletionHint())
        {
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape && IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
        }
    }

    public new bool Focus()
    {
        if (!IsEnabled)
        {
            return false;
        }

        if (textBoxPart is not null)
        {
            textBoxPart.Focus();
            textBoxPart.Select(textBoxPart.Text.Length, 0);
            return textBoxPart.IsKeyboardFocused || textBoxPart.IsKeyboardFocusWithin;
        }

        return base.Focus();
    }

    public void Blur()
    {
        SetCurrentValue(IsDropDownOpenProperty, false);
        Keyboard.ClearFocus();
    }

    public void ClearSelection()
    {
        var oldText = Text;
        var oldValue = Value;

        SelectedItem = null;
        committingSelection = true;
        try
        {
            SetCurrentValue(TextProperty, string.Empty);
            SetCurrentValue(ValueProperty, null);
        }
        finally
        {
            committingSelection = false;
        }

        SetCurrentValue(IsDropDownOpenProperty, false);
        UpdateVisualState();
        SyncListBoxSelection();
        RaiseEvent(new RoutedEventArgs(ClearEvent, this));
        RaiseEvent(new NAutoCompleteValueChangedEventArgs(ValueChangedEvent, this, oldValue, null, oldText, string.Empty));
    }

    private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NAutoComplete autoComplete)
        {
            return;
        }

        autoComplete.AttachItemsSourceCollectionChanged(e.NewValue as IEnumerable);
        autoComplete.RefreshOptions();
        autoComplete.TryResolveSelectionFromValue();
        autoComplete.CloseDropDownIfCannotDisplay();
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NAutoComplete autoComplete)
        {
            return;
        }

        var oldText = e.OldValue as string ?? string.Empty;
        var newText = e.NewValue as string ?? string.Empty;

        autoComplete.UpdateTextBoxText();
        autoComplete.UpdateVisualState();

        var shouldRefreshSuggestionsInternally = autoComplete.ItemsSource is null || autoComplete.EnableInternalFilter;
        if (shouldRefreshSuggestionsInternally)
        {
            autoComplete.RefreshOptions();
        }
        else
        {
            autoComplete.UpdateCompletionState();
            autoComplete.SyncListBoxSelection();
        }

        if (!autoComplete.committingSelection)
        {
            autoComplete.ClearCommittedSelectionIfTextDiverged(newText);
        }

        if (autoComplete.committingSelection || autoComplete.suppressAutoOpen)
        {
            autoComplete.SetCurrentValue(IsDropDownOpenProperty, false);
        }
        else if (autoComplete.textUpdateOriginatedFromInnerTextBox)
        {
            autoComplete.UpdateDropDownState(forceOpen: autoComplete.OpenOnInput && !string.IsNullOrWhiteSpace(newText));
        }
        else
        {
            autoComplete.CloseDropDownIfCannotDisplay();
        }

        if (!autoComplete.committingSelection && !string.Equals(oldText, newText, StringComparison.Ordinal))
        {
            autoComplete.RaiseEvent(new NAutoCompleteValueChangedEventArgs(
                ValueChangedEvent,
                autoComplete,
                autoComplete.Value,
                autoComplete.Value,
                oldText,
                newText));
        }

        autoComplete.textUpdateOriginatedFromInnerTextBox = false;
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NAutoComplete autoComplete)
        {
            return;
        }

        if (!autoComplete.committingSelection)
        {
            autoComplete.TryResolveSelectionFromValue();
        }
        else
        {
            autoComplete.SyncListBoxSelection();
        }

        autoComplete.UpdateVisualState();
    }

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NAutoComplete autoComplete)
        {
            return;
        }

        autoComplete.UpdateResolvedMetrics();
        autoComplete.UpdatePopupMetrics();
        autoComplete.UpdateVisualState();
        autoComplete.CloseDropDownIfCannotDisplay();
    }

    private static void OnPopupPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NAutoComplete autoComplete)
        {
            autoComplete.UpdatePopupMetrics();
        }
    }

    private static void OnDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NAutoComplete autoComplete)
        {
            return;
        }

        autoComplete.UpdatePopupMetrics();
        if ((bool)e.NewValue)
        {
            if (openInstance is not null && !ReferenceEquals(openInstance, autoComplete))
            {
                openInstance.SetCurrentValue(IsDropDownOpenProperty, false);
            }

            openInstance = autoComplete;
            autoComplete.SelectFirstSuggestionIfNeeded();
            return;
        }

        if (ReferenceEquals(openInstance, autoComplete))
        {
            openInstance = null;
        }
    }

    private void ExecuteClearCommand(object sender, ExecutedRoutedEventArgs e)
    {
        ClearSelection();
        e.Handled = true;
    }

    private void CanExecuteClearCommand(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = CanClear;
        e.Handled = true;
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
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

    private void HandleOptionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshOptions();
        CloseDropDownIfCannotDisplay();
    }

    private void HandleItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshOptions();
        CloseDropDownIfCannotDisplay();
    }

    private void HandleTextBoxGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (Trigger != NAutoCompleteTrigger.Manual && (OpenOnFocus || Trigger == NAutoCompleteTrigger.Focus))
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, () =>
            {
                if (textBoxPart?.IsKeyboardFocusWithin == true)
                {
                    UpdateDropDownState(forceOpen: true);
                }
            });
        }

        UpdateCompletionState();
    }

    private void HandleTextBoxLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.Input, () =>
        {
            if (!IsKeyboardFocusWithin)
            {
                SetCurrentValue(IsDropDownOpenProperty, false);
            }
        });
    }

    private void HandleClearButtonClick(object sender, RoutedEventArgs e)
    {
        suppressAutoOpen = true;
        try
        {
            ClearSelection();
            textBoxPart?.Focus();
            textBoxPart?.ScrollToHorizontalOffset(double.MaxValue);
        }
        finally
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, () => suppressAutoOpen = false);
        }

        e.Handled = true;
    }

    private void HandleArrowButtonClick(object sender, RoutedEventArgs e)
    {
        if (!IsEnabled)
        {
            return;
        }

        var nextOpenState = !IsDropDownOpen && CanDisplayPopupContent();
        SetCurrentValue(IsDropDownOpenProperty, nextOpenState);

        if (nextOpenState && textBoxPart is not null && !textBoxPart.IsKeyboardFocusWithin)
        {
            textBoxPart.Focus();
        }

        e.Handled = true;
    }

    private void HandleInnerTextBoxTextChanged(object sender, TextChangedEventArgs e)
    {
        if (syncingTextBoxText || textBoxPart is null)
        {
            return;
        }

        var nextText = textBoxPart.Text;
        if (!string.Equals(Text, nextText, StringComparison.Ordinal))
        {
            ScrollTextBoxToCaret();
            textUpdateOriginatedFromInnerTextBox = true;
            SetCurrentValue(TextProperty, nextText);
            return;
        }

        if (!suppressAutoOpen && !textUpdateOriginatedFromInnerTextBox && Trigger != NAutoCompleteTrigger.Manual)
        {
            UpdateDropDownState(forceOpen: OpenOnInput && !string.IsNullOrWhiteSpace(nextText));
        }

        if (!textUpdateOriginatedFromInnerTextBox)
        {
            UpdateCompletionState();
        }
    }

    private void HandleTextBoxSelectionChanged(object sender, RoutedEventArgs e)
    {
        UpdateCompletionState();
    }

    private void HandleTextBoxSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateCompletionState();
    }

    private void HandleListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (syncingSuggestionSelection || listBoxPart?.SelectedItem is not NSelectOption option)
        {
            return;
        }

        SelectedItem = option;
    }

    private void HandleListBoxPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var item = FindAncestor<ListBoxItem>(e.OriginalSource as DependencyObject);
        if (item?.DataContext is not NSelectOption option || option.Disabled)
        {
            return;
        }

        CommitSelection(option);
        e.Handled = true;
    }

    private void HandleListBoxPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        ScrollDropDown(e);
    }

    private void ScrollDropDown(MouseWheelEventArgs e)
    {
        if (listBoxPart is null)
        {
            return;
        }

        var scrollViewer = FindDescendant<ScrollViewer>(listBoxPart);
        if (scrollViewer is null || scrollViewer.ScrollableHeight <= 0d)
        {
            return;
        }

        if (e.Delta < 0)
        {
            scrollViewer.LineDown();
        }
        else
        {
            scrollViewer.LineUp();
        }

        e.Handled = true;
    }

    private void RefreshOptions()
    {
        var options = BuildOptions();
        var nextOptions = EnableInternalFilter ? FilterOptions(options) : options;

        FilteredOptions = [.. nextOptions];
        HasFilteredOptions = FilteredOptions.Count > 0;
        UpdateCompletionState();
        SyncListBoxSelection();
        if (IsDropDownOpen)
        {
            SelectFirstSuggestionIfNeeded();
        }

        UpdateVisualState();
    }

    private List<NSelectOption> BuildOptions()
    {
        if (ItemsSource is null)
        {
            foreach (var option in Options)
            {
                option.Source ??= option;
            }

            return Options.Where(option => option.Show).ToList();
        }

        var options = new List<NSelectOption>();
        foreach (var item in ItemsSource)
        {
            if (item is NSelectOption option)
            {
                if (option.Show)
                {
                    option.Source ??= option;
                    options.Add(option);
                }

                continue;
            }

            var label = ResolveMemberValue(item, DisplayMemberPath) ?? item;
            var value = string.IsNullOrWhiteSpace(ValueMemberPath) ? label : ResolveMemberValue(item, ValueMemberPath);
            options.Add(new NSelectOption
            {
                Label = label,
                Value = value,
                Source = item
            });
        }

        return options;
    }

    private List<NSelectOption> FilterOptions(IEnumerable<NSelectOption> options)
    {
        var search = Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(search))
        {
            return options.ToList();
        }

        return options.Where(option => IsOptionMatched(option, search)).ToList();
    }

    private bool IsOptionMatched(NSelectOption option, string search)
    {
        var comparison = IgnoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;
        var optionText = GetOptionText(option);
        var valueText = Convert.ToString(option.Value, CultureInfo.CurrentCulture) ?? string.Empty;

        return MatchMode switch
        {
            NAutoCompleteMatchMode.StartsWith => optionText.StartsWith(search, comparison) || valueText.StartsWith(search, comparison),
            _ => optionText.Contains(search, comparison) || valueText.Contains(search, comparison)
        };
    }

    private void UpdateResolvedMetrics()
    {
        var (height, fontSize, optionHeight, padding) = Size switch
        {
            NSelectSize.Tiny => (22d, 12d, 28d, new Thickness(8, 0, 6, 0)),
            NSelectSize.Small => (28d, 13d, 30d, new Thickness(10, 0, 8, 0)),
            NSelectSize.Large => (40d, 15d, 40d, new Thickness(14, 0, 10, 0)),
            _ => (34d, 14d, 34d, new Thickness(12, 0, 8, 0))
        };

        ResolvedHeight = height;
        ResolvedFontSize = fontSize;
        EffectiveFontSize = ReadLocalValue(FontSizeProperty) == DependencyProperty.UnsetValue ? fontSize : FontSize;
        ResolvedOptionHeight = optionHeight;
        ResolvedPadding = padding;
    }

    private void UpdatePopupMetrics()
    {
        ResolvedPopupPlacement = PlacementMode.Custom;

        ResolvedDropDownWidth = double.IsNaN(DropDownWidth) || DropDownWidth <= 0d
            ? Math.Max(ActualWidth, MinWidth)
            : DropDownWidth;

        if (popupPart is not null)
        {
            popupPart.CustomPopupPlacementCallback = PlacePopup;
        }
    }

    private CustomPopupPlacement[] PlacePopup(Size popupSize, Size targetSize, Point offset)
    {
        var verticalGap = 6d;
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
            NSelectPlacement.Top or NSelectPlacement.TopStart or NSelectPlacement.TopEnd => -popupSize.Height - verticalGap,
            _ => targetSize.Height + verticalGap
        };

        var primaryAxis = Placement switch
        {
            NSelectPlacement.Top or NSelectPlacement.TopStart or NSelectPlacement.TopEnd => PopupPrimaryAxis.Vertical,
            _ => PopupPrimaryAxis.Vertical
        };

        return [new CustomPopupPlacement(new Point(x + offset.X, y + offset.Y), primaryAxis)];
    }

    private void UpdateVisualState()
    {
        HasHeaderContent = HeaderContent is not null || ActionContent is not null;
        HasActionContent = ActionContent is not null;
        HasPrefixContent = PrefixContent is not null;
        HasSuffixContent = SuffixContent is not null;
        HasArrowContent = ArrowContent is not null;
        HasClearIconContent = ClearIconContent is not null;
        ShowPlaceholder = string.IsNullOrWhiteSpace(Text);
        CanClear = Clearable && !string.IsNullOrWhiteSpace(Text) && IsEnabled;
        HasTrailingContent = HasSuffixContent || ShowArrow || CanClear;
    }

    private void UpdateTextBoxText()
    {
        if (textBoxPart is null)
        {
            return;
        }

        var targetText = Text ?? string.Empty;
        if (string.Equals(textBoxPart.Text, targetText, StringComparison.Ordinal))
        {
            return;
        }

        syncingTextBoxText = true;
        try
        {
            textBoxPart.Text = targetText;
            textBoxPart.CaretIndex = textBoxPart.Text.Length;
            ScrollTextBoxToCaret();
        }
        finally
        {
            syncingTextBoxText = false;
        }
    }

    private void ClearCommittedSelectionIfTextDiverged(string currentText)
    {
        if (SelectedItem is not NSelectOption selectedOption)
        {
            if (Value is not null)
            {
                SetCurrentValue(ValueProperty, null);
            }

            UpdateCompletionState();
            return;
        }

        var comparison = IgnoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;
        if (string.Equals(GetOptionText(selectedOption), currentText, comparison))
        {
            return;
        }

        SelectedItem = null;
        if (Value is not null)
        {
            SetCurrentValue(ValueProperty, null);
        }

        SyncListBoxSelection();
        UpdateCompletionState();
    }

    private void TryResolveSelectionFromValue()
    {
        if (Value is null)
        {
            SelectedItem = null;
            SyncListBoxSelection();
            return;
        }

        var option = BuildOptions().FirstOrDefault(item => Equals(item.Value, Value));
        SelectedItem = option;
        SyncListBoxSelection();

        if (option is null || !UpdateTextOnSelect)
        {
            UpdateCompletionState();
            return;
        }

        var nextText = GetOptionText(option);
        if (string.Equals(Text, nextText, StringComparison.Ordinal))
        {
            return;
        }

        committingSelection = true;
        try
        {
            SetCurrentValue(TextProperty, nextText);
        }
        finally
        {
            committingSelection = false;
        }

        UpdateCompletionState();
    }

    private void UpdateDropDownState(bool forceOpen = false)
    {
        if (Trigger == NAutoCompleteTrigger.Manual)
        {
            return;
        }

        var shouldOpen = forceOpen
            ? CanDisplayPopupContent()
            : ShouldAutoOpenSuggestions();

        SetCurrentValue(IsDropDownOpenProperty, shouldOpen);
    }

    private void CloseDropDownIfCannotDisplay()
    {
        if (IsDropDownOpen && !CanDisplayPopupContent())
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }
    }

    private void ScrollTextBoxToCaret()
    {
        if (textBoxPart is null)
        {
            return;
        }

        textBoxPart.CaretIndex = textBoxPart.Text.Length;
        textBoxPart.ScrollToHorizontalOffset(double.MaxValue);
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
        {
            if (textBoxPart is not null)
            {
                textBoxPart.CaretIndex = textBoxPart.Text.Length;
                textBoxPart.ScrollToHorizontalOffset(double.MaxValue);
            }
        });
    }

    private bool ShouldAutoOpenSuggestions()
    {
        if (!CanDisplayPopupContent())
        {
            return false;
        }

        if (!CanShowByPredicate())
        {
            return false;
        }

        if (!(textBoxPart?.IsKeyboardFocusWithin == true || IsKeyboardFocusWithin))
        {
            return false;
        }

        return Trigger switch
        {
            NAutoCompleteTrigger.Focus => true,
            NAutoCompleteTrigger.Input => (!string.IsNullOrWhiteSpace(Text) || OpenOnFocus),
            _ => false
        };
    }

    private bool CanDisplayPopupContent()
    {
        if (!IsEnabled)
        {
            return false;
        }

        if (!CanShowByPredicate())
        {
            return false;
        }

        if (Loading)
        {
            return true;
        }

        if (HasFilteredOptions)
        {
            return true;
        }

        return ShowEmptyWhenNoMatch;
    }

    private bool CanShowByPredicate()
    {
        return GetShow?.Invoke(Text ?? string.Empty) ?? true;
    }

    private void EnsureDropDownOpenForKeyboard()
    {
        if (Trigger == NAutoCompleteTrigger.Manual)
        {
            return;
        }

        if (!IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, CanDisplayPopupContent());
        }
    }

    private void MoveSuggestion(int delta)
    {
        if (listBoxPart is null || FilteredOptions.Count == 0)
        {
            return;
        }

        var currentIndex = listBoxPart.SelectedIndex;
        var nextIndex = currentIndex;
        var guard = 0;

        do
        {
            nextIndex = nextIndex < 0
                ? (delta > 0 ? 0 : FilteredOptions.Count - 1)
                : (nextIndex + delta + FilteredOptions.Count) % FilteredOptions.Count;
            guard++;
        } while (guard <= FilteredOptions.Count && FilteredOptions[nextIndex].Disabled);

        if (guard > FilteredOptions.Count)
        {
            return;
        }

        syncingSuggestionSelection = true;
        try
        {
            listBoxPart.SelectedIndex = nextIndex;
            listBoxPart.ScrollIntoView(listBoxPart.SelectedItem);
        }
        finally
        {
            syncingSuggestionSelection = false;
        }

        if (listBoxPart.SelectedItem is NSelectOption option)
        {
            SelectedItem = option;
        }
    }

    private void SelectFirstSuggestionIfNeeded()
    {
        if (!SelectFirstOptionOnOpen || listBoxPart is null || FilteredOptions.Count == 0)
        {
            return;
        }

        var first = FilteredOptions.FirstOrDefault(option => !option.Disabled);
        if (first is null)
        {
            return;
        }

        syncingSuggestionSelection = true;
        try
        {
            listBoxPart.SelectedItem = first;
            listBoxPart.ScrollIntoView(first);
        }
        finally
        {
            syncingSuggestionSelection = false;
        }

        SelectedItem = first;
    }

    private bool TryCommitHighlightedOption()
    {
        if (SelectedItem is NSelectOption selectedOption && !selectedOption.Disabled)
        {
            CommitSelection(selectedOption);
            return true;
        }

        if (FilteredOptions.Count == 1 && !FilteredOptions[0].Disabled)
        {
            CommitSelection(FilteredOptions[0]);
            return true;
        }

        return false;
    }

    private void CommitSelection(NSelectOption option)
    {
        var oldText = Text;
        var oldValue = Value;
        var selectedText = GetOptionText(option);
        var committedText = ClearAfterSelect ? string.Empty : selectedText;

        committingSelection = true;
        try
        {
            SelectedItem = option;

            if (UpdateTextOnSelect)
            {
                SetCurrentValue(TextProperty, committedText);
            }

            SetCurrentValue(ValueProperty, option.Value);
        }
        finally
        {
            committingSelection = false;
        }

        SyncListBoxSelection();
        RaiseEvent(new NAutoCompleteOptionSelectedEventArgs(OptionSelectedEvent, this, option));

        if (CloseOnSelect)
        {
            suppressAutoOpen = true;
            SetCurrentValue(IsDropDownOpenProperty, false);
            Dispatcher.BeginInvoke(DispatcherPriority.Input, () => suppressAutoOpen = false);
        }

        RaiseEvent(new NAutoCompleteValueChangedEventArgs(
            ValueChangedEvent,
            this,
            oldValue,
            option.Value,
            oldText,
            committedText));

        UpdateCompletionState();

        if (textBoxPart is not null)
        {
            if (BlurAfterSelect)
            {
                Blur();
            }
            else
            {
                textBoxPart.Focus();
                textBoxPart.Select(textBoxPart.Text.Length, 0);
            }
        }
    }

    private void SyncListBoxSelection()
    {
        if (listBoxPart is null)
        {
            return;
        }

        syncingSuggestionSelection = true;
        try
        {
            if (SelectedItem is NSelectOption selectedOption)
            {
                listBoxPart.SelectedItem = FilteredOptions.FirstOrDefault(option => Equals(option.Value, selectedOption.Value));
            }
            else
            {
                listBoxPart.SelectedItem = null;
            }
        }
        finally
        {
            syncingSuggestionSelection = false;
        }
    }

    private void AttachItemsSourceCollectionChanged(IEnumerable? itemsSource)
    {
        if (ReferenceEquals(itemsSourceNotifier, itemsSource))
        {
            return;
        }

        if (itemsSourceNotifier is not null)
        {
            itemsSourceNotifier.CollectionChanged -= HandleItemsSourceCollectionChanged;
            itemsSourceNotifier = null;
        }

        if (itemsSource is INotifyCollectionChanged notifier)
        {
            itemsSourceNotifier = notifier;
            itemsSourceNotifier.CollectionChanged += HandleItemsSourceCollectionChanged;
        }
    }

    private static object? ResolveMemberValue(object? source, string path)
    {
        if (source is null || string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        object? current = source;
        foreach (var segment in path.Split('.'))
        {
            if (current is null)
            {
                return null;
            }

            var property = TypeDescriptor.GetProperties(current)[segment];
            if (property is not null)
            {
                current = property.GetValue(current);
                continue;
            }

            var field = current.GetType().GetField(segment);
            current = field?.GetValue(current);
        }

        return current;
    }

    private string GetOptionText(NSelectOption option)
    {
        return Convert.ToString(option.Label ?? option.Value, CultureInfo.CurrentCulture) ?? string.Empty;
    }

    private void UpdateCompletionState()
    {
        CompletionSuffix = string.Empty;
        HasCompletionSuffix = false;
        CompletionSuffixOffset = 0d;

        if (!ShowCompletionHint)
        {
            return;
        }

        var text = Text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var option = FindCompletionOption(text);
        if (option is null)
        {
            return;
        }

        var optionText = GetOptionText(option);
        if (optionText.Length <= text.Length)
        {
            return;
        }

        if (textBoxPart is null || !textBoxPart.IsKeyboardFocusWithin)
        {
            return;
        }

        if (textBoxPart.SelectionLength > 0 || textBoxPart.CaretIndex != text.Length)
        {
            return;
        }

        if (IsTextOverflowing(optionText))
        {
            return;
        }

        CompletionSuffixOffset = MeasureTextWidth(text);
        CompletionSuffix = optionText[text.Length..];
        HasCompletionSuffix = !string.IsNullOrEmpty(CompletionSuffix);
    }

    private NSelectOption? FindCompletionOption(string text)
    {
        var comparison = IgnoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;
        return FilteredOptions.FirstOrDefault(option =>
            !option.Disabled &&
            GetOptionText(option).StartsWith(text, comparison) &&
            !string.Equals(GetOptionText(option), text, comparison));
    }

    private bool TryAcceptCompletionHint()
    {
        var text = Text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var option = SelectedItem as NSelectOption ?? FindCompletionOption(text);
        if (option is null)
        {
            return false;
        }

        var optionText = GetOptionText(option);
        var comparison = IgnoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;
        if (!optionText.StartsWith(text, comparison))
        {
            return false;
        }

        CommitSelection(option);
        return true;
    }

    private bool IsTextOverflowing(string text)
    {
        if (textBoxPart is null || textBoxPart.ActualWidth <= 0)
        {
            return false;
        }

        var formattedText = CreateFormattedText(text);
        return formattedText.WidthIncludingTrailingWhitespace > Math.Max(0d, textBoxPart.ActualWidth - 2d);
    }

    private double MeasureTextWidth(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0d;
        }

        return CreateFormattedText(text).WidthIncludingTrailingWhitespace;
    }

    private FormattedText CreateFormattedText(string text)
    {
        if (textBoxPart is null)
        {
            return new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                EffectiveFontSize,
                Brushes.Transparent,
                1d);
        }

        var dpi = VisualTreeHelper.GetDpi(textBoxPart);
        return new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(textBoxPart.FontFamily, textBoxPart.FontStyle, textBoxPart.FontWeight, textBoxPart.FontStretch),
            textBoxPart.FontSize,
            Brushes.Transparent,
            dpi.PixelsPerDip);
    }

    private static T? FindDescendant<T>(DependencyObject? source)
        where T : DependencyObject
    {
        if (source is null)
        {
            return null;
        }

        var count = VisualTreeHelper.GetChildrenCount(source);
        for (var index = 0; index < count; index++)
        {
            var child = VisualTreeHelper.GetChild(source, index);
            if (child is T match)
            {
                return match;
            }

            var descendant = FindDescendant<T>(child);
            if (descendant is not null)
            {
                return descendant;
            }
        }

        return null;
    }

    private static T? FindAncestor<T>(DependencyObject? source)
        where T : DependencyObject
    {
        while (source is not null)
        {
            if (source is T match)
            {
                return match;
            }

            source = source switch
            {
                Visual visual => VisualTreeHelper.GetParent(visual),
                System.Windows.Media.Media3D.Visual3D visual3D => VisualTreeHelper.GetParent(visual3D),
                FrameworkContentElement contentElement => contentElement.Parent,
                _ => LogicalTreeHelper.GetParent(source)
            };
        }

        return null;
    }

    private bool IsDescendantOfTextBox(DependencyObject? source)
    {
        while (source is not null)
        {
            if (ReferenceEquals(source, textBoxPart))
            {
                return true;
            }

            source = source switch
            {
                Visual visual => VisualTreeHelper.GetParent(visual),
                System.Windows.Media.Media3D.Visual3D visual3D => VisualTreeHelper.GetParent(visual3D),
                FrameworkContentElement contentElement => contentElement.Parent,
                _ => LogicalTreeHelper.GetParent(source)
            };
        }

        return false;
    }
}

public sealed class NAutoCompleteValueChangedEventArgs : RoutedEventArgs
{
    public NAutoCompleteValueChangedEventArgs(RoutedEvent routedEvent, object source, object? oldValue, object? newValue, string oldText, string newText)
        : base(routedEvent, source)
    {
        OldValue = oldValue;
        NewValue = newValue;
        OldText = oldText;
        NewText = newText;
    }

    public object? OldValue { get; }

    public object? NewValue { get; }

    public string OldText { get; }

    public string NewText { get; }
}

public sealed class NAutoCompleteOptionSelectedEventArgs : RoutedEventArgs
{
    public NAutoCompleteOptionSelectedEventArgs(RoutedEvent routedEvent, object source, NSelectOption option)
        : base(routedEvent, source)
    {
        Option = option;
    }

    public NSelectOption Option { get; }
}
