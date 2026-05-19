using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public class NDropdownItem : Control
{
    static NDropdownItem()
    {
        ElementBase.DefaultStyle<NDropdownItem>(DefaultStyleKeyProperty);
    }

    internal DropdownEntry? Entry { get; set; }

    internal NDropdown? OwnerDropdown { get; set; }

    internal NDropdownMenu? ParentMenu { get; set; }

    public NDropdownEntryKind EntryKind
    {
        get => (NDropdownEntryKind)GetValue(EntryKindProperty);
        private set => SetValue(EntryKindProperty, value);
    }

    public static readonly DependencyProperty EntryKindProperty =
        ElementBase.Property<NDropdownItem, NDropdownEntryKind>(nameof(EntryKindProperty), NDropdownEntryKind.Option);

    public bool HasSuffix
    {
        get => (bool)GetValue(HasSuffixProperty);
        private set => SetValue(HasSuffixProperty, value);
    }

    public static readonly DependencyProperty HasSuffixProperty =
        ElementBase.Property<NDropdownItem, bool>(nameof(HasSuffixProperty), false);

    public bool IsDisabled
    {
        get => (bool)GetValue(IsDisabledProperty);
        private set => SetValue(IsDisabledProperty, value);
    }

    public static readonly DependencyProperty IsDisabledProperty =
        ElementBase.Property<NDropdownItem, bool>(nameof(IsDisabledProperty), false);

    public bool IsHighlighted
    {
        get => (bool)GetValue(IsHighlightedProperty);
        private set => SetValue(IsHighlightedProperty, value);
    }

    public static readonly DependencyProperty IsHighlightedProperty =
        ElementBase.Property<NDropdownItem, bool>(nameof(IsHighlightedProperty), false);

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        private set => SetValue(IsSelectedProperty, value);
    }

    public static readonly DependencyProperty IsSelectedProperty =
        ElementBase.Property<NDropdownItem, bool>(nameof(IsSelectedProperty), false);

    public object? LabelContent
    {
        get => GetValue(LabelContentProperty);
        private set => SetValue(LabelContentProperty, value);
    }

    public static readonly DependencyProperty LabelContentProperty =
        ElementBase.Property<NDropdownItem, object?>(nameof(LabelContentProperty), null);

    public object? IconContent
    {
        get => GetValue(IconContentProperty);
        private set => SetValue(IconContentProperty, value);
    }

    public static readonly DependencyProperty IconContentProperty =
        ElementBase.Property<NDropdownItem, object?>(nameof(IconContentProperty), null);

    public object? SuffixContent
    {
        get => GetValue(SuffixContentProperty);
        private set => SetValue(SuffixContentProperty, value);
    }

    public static readonly DependencyProperty SuffixContentProperty =
        ElementBase.Property<NDropdownItem, object?>(nameof(SuffixContentProperty), null);

    public object? RenderContent
    {
        get => GetValue(RenderContentProperty);
        private set => SetValue(RenderContentProperty, value);
    }

    public static readonly DependencyProperty RenderContentProperty =
        ElementBase.Property<NDropdownItem, object?>(nameof(RenderContentProperty), null);

    public double OptionHeight
    {
        get => (double)GetValue(OptionHeightProperty);
        private set => SetValue(OptionHeightProperty, value);
    }

    public static readonly DependencyProperty OptionHeightProperty =
        ElementBase.Property<NDropdownItem, double>(nameof(OptionHeightProperty), 34d);

    public double OptionFontSize
    {
        get => (double)GetValue(OptionFontSizeProperty);
        private set => SetValue(OptionFontSizeProperty, value);
    }

    public static readonly DependencyProperty OptionFontSizeProperty =
        ElementBase.Property<NDropdownItem, double>(nameof(OptionFontSizeProperty), 14d);

    public double PrefixWidth
    {
        get => (double)GetValue(PrefixWidthProperty);
        private set => SetValue(PrefixWidthProperty, value);
    }

    public static readonly DependencyProperty PrefixWidthProperty =
        ElementBase.Property<NDropdownItem, double>(nameof(PrefixWidthProperty), 14d);

    public double OptionIconSize
    {
        get => (double)GetValue(OptionIconSizeProperty);
        private set => SetValue(OptionIconSizeProperty, value);
    }

    public static readonly DependencyProperty OptionIconSizeProperty =
        ElementBase.Property<NDropdownItem, double>(nameof(OptionIconSizeProperty), 16d);

    public double SuffixWidth
    {
        get => (double)GetValue(SuffixWidthProperty);
        private set => SetValue(SuffixWidthProperty, value);
    }

    public static readonly DependencyProperty SuffixWidthProperty =
        ElementBase.Property<NDropdownItem, double>(nameof(SuffixWidthProperty), 12d);

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        RefreshFromEntry();
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);

        OwnerDropdown?.CancelHoverCloseTimer();

        if (EntryKind != NDropdownEntryKind.Option || IsDisabled)
        {
            return;
        }

        IsHighlighted = true;
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        IsHighlighted = false;
    }

    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonUp(e);

        if (EntryKind != NDropdownEntryKind.Option || IsDisabled || Entry is null)
        {
            return;
        }

        OwnerDropdown?.HandleEntryInvoked(Entry);
        e.Handled = true;
    }

    internal void RefreshFromEntry()
    {
        var entry = Entry;
        if (entry is null)
        {
            EntryKind = NDropdownEntryKind.Option;
            HasSuffix = false;
            IsDisabled = false;
            IsHighlighted = false;
            IsSelected = false;
            LabelContent = null;
            IconContent = null;
            SuffixContent = null;
            RenderContent = null;
            OptionHeight = 34d;
            OptionFontSize = 14d;
            OptionIconSize = 16d;
            PrefixWidth = 14d;
            SuffixWidth = 12d;
            Cursor = Cursors.Arrow;
            return;
        }

        var size = OwnerDropdown?.Size ?? NDropdownSize.Medium;
        var isGroupHeader = entry.Kind == NDropdownEntryKind.GroupHeader;
        var menuPrefixWidth = ParentMenu?.ResolvedPrefixWidth ?? GetCompactPrefixWidth(size);
        var menuSuffixWidth = ParentMenu?.ResolvedSuffixWidth ?? GetCompactSuffixWidth(size);

        EntryKind = entry.Kind;
        HasSuffix = entry.Kind == NDropdownEntryKind.Option && entry.Suffix is not null;
        IsDisabled = entry.Kind == NDropdownEntryKind.Option && entry.Disabled;
        LabelContent = entry.Label;
        IconContent = entry.Icon;
        SuffixContent = entry.Suffix;
        RenderContent = entry.Content;
        OptionHeight = GetOptionHeight(size);
        OptionFontSize = isGroupHeader ? GetOptionFontSize(size) - 1d : GetOptionFontSize(size);
        OptionIconSize = GetOptionIconSize(size);
        PrefixWidth = isGroupHeader ? Math.Max(8d, menuPrefixWidth / 2d) : menuPrefixWidth;
        SuffixWidth = isGroupHeader ? 8d : menuSuffixWidth;
        Cursor = entry.Kind == NDropdownEntryKind.Option && !entry.Disabled ? Cursors.Hand : Cursors.Arrow;
        ApplyIconPresentation(IconContent);
        ApplyIconPresentation(SuffixContent);
        RefreshSelectionState();
    }

    internal void RefreshSelectionState()
    {
        if (EntryKind != NDropdownEntryKind.Option || Entry is null || OwnerDropdown is null)
        {
            IsSelected = false;
            return;
        }

        IsSelected = Entry.Key is not null && OwnerDropdown.IsSelectedKey(Entry.Key);
    }

    internal double MeasureRequiredWidth()
    {
        InvalidateMeasure();
        Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        return DesiredSize.Width;
    }

    private static double GetOptionHeight(NDropdownSize size)
    {
        return size switch
        {
            NDropdownSize.Small => 28d,
            NDropdownSize.Large => 40d,
            NDropdownSize.Huge => 46d,
            _ => 34d
        };
    }

    private static double GetOptionFontSize(NDropdownSize size)
    {
        return size switch
        {
            NDropdownSize.Small => 13d,
            NDropdownSize.Large => 15d,
            NDropdownSize.Huge => 16d,
            _ => 14d
        };
    }

    private static double GetOptionIconSize(NDropdownSize size)
    {
        return size switch
        {
            NDropdownSize.Small => 14d,
            NDropdownSize.Huge => 18d,
            _ => 16d
        };
    }

    private static double GetCompactPrefixWidth(NDropdownSize size)
    {
        return size is NDropdownSize.Large or NDropdownSize.Huge ? 16d : 14d;
    }

    private static double GetCompactSuffixWidth(NDropdownSize size)
    {
        return size is NDropdownSize.Large or NDropdownSize.Huge ? 16d : 14d;
    }

    private void ApplyIconPresentation(object? content)
    {
        if (content is FrameworkElement element)
        {
            element.VerticalAlignment = VerticalAlignment.Center;
        }

        if (content is not NIcon icon)
        {
            return;
        }

        icon.Width = OptionIconSize;
        icon.Height = OptionIconSize;
        icon.HorizontalAlignment = HorizontalAlignment.Center;
        icon.VerticalAlignment = VerticalAlignment.Center;

        BindingOperations.SetBinding(
            icon,
            NIcon.IconBrushProperty,
            new Binding(nameof(Foreground))
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(NDropdownItem), 1)
            });
    }
}
