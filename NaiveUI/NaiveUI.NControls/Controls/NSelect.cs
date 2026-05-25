using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public enum NSelectSize
{
    Tiny,
    Small,
    Medium,
    Large
}

public enum NSelectStatus
{
    Default,
    Success,
    Warning,
    Error
}

public enum NSelectPlacement
{
    BottomStart,
    Bottom,
    BottomEnd,
    TopStart,
    Top,
    TopEnd
}

[ContentProperty(nameof(Options))]
public class NSelect : Control
{
    private const string PopupPartName = "PART_Popup";
    private const string SearchBoxPartName = "PART_SearchBox";
    private const string ListBoxPartName = "PART_ListBox";
    private Popup? popupPart;
    private TextBox? searchBoxPart;
    private ListBox? listBoxPart;
    private Window? ownerWindow;
    private INotifyCollectionChanged? selectedValuesNotifier;
    private bool syncingSelection;
    private bool handlingPointerSelection;
    private bool suppressSelectedValuesCollectionSync;
    private bool pendingOpenOnMouseUp;

    static NSelect()
    {
        ElementBase.DefaultStyle<NSelect>(DefaultStyleKeyProperty);
    }

    public NSelect()
    {
        CommandBindings.Add(new CommandBinding(ClearSelectionCommand, HandleClearSelectionCommand, CanExecuteClearSelectionCommand));
        CommandBindings.Add(new CommandBinding(RemoveSelectedValueCommand, HandleRemoveSelectedValueCommand, CanExecuteRemoveSelectedValueCommand));
        Options.CollectionChanged += HandleOptionsCollectionChanged;
        FilteredOptions = [];
        SelectedOptionLabels = [];
        AttachSelectedValuesCollectionChanged(SelectedValues);
        UpdateResolvedMetrics();
        UpdatePopupMetrics();
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
        DependencyProperty.RegisterReadOnly(nameof(FilteredOptions), typeof(ObservableCollection<NSelectOption>), typeof(NSelect), new PropertyMetadata(null));

    public static readonly DependencyProperty FilteredOptionsProperty = FilteredOptionsPropertyKey.DependencyProperty;

    public ObservableCollection<NSelectTagItem> SelectedOptionLabels
    {
        get => (ObservableCollection<NSelectTagItem>)GetValue(SelectedOptionLabelsProperty);
        private set => SetValue(SelectedOptionLabelsPropertyKey, value);
    }

    private static readonly DependencyPropertyKey SelectedOptionLabelsPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(SelectedOptionLabels), typeof(ObservableCollection<NSelectTagItem>), typeof(NSelect), new PropertyMetadata(null));

    public static readonly DependencyProperty SelectedOptionLabelsProperty = SelectedOptionLabelsPropertyKey.DependencyProperty;

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty ItemsSourceProperty =
        ElementBase.Property<NSelect, IEnumerable?>(nameof(ItemsSourceProperty), null, OnOptionsSourceChanged);

    public string DisplayMemberPath
    {
        get => (string)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    public static readonly DependencyProperty DisplayMemberPathProperty =
        ElementBase.Property<NSelect, string>(nameof(DisplayMemberPathProperty), string.Empty, OnOptionsSourceChanged);

    public string ValueMemberPath
    {
        get => (string)GetValue(ValueMemberPathProperty);
        set => SetValue(ValueMemberPathProperty, value);
    }

    public static readonly DependencyProperty ValueMemberPathProperty =
        ElementBase.Property<NSelect, string>(nameof(ValueMemberPathProperty), string.Empty, OnOptionsSourceChanged);

    public object? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    public static readonly DependencyProperty SelectedValueProperty =
        DependencyProperty.Register(
            nameof(SelectedValue),
            typeof(object),
            typeof(NSelect),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedValueChanged));

    public IList? SelectedValues
    {
        get => (IList?)GetValue(SelectedValuesProperty);
        set => SetValue(SelectedValuesProperty, value);
    }

    public static readonly DependencyProperty SelectedValuesProperty =
        DependencyProperty.Register(
            nameof(SelectedValues),
            typeof(IList),
            typeof(NSelect),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedValuesChanged));

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        private set => SetValue(SelectedItemPropertyKey, value);
    }

    private static readonly DependencyPropertyKey SelectedItemPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(SelectedItem), typeof(object), typeof(NSelect), new PropertyMetadata(null));

    public static readonly DependencyProperty SelectedItemProperty = SelectedItemPropertyKey.DependencyProperty;

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public static readonly DependencyProperty PlaceholderProperty =
        ElementBase.Property<NSelect, string>(nameof(PlaceholderProperty), "请选择");

    public string SearchText
    {
        get => (string)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public static readonly DependencyProperty SearchTextProperty =
        DependencyProperty.Register(
            nameof(SearchText),
            typeof(string),
            typeof(NSelect),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSearchTextChanged));

    public bool Filterable
    {
        get => (bool)GetValue(FilterableProperty);
        set => SetValue(FilterableProperty, value);
    }

    public static readonly DependencyProperty FilterableProperty =
        ElementBase.Property<NSelect, bool>(nameof(FilterableProperty), false, OnVisualPropertyChanged);

    public bool Clearable
    {
        get => (bool)GetValue(ClearableProperty);
        set => SetValue(ClearableProperty, value);
    }

    public static readonly DependencyProperty ClearableProperty =
        ElementBase.Property<NSelect, bool>(nameof(ClearableProperty), false, OnVisualPropertyChanged);

    public bool Multiple
    {
        get => (bool)GetValue(MultipleProperty);
        set => SetValue(MultipleProperty, value);
    }

    public static readonly DependencyProperty MultipleProperty =
        ElementBase.Property<NSelect, bool>(nameof(MultipleProperty), false, OnMultipleChanged);

    public bool Loading
    {
        get => (bool)GetValue(LoadingProperty);
        set => SetValue(LoadingProperty, value);
    }

    public static readonly DependencyProperty LoadingProperty =
        ElementBase.Property<NSelect, bool>(nameof(LoadingProperty), false, OnVisualPropertyChanged);

    public bool Remote
    {
        get => (bool)GetValue(RemoteProperty);
        set => SetValue(RemoteProperty, value);
    }

    public static readonly DependencyProperty RemoteProperty =
        ElementBase.Property<NSelect, bool>(nameof(RemoteProperty), false);

    public bool AllowCreate
    {
        get => (bool)GetValue(AllowCreateProperty);
        set => SetValue(AllowCreateProperty, value);
    }

    public static readonly DependencyProperty AllowCreateProperty =
        ElementBase.Property<NSelect, bool>(nameof(AllowCreateProperty), false);

    public bool HideSelected
    {
        get => (bool)GetValue(HideSelectedProperty);
        set => SetValue(HideSelectedProperty, value);
    }

    public static readonly DependencyProperty HideSelectedProperty =
        ElementBase.Property<NSelect, bool>(nameof(HideSelectedProperty), false, OnOptionsSourceChanged);

    public bool CloseOnSelect
    {
        get => (bool)GetValue(CloseOnSelectProperty);
        set => SetValue(CloseOnSelectProperty, value);
    }

    public static readonly DependencyProperty CloseOnSelectProperty =
        ElementBase.Property<NSelect, bool>(nameof(CloseOnSelectProperty), true);

    public bool ShowArrow
    {
        get => (bool)GetValue(ShowArrowProperty);
        set => SetValue(ShowArrowProperty, value);
    }

    public static readonly DependencyProperty ShowArrowProperty =
        ElementBase.Property<NSelect, bool>(nameof(ShowArrowProperty), true);

    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public static readonly DependencyProperty IsDropDownOpenProperty =
        DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(NSelect),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDropDownOpenChanged));

    public NSelectSize Size
    {
        get => (NSelectSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public static readonly DependencyProperty SizeProperty =
        ElementBase.Property<NSelect, NSelectSize>(nameof(SizeProperty), NSelectSize.Medium, OnVisualPropertyChanged);

    public NSelectStatus Status
    {
        get => (NSelectStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public static readonly DependencyProperty StatusProperty =
        ElementBase.Property<NSelect, NSelectStatus>(nameof(StatusProperty), NSelectStatus.Default);

    public bool IsInvalid
    {
        get => (bool)GetValue(IsInvalidProperty);
        set => SetValue(IsInvalidProperty, value);
    }

    public static readonly DependencyProperty IsInvalidProperty =
        ElementBase.Property<NSelect, bool>(nameof(IsInvalidProperty), false);

    public NSelectPlacement Placement
    {
        get => (NSelectPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public static readonly DependencyProperty PlacementProperty =
        ElementBase.Property<NSelect, NSelectPlacement>(nameof(PlacementProperty), NSelectPlacement.BottomStart, OnPopupPropertyChanged);

    public double MaxDropDownHeight
    {
        get => (double)GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    public static readonly DependencyProperty MaxDropDownHeightProperty =
        ElementBase.Property<NSelect, double>(nameof(MaxDropDownHeightProperty), 280d);

    public double DropDownWidth
    {
        get => (double)GetValue(DropDownWidthProperty);
        set => SetValue(DropDownWidthProperty, value);
    }

    public static readonly DependencyProperty DropDownWidthProperty =
        ElementBase.Property<NSelect, double>(nameof(DropDownWidthProperty), double.NaN, OnPopupPropertyChanged);

    public int MaxTagCount
    {
        get => (int)GetValue(MaxTagCountProperty);
        set => SetValue(MaxTagCountProperty, value);
    }

    public static readonly DependencyProperty MaxTagCountProperty =
        ElementBase.Property<NSelect, int>(nameof(MaxTagCountProperty), 0, OnVisualPropertyChanged);

    public object? PrefixContent
    {
        get => GetValue(PrefixContentProperty);
        set => SetValue(PrefixContentProperty, value);
    }

    public static readonly DependencyProperty PrefixContentProperty =
        ElementBase.Property<NSelect, object?>(nameof(PrefixContentProperty), null);

    public object? SuffixContent
    {
        get => GetValue(SuffixContentProperty);
        set => SetValue(SuffixContentProperty, value);
    }

    public static readonly DependencyProperty SuffixContentProperty =
        ElementBase.Property<NSelect, object?>(nameof(SuffixContentProperty), null);

    public object? ArrowContent
    {
        get => GetValue(ArrowContentProperty);
        set => SetValue(ArrowContentProperty, value);
    }

    public static readonly DependencyProperty ArrowContentProperty =
        ElementBase.Property<NSelect, object?>(nameof(ArrowContentProperty), null, OnVisualPropertyChanged);

    public object? ClosedArrowContent
    {
        get => GetValue(ClosedArrowContentProperty);
        set => SetValue(ClosedArrowContentProperty, value);
    }

    public static readonly DependencyProperty ClosedArrowContentProperty =
        ElementBase.Property<NSelect, object?>(nameof(ClosedArrowContentProperty), null, OnVisualPropertyChanged);

    public object? OpenArrowContent
    {
        get => GetValue(OpenArrowContentProperty);
        set => SetValue(OpenArrowContentProperty, value);
    }

    public static readonly DependencyProperty OpenArrowContentProperty =
        ElementBase.Property<NSelect, object?>(nameof(OpenArrowContentProperty), null, OnVisualPropertyChanged);

    public object? ClearIconContent
    {
        get => GetValue(ClearIconContentProperty);
        set => SetValue(ClearIconContentProperty, value);
    }

    public static readonly DependencyProperty ClearIconContentProperty =
        ElementBase.Property<NSelect, object?>(nameof(ClearIconContentProperty), null, OnVisualPropertyChanged);

    public object? HeaderContent
    {
        get => GetValue(HeaderContentProperty);
        set => SetValue(HeaderContentProperty, value);
    }

    public static readonly DependencyProperty HeaderContentProperty =
        ElementBase.Property<NSelect, object?>(nameof(HeaderContentProperty), null, OnVisualPropertyChanged);

    public object? ActionContent
    {
        get => GetValue(ActionContentProperty);
        set => SetValue(ActionContentProperty, value);
    }

    public static readonly DependencyProperty ActionContentProperty =
        ElementBase.Property<NSelect, object?>(nameof(ActionContentProperty), null, OnVisualPropertyChanged);

    public DataTemplate? OptionTemplate
    {
        get => (DataTemplate?)GetValue(OptionTemplateProperty);
        set => SetValue(OptionTemplateProperty, value);
    }

    public static readonly DependencyProperty OptionTemplateProperty =
        ElementBase.Property<NSelect, DataTemplate?>(nameof(OptionTemplateProperty), null);

    public DataTemplate? SelectedItemTemplate
    {
        get => (DataTemplate?)GetValue(SelectedItemTemplateProperty);
        set => SetValue(SelectedItemTemplateProperty, value);
    }

    public static readonly DependencyProperty SelectedItemTemplateProperty =
        ElementBase.Property<NSelect, DataTemplate?>(nameof(SelectedItemTemplateProperty), null);

    public DataTemplate? TagTemplate
    {
        get => (DataTemplate?)GetValue(TagTemplateProperty);
        set => SetValue(TagTemplateProperty, value);
    }

    public static readonly DependencyProperty TagTemplateProperty =
        ElementBase.Property<NSelect, DataTemplate?>(nameof(TagTemplateProperty), null);

    public object? NoDataContent
    {
        get => GetValue(NoDataContentProperty);
        set => SetValue(NoDataContentProperty, value);
    }

    public static readonly DependencyProperty NoDataContentProperty =
        ElementBase.Property<NSelect, object?>(nameof(NoDataContentProperty), "无数据");

    public object? LoadingContent
    {
        get => GetValue(LoadingContentProperty);
        set => SetValue(LoadingContentProperty, value);
    }

    public static readonly DependencyProperty LoadingContentProperty =
        ElementBase.Property<NSelect, object?>(nameof(LoadingContentProperty), "加载中");

    public double ResolvedHeight
    {
        get => (double)GetValue(ResolvedHeightProperty);
        private set => SetValue(ResolvedHeightPropertyKey, value);
    }

    private static readonly DependencyPropertyKey ResolvedHeightPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ResolvedHeight), typeof(double), typeof(NSelect), new PropertyMetadata(34d));

    public static readonly DependencyProperty ResolvedHeightProperty = ResolvedHeightPropertyKey.DependencyProperty;

    public double ResolvedFontSize
    {
        get => (double)GetValue(ResolvedFontSizeProperty);
        private set => SetValue(ResolvedFontSizePropertyKey, value);
    }

    private static readonly DependencyPropertyKey ResolvedFontSizePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ResolvedFontSize), typeof(double), typeof(NSelect), new PropertyMetadata(14d));

    public static readonly DependencyProperty ResolvedFontSizeProperty = ResolvedFontSizePropertyKey.DependencyProperty;

    public Thickness ResolvedPadding
    {
        get => (Thickness)GetValue(ResolvedPaddingProperty);
        private set => SetValue(ResolvedPaddingPropertyKey, value);
    }

    private static readonly DependencyPropertyKey ResolvedPaddingPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ResolvedPadding), typeof(Thickness), typeof(NSelect), new PropertyMetadata(new Thickness(12, 0, 32, 0)));

    public static readonly DependencyProperty ResolvedPaddingProperty = ResolvedPaddingPropertyKey.DependencyProperty;

    public double ResolvedOptionHeight
    {
        get => (double)GetValue(ResolvedOptionHeightProperty);
        private set => SetValue(ResolvedOptionHeightPropertyKey, value);
    }

    private static readonly DependencyPropertyKey ResolvedOptionHeightPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ResolvedOptionHeight), typeof(double), typeof(NSelect), new PropertyMetadata(34d));

    public static readonly DependencyProperty ResolvedOptionHeightProperty = ResolvedOptionHeightPropertyKey.DependencyProperty;

    public bool HasSelection
    {
        get => (bool)GetValue(HasSelectionProperty);
        private set => SetValue(HasSelectionPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasSelectionPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasSelection), typeof(bool), typeof(NSelect), new PropertyMetadata(false));

    public static readonly DependencyProperty HasSelectionProperty = HasSelectionPropertyKey.DependencyProperty;

    public bool HasArrowContent
    {
        get => (bool)GetValue(HasArrowContentProperty);
        private set => SetValue(HasArrowContentPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasArrowContentPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasArrowContent), typeof(bool), typeof(NSelect), new PropertyMetadata(false));

    public static readonly DependencyProperty HasArrowContentProperty = HasArrowContentPropertyKey.DependencyProperty;

    public object? ResolvedClosedArrowContent
    {
        get => GetValue(ResolvedClosedArrowContentProperty);
        private set => SetValue(ResolvedClosedArrowContentPropertyKey, value);
    }

    private static readonly DependencyPropertyKey ResolvedClosedArrowContentPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ResolvedClosedArrowContent), typeof(object), typeof(NSelect), new PropertyMetadata(null));

    public static readonly DependencyProperty ResolvedClosedArrowContentProperty = ResolvedClosedArrowContentPropertyKey.DependencyProperty;

    public object? ResolvedOpenArrowContent
    {
        get => GetValue(ResolvedOpenArrowContentProperty);
        private set => SetValue(ResolvedOpenArrowContentPropertyKey, value);
    }

    private static readonly DependencyPropertyKey ResolvedOpenArrowContentPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ResolvedOpenArrowContent), typeof(object), typeof(NSelect), new PropertyMetadata(null));

    public static readonly DependencyProperty ResolvedOpenArrowContentProperty = ResolvedOpenArrowContentPropertyKey.DependencyProperty;

    public bool HasResolvedClosedArrowContent
    {
        get => (bool)GetValue(HasResolvedClosedArrowContentProperty);
        private set => SetValue(HasResolvedClosedArrowContentPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasResolvedClosedArrowContentPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasResolvedClosedArrowContent), typeof(bool), typeof(NSelect), new PropertyMetadata(false));

    public static readonly DependencyProperty HasResolvedClosedArrowContentProperty = HasResolvedClosedArrowContentPropertyKey.DependencyProperty;

    public bool HasResolvedOpenArrowContent
    {
        get => (bool)GetValue(HasResolvedOpenArrowContentProperty);
        private set => SetValue(HasResolvedOpenArrowContentPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasResolvedOpenArrowContentPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasResolvedOpenArrowContent), typeof(bool), typeof(NSelect), new PropertyMetadata(false));

    public static readonly DependencyProperty HasResolvedOpenArrowContentProperty = HasResolvedOpenArrowContentPropertyKey.DependencyProperty;

    public bool HasClearIconContent
    {
        get => (bool)GetValue(HasClearIconContentProperty);
        private set => SetValue(HasClearIconContentPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasClearIconContentPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasClearIconContent), typeof(bool), typeof(NSelect), new PropertyMetadata(false));

    public static readonly DependencyProperty HasClearIconContentProperty = HasClearIconContentPropertyKey.DependencyProperty;

    public bool HasHeaderContent
    {
        get => (bool)GetValue(HasHeaderContentProperty);
        private set => SetValue(HasHeaderContentPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasHeaderContentPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasHeaderContent), typeof(bool), typeof(NSelect), new PropertyMetadata(false));

    public static readonly DependencyProperty HasHeaderContentProperty = HasHeaderContentPropertyKey.DependencyProperty;

    public bool HasActionContent
    {
        get => (bool)GetValue(HasActionContentProperty);
        private set => SetValue(HasActionContentPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasActionContentPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasActionContent), typeof(bool), typeof(NSelect), new PropertyMetadata(false));

    public static readonly DependencyProperty HasActionContentProperty = HasActionContentPropertyKey.DependencyProperty;

    public bool ShowPlaceholder
    {
        get => (bool)GetValue(ShowPlaceholderProperty);
        private set => SetValue(ShowPlaceholderPropertyKey, value);
    }

    private static readonly DependencyPropertyKey ShowPlaceholderPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ShowPlaceholder), typeof(bool), typeof(NSelect), new PropertyMetadata(true));

    public static readonly DependencyProperty ShowPlaceholderProperty = ShowPlaceholderPropertyKey.DependencyProperty;

    public object? DisplayContent
    {
        get => GetValue(DisplayContentProperty);
        private set => SetValue(DisplayContentPropertyKey, value);
    }

    private static readonly DependencyPropertyKey DisplayContentPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(DisplayContent), typeof(object), typeof(NSelect), new PropertyMetadata(null));

    public static readonly DependencyProperty DisplayContentProperty = DisplayContentPropertyKey.DependencyProperty;

    public bool HasFilteredOptions
    {
        get => (bool)GetValue(HasFilteredOptionsProperty);
        private set => SetValue(HasFilteredOptionsPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasFilteredOptionsPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasFilteredOptions), typeof(bool), typeof(NSelect), new PropertyMetadata(false));

    public static readonly DependencyProperty HasFilteredOptionsProperty = HasFilteredOptionsPropertyKey.DependencyProperty;

    public bool CanClear
    {
        get => (bool)GetValue(CanClearProperty);
        private set => SetValue(CanClearPropertyKey, value);
    }

    private static readonly DependencyPropertyKey CanClearPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(CanClear), typeof(bool), typeof(NSelect), new PropertyMetadata(false));

    public static readonly DependencyProperty CanClearProperty = CanClearPropertyKey.DependencyProperty;

    public PlacementMode ResolvedPopupPlacement
    {
        get => (PlacementMode)GetValue(ResolvedPopupPlacementProperty);
        private set => SetValue(ResolvedPopupPlacementPropertyKey, value);
    }

    private static readonly DependencyPropertyKey ResolvedPopupPlacementPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ResolvedPopupPlacement), typeof(PlacementMode), typeof(NSelect), new PropertyMetadata(PlacementMode.Bottom));

    public static readonly DependencyProperty ResolvedPopupPlacementProperty = ResolvedPopupPlacementPropertyKey.DependencyProperty;

    public double ResolvedDropDownWidth
    {
        get => (double)GetValue(ResolvedDropDownWidthProperty);
        private set => SetValue(ResolvedDropDownWidthPropertyKey, value);
    }

    private static readonly DependencyPropertyKey ResolvedDropDownWidthPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ResolvedDropDownWidth), typeof(double), typeof(NSelect), new PropertyMetadata(0d));

    public static readonly DependencyProperty ResolvedDropDownWidthProperty = ResolvedDropDownWidthPropertyKey.DependencyProperty;

    public static readonly RoutedEvent SelectionChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(SelectionChanged), RoutingStrategy.Bubble, typeof(EventHandler<NSelectSelectionChangedEventArgs>), typeof(NSelect));

    public event EventHandler<NSelectSelectionChangedEventArgs> SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }

    public static readonly RoutedEvent ClearEvent =
        ElementBase.RoutedEvent<NSelect, RoutedEventHandler>(nameof(ClearEvent));

    public static readonly RoutedUICommand ClearSelectionCommand =
        ElementBase.Command<NSelect>(nameof(ClearSelectionCommand));

    public static readonly RoutedUICommand RemoveSelectedValueCommand =
        ElementBase.Command<NSelect>(nameof(RemoveSelectedValueCommand));

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
            listBoxPart.PreviewMouseRightButtonDown -= HandlePartPreviewMouseRightButtonDown;
        }

        if (searchBoxPart is not null)
        {
            searchBoxPart.PreviewMouseRightButtonDown -= HandlePartPreviewMouseRightButtonDown;
        }

        base.OnApplyTemplate();

        popupPart = GetTemplateChild(PopupPartName) as Popup;
        searchBoxPart = GetTemplateChild(SearchBoxPartName) as TextBox;
        listBoxPart = GetTemplateChild(ListBoxPartName) as ListBox;

        if (listBoxPart is not null)
        {
            listBoxPart.SelectionChanged += HandleListBoxSelectionChanged;
            listBoxPart.PreviewMouseLeftButtonDown += HandleListBoxPreviewMouseLeftButtonDown;
            listBoxPart.PreviewMouseRightButtonDown += HandlePartPreviewMouseRightButtonDown;
        }

        if (searchBoxPart is not null)
        {
            searchBoxPart.PreviewMouseRightButtonDown += HandlePartPreviewMouseRightButtonDown;
        }

        SyncListBoxSelection();
        UpdatePopupMetrics();
        FocusSearchBoxIfNeeded();
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        UpdatePopupMetrics();
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);

        if (!IsEnabled || Loading || IsSelectionActionSource(e.OriginalSource as DependencyObject))
        {
            pendingOpenOnMouseUp = false;
            return;
        }

        if (!IsDropDownOpen)
        {
            pendingOpenOnMouseUp = true;
            return;
        }

        pendingOpenOnMouseUp = false;

        if (Filterable && searchBoxPart is not null && !searchBoxPart.IsKeyboardFocusWithin)
        {
            Dispatcher.BeginInvoke(searchBoxPart.Focus);
        }
    }

    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonUp(e);

        if (!pendingOpenOnMouseUp)
        {
            return;
        }

        pendingOpenOnMouseUp = false;

        if (!IsEnabled || Loading || IsSelectionActionSource(e.OriginalSource as DependencyObject) || IsDropDownOpen)
        {
            return;
        }

        SetCurrentValue(IsDropDownOpenProperty, true);
        e.Handled = true;
    }

    protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseRightButtonDown(e);

        if (!IsEnabled || Loading)
        {
            return;
        }

        if (IsSourceWithinSelect(e.OriginalSource as DependencyObject))
        {
            e.Handled = true;
        }
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (e.Key == Key.Escape && IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
            return;
        }

        if ((e.Key == Key.Enter || e.Key == Key.Space) && !IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, true);
            e.Handled = true;
        }
    }

    public void ClearSelection()
    {
        if (!CanClear)
        {
            return;
        }

        var oldValue = Multiple ? CloneList(SelectedValues) : SelectedValue;

        if (Multiple)
        {
            if (SelectedValues is not null)
            {
                suppressSelectedValuesCollectionSync = true;
                try
                {
                    SelectedValues.Clear();
                }
                finally
                {
                    suppressSelectedValuesCollectionSync = false;
                }
            }
        }
        else
        {
            SetCurrentValue(SelectedValueProperty, null);
        }

        SetCurrentValue(SearchTextProperty, string.Empty);
        UpdateSelectionState();
        SyncListBoxSelection();
        RaiseEvent(new RoutedEventArgs(ClearEvent, this));
        RaiseSelectionChanged(oldValue, Multiple ? CloneList(SelectedValues) : SelectedValue);
    }

    public new bool Focus()
    {
        if (!IsEnabled)
        {
            return false;
        }

        if (!IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, true);
        }

        if (Filterable)
        {
            return FocusInput();
        }

        return base.Focus();
    }

    public bool FocusInput()
    {
        if (!IsEnabled)
        {
            return false;
        }

        if (!IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, true);
        }

        if (Filterable && searchBoxPart is not null)
        {
            searchBoxPart.Focus();
            searchBoxPart.Select(searchBoxPart.Text.Length, 0);
            return searchBoxPart.IsKeyboardFocused || searchBoxPart.IsKeyboardFocusWithin;
        }

        return base.Focus();
    }

    public void Blur()
    {
        BlurInput();
    }

    public void BlurInput()
    {
        SetCurrentValue(IsDropDownOpenProperty, false);
        SetCurrentValue(SearchTextProperty, string.Empty);

        if (searchBoxPart is not null)
        {
            searchBoxPart.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
    }

    private void CanExecuteClearSelectionCommand(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = CanClear;
        e.Handled = true;
    }

    private void HandleClearSelectionCommand(object sender, ExecutedRoutedEventArgs e)
    {
        ClearSelection();
        e.Handled = true;
    }

    private void CanExecuteRemoveSelectedValueCommand(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = Multiple && e.Parameter is NSelectTagItem { CanClose: true };
        e.Handled = true;
    }

    private void HandleRemoveSelectedValueCommand(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Parameter is NSelectTagItem tagItem)
        {
            RemoveSelectedValue(tagItem.Value);
        }

        e.Handled = true;
    }

    internal void RemoveSelectedValue(object? value)
    {
        if (!Multiple || SelectedValues is null)
        {
            return;
        }

        var oldValue = CloneList(SelectedValues);
        suppressSelectedValuesCollectionSync = true;
        try
        {
            RemoveValue(SelectedValues, value);
        }
        finally
        {
            suppressSelectedValuesCollectionSync = false;
        }

        UpdateSelectionState();
        SyncListBoxSelection();
        RaiseSelectionChanged(oldValue, CloneList(SelectedValues));
    }

    internal bool IsValueSelected(object? value)
    {
        if (Multiple)
        {
            return ContainsValue(SelectedValues, value);
        }

        return Equals(SelectedValue, value);
    }

    private static void OnOptionsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NSelect select)
        {
            select.RefreshOptions();
        }
    }

    private static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NSelect select && !select.syncingSelection)
        {
            select.UpdateSelectionState();
            select.SyncListBoxSelection();
            select.RaiseSelectionChanged(e.OldValue, e.NewValue);
        }
    }

    private static void OnSelectedValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NSelect select && !select.syncingSelection)
        {
            select.AttachSelectedValuesCollectionChanged(e.NewValue as IList);
            select.UpdateSelectionState();
            select.SyncListBoxSelection();
            select.RaiseSelectionChanged(e.OldValue, e.NewValue);
        }
    }

    private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NSelect select)
        {
            select.RefreshOptions();
        }
    }

    private static void OnMultipleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NSelect select)
        {
            select.UpdateSelectionState();
            select.SyncListBoxSelection();
        }
    }

    private static void OnDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NSelect select)
        {
            return;
        }

        select.UpdatePopupMetrics();

        if ((bool)e.NewValue)
        {
            select.SyncListBoxSelection();
            select.FocusSearchBoxIfNeeded();
            return;
        }

        if (!string.IsNullOrEmpty(select.SearchText))
        {
            select.SetCurrentValue(SearchTextProperty, string.Empty);
        }
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

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NSelect select)
        {
            select.UpdateResolvedMetrics();
            select.UpdatePopupMetrics();
            select.UpdateSelectionState();
        }
    }

    private static void OnPopupPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NSelect select)
        {
            select.UpdatePopupMetrics();
        }
    }

    private void HandleOptionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshOptions();
    }

    private void HandleSelectedValuesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (syncingSelection || suppressSelectedValuesCollectionSync)
        {
            return;
        }

        UpdateSelectionState();
        SyncListBoxSelection();
    }

    private void HandleListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (syncingSelection || listBoxPart is null)
        {
            return;
        }

        if (Multiple)
        {
            if (handlingPointerSelection)
            {
                return;
            }

            if ((e.AddedItems.OfType<NSelectOption>().LastOrDefault(option => !option.Disabled)
                ?? e.RemovedItems.OfType<NSelectOption>().LastOrDefault(option => !option.Disabled)) is NSelectOption selectedOption)
            {
                SelectOption(selectedOption);
            }
        }
        else if (listBoxPart.SelectedItem is NSelectOption selectedOption && !selectedOption.Disabled)
        {
            var oldValue = SelectedValue;
            SetCurrentValue(SelectedValueProperty, selectedOption.Value);
            SetCurrentValue(SearchTextProperty, string.Empty);
            SetCurrentValue(IsDropDownOpenProperty, false);
            UpdateSelectionState();
            RaiseSelectionChanged(oldValue, selectedOption.Value);
        }
    }

    private void HandleListBoxPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!Multiple)
        {
            return;
        }

        var item = FindAncestor<ListBoxItem>(e.OriginalSource as DependencyObject);
        if (item?.DataContext is not NSelectOption option || option.Disabled)
        {
            return;
        }

        handlingPointerSelection = true;
        try
        {
            SelectOption(option);
            e.Handled = true;
        }
        finally
        {
            handlingPointerSelection = false;
        }
    }

    private void HandlePartPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }

    private void SelectOption(NSelectOption option)
    {
        if (Multiple)
        {
            var selectedValues = EnsureSelectedValues();
            var oldValue = CloneList(selectedValues);

            suppressSelectedValuesCollectionSync = true;
            try
            {
                if (ContainsValue(selectedValues, option.Value))
                {
                    RemoveValue(selectedValues, option.Value);
                }
                else
                {
                    selectedValues.Add(option.Value);
                }
            }
            finally
            {
                suppressSelectedValuesCollectionSync = false;
            }

            SetCurrentValue(SearchTextProperty, string.Empty);
            UpdateSelectionState();
            RefreshOptions();
            SyncListBoxSelection();
            RaiseSelectionChanged(oldValue, CloneList(selectedValues));
            return;
        }

        var oldSingleValue = SelectedValue;
        SetCurrentValue(SelectedValueProperty, option.Value);
        SetCurrentValue(SearchTextProperty, string.Empty);
        UpdateSelectionState();
        SyncListBoxSelection();
        RaiseSelectionChanged(oldSingleValue, option.Value);

        if (CloseOnSelect)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }
    }

    private void RefreshOptions()
    {
        var nextOptions = BuildOptions();
        var filtered = FilterOptions(nextOptions);

        FilteredOptions.Clear();
        foreach (var option in filtered)
        {
            FilteredOptions.Add(option);
        }

        HasFilteredOptions = FilteredOptions.Count > 0;
        UpdateSelectionState();
        SyncListBoxSelection();
    }

    private List<NSelectOption> BuildOptions()
    {
        if (ItemsSource is null)
        {
            foreach (var option in Options)
            {
                if (option.Source is null)
                {
                    option.Source = option;
                }
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
                    options.Add(option);
                }

                continue;
            }

            var label = ResolveMemberValue(item, DisplayMemberPath) ?? item;
            var value = string.IsNullOrWhiteSpace(ValueMemberPath) ? item : ResolveMemberValue(item, ValueMemberPath);
            options.Add(new NSelectOption { Label = label, Value = value, Source = item });
        }

        return options;
    }

    private List<NSelectOption> FilterOptions(IEnumerable<NSelectOption> source)
    {
        var search = SearchText?.Trim();
        var results = new List<NSelectOption>();

        foreach (var option in source)
        {
            if (HideSelected && IsValueSelected(option.Value))
            {
                continue;
            }

            if (!Filterable || string.IsNullOrWhiteSpace(search) || IsOptionMatched(option, search))
            {
                results.Add(option);
            }
        }

        if (AllowCreate && Filterable && !string.IsNullOrWhiteSpace(search) && !results.Any(option => string.Equals(Convert.ToString(option.Label, CultureInfo.CurrentCulture), search, StringComparison.CurrentCultureIgnoreCase)))
        {
            results.Insert(0, new NSelectOption { Label = search, Value = search });
        }

        return results;
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

    private bool IsOptionMatched(NSelectOption option, string search)
    {
        var label = Convert.ToString(option.Label, CultureInfo.CurrentCulture) ?? string.Empty;
        var value = Convert.ToString(option.Value, CultureInfo.CurrentCulture) ?? string.Empty;
        return label.Contains(search, StringComparison.CurrentCultureIgnoreCase)
               || value.Contains(search, StringComparison.CurrentCultureIgnoreCase);
    }

    private IList EnsureSelectedValues()
    {
        if (SelectedValues is not null)
        {
            return SelectedValues;
        }

        var collection = new ObservableCollection<object?>();
        SetCurrentValue(SelectedValuesProperty, collection);
        return collection;
    }

    private void AttachSelectedValuesCollectionChanged(IList? selectedValues)
    {
        if (ReferenceEquals(selectedValuesNotifier, selectedValues))
        {
            return;
        }

        if (selectedValuesNotifier is not null)
        {
            selectedValuesNotifier.CollectionChanged -= HandleSelectedValuesCollectionChanged;
            selectedValuesNotifier = null;
        }

        if (selectedValues is INotifyCollectionChanged notifier)
        {
            selectedValuesNotifier = notifier;
            selectedValuesNotifier.CollectionChanged += HandleSelectedValuesCollectionChanged;
        }
    }

    private void UpdateSelectionState()
    {
        SelectedOptionLabels.Clear();
        ResolvedClosedArrowContent = ClosedArrowContent ?? ArrowContent;
        ResolvedOpenArrowContent = OpenArrowContent ?? ArrowContent;
        HasResolvedClosedArrowContent = ResolvedClosedArrowContent is not null;
        HasResolvedOpenArrowContent = ResolvedOpenArrowContent is not null;
        HasClearIconContent = ClearIconContent is not null;
        HasArrowContent = HasResolvedClosedArrowContent || HasResolvedOpenArrowContent;
        HasHeaderContent = HeaderContent is not null;
        HasActionContent = ActionContent is not null;

        var options = BuildOptions();
        object? selectedItem = null;
        var selectedCount = 0;

        if (Multiple)
        {
            var selectedValues = SelectedValues;
            if (selectedValues is not null)
            {
                foreach (var value in selectedValues)
                {
                    selectedCount++;
                    var option = options.FirstOrDefault(item => Equals(item.Value, value));
                    selectedItem ??= option;

                    if (MaxTagCount > 0 && selectedCount > MaxTagCount)
                    {
                        continue;
                    }

                    SelectedOptionLabels.Add(new NSelectTagItem(
                        option?.Label ?? value ?? string.Empty,
                        value,
                        option));
                }
            }

            var overflow = MaxTagCount > 0 ? selectedCount - Math.Min(selectedCount, MaxTagCount) : 0;
            if (overflow > 0)
            {
                SelectedOptionLabels.Add(new NSelectTagItem($"+{overflow}", null, canClose: false, isOverflow: true));
            }
        }
        else
        {
            var option = options.FirstOrDefault(item => Equals(item.Value, SelectedValue));
            if (option is not null)
            {
                selectedItem = option;
                DisplayContent = option;
            }
            else
            {
                DisplayContent = null;
            }
        }

        SelectedItem = selectedItem;
        HasSelection = Multiple ? selectedCount > 0 : SelectedValue is not null;
        ShowPlaceholder = !HasSelection && string.IsNullOrWhiteSpace(SearchText);
        CanClear = Clearable && HasSelection && IsEnabled && !Loading;
    }

    private void SyncListBoxSelection()
    {
        if (listBoxPart is null)
        {
            return;
        }

        try
        {
            syncingSelection = true;
            listBoxPart.SelectionMode = Multiple ? SelectionMode.Multiple : SelectionMode.Single;

            if (Multiple)
            {
                listBoxPart.SelectedItems.Clear();
                foreach (var option in FilteredOptions)
                {
                    if (ContainsValue(SelectedValues, option.Value))
                    {
                        listBoxPart.SelectedItems.Add(option);
                    }
                }
            }
            else
            {
                listBoxPart.SelectedItem = FilteredOptions.FirstOrDefault(option => Equals(option.Value, SelectedValue));
            }
        }
        finally
        {
            syncingSelection = false;
        }
    }

    private void UpdateResolvedMetrics()
    {
        var (height, fontSize, optionHeight, padding) = Size switch
        {
            NSelectSize.Tiny => (22d, 12d, 28d, new Thickness(8, 0, 28, 0)),
            NSelectSize.Small => (28d, 13d, 30d, new Thickness(10, 0, 30, 0)),
            NSelectSize.Large => (40d, 15d, 40d, new Thickness(14, 0, 36, 0)),
            _ => (34d, 14d, 34d, new Thickness(12, 0, 34, 0))
        };

        ResolvedHeight = height;
        ResolvedFontSize = fontSize;
        ResolvedOptionHeight = optionHeight;
        ResolvedPadding = padding;
    }

    private void UpdatePopupMetrics()
    {
        ResolvedPopupPlacement = Placement switch
        {
            NSelectPlacement.Top or NSelectPlacement.TopStart or NSelectPlacement.TopEnd => PlacementMode.Top,
            _ => PlacementMode.Bottom
        };

        ResolvedDropDownWidth = double.IsNaN(DropDownWidth) || DropDownWidth <= 0d
            ? Math.Max(ActualWidth, MinWidth)
            : DropDownWidth;
    }

    private void FocusSearchBoxIfNeeded()
    {
        if (!IsDropDownOpen || !Filterable || searchBoxPart is null)
        {
            return;
        }

        Dispatcher.BeginInvoke(() =>
        {
            searchBoxPart.Focus();
            searchBoxPart.Select(searchBoxPart.Text.Length, 0);
        });
    }

    private static bool ContainsValue(IEnumerable? values, object? value)
    {
        if (values is null)
        {
            return false;
        }

        foreach (var item in values)
        {
            if (Equals(item, value))
            {
                return true;
            }
        }

        return false;
    }

    private static void RemoveValue(IList values, object? value)
    {
        for (var i = values.Count - 1; i >= 0; i--)
        {
            if (Equals(values[i], value))
            {
                values.RemoveAt(i);
            }
        }
    }

    private static List<object?> CloneList(IEnumerable? values)
    {
        var result = new List<object?>();
        if (values is null)
        {
            return result;
        }

        foreach (var value in values)
        {
            result.Add(value);
        }

        return result;
    }

    private void RaiseSelectionChanged(object? oldValue, object? newValue)
    {
        RaiseEvent(new NSelectSelectionChangedEventArgs(SelectionChangedEvent, this, oldValue, newValue));
    }

    private bool IsSelectionActionSource(DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is ButtonBase button
                && (ReferenceEquals(button.Command, ClearSelectionCommand)
                    || ReferenceEquals(button.Command, RemoveSelectedValueCommand)
                    || Equals(button.Tag, "Clear")))
            {
                return true;
            }

            source = source switch
            {
                System.Windows.Media.Visual visual => System.Windows.Media.VisualTreeHelper.GetParent(visual),
                System.Windows.Media.Media3D.Visual3D visual3D => System.Windows.Media.VisualTreeHelper.GetParent(visual3D),
                FrameworkContentElement contentElement => contentElement.Parent,
                _ => null
            };
        }

        return false;
    }

    private bool IsSourceWithinSelect(DependencyObject? source)
    {
        while (source is not null)
        {
            if (ReferenceEquals(source, this) || ReferenceEquals(source, popupPart) || ReferenceEquals(source, listBoxPart) || ReferenceEquals(source, searchBoxPart))
            {
                return true;
            }

            if (source is FrameworkElement { TemplatedParent: NSelect select } && ReferenceEquals(select, this))
            {
                return true;
            }

            if (source is FrameworkElement element && ReferenceEquals(element.Tag, this))
            {
                return true;
            }

            source = GetParent(source);
        }

        return false;
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

            source = GetParent(source);
        }

        return null;
    }

    private static DependencyObject? GetParent(DependencyObject source)
    {
        return source switch
        {
            Visual visual => VisualTreeHelper.GetParent(visual),
            System.Windows.Media.Media3D.Visual3D visual3D => VisualTreeHelper.GetParent(visual3D),
            FrameworkContentElement contentElement => contentElement.Parent,
            _ => LogicalTreeHelper.GetParent(source)
        };
    }
}

public sealed class NSelectValueEqualityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return values.Length >= 2 && Equals(values[0], values[1]);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public sealed class NSelectValueInListConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[1] is not IEnumerable selectedValues)
        {
            return false;
        }

        foreach (var selectedValue in selectedValues)
        {
            if (Equals(selectedValue, values[0]))
            {
                return true;
            }
        }

        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
