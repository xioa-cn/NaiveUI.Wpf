using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public class NDropdownItem : MenuItem
{
    static NDropdownItem()
    {
        ElementBase.DefaultStyle<NDropdownItem>(DefaultStyleKeyProperty);
    }

    internal DropdownEntry? Entry { get; private set; }

    internal NDropdown? OwnerDropdown { get; set; }

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

    public bool IsCurrentSelection
    {
        get => (bool)GetValue(IsCurrentSelectionProperty);
        private set => SetValue(IsCurrentSelectionProperty, value);
    }

    public static readonly DependencyProperty IsCurrentSelectionProperty =
        ElementBase.Property<NDropdownItem, bool>(nameof(IsCurrentSelectionProperty), false);

    public bool IsSelectionPath
    {
        get => (bool)GetValue(IsSelectionPathProperty);
        private set => SetValue(IsSelectionPathProperty, value);
    }

    public static readonly DependencyProperty IsSelectionPathProperty =
        ElementBase.Property<NDropdownItem, bool>(nameof(IsSelectionPathProperty), false);

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

    public double MenuMaxHeight
    {
        get => (double)GetValue(MenuMaxHeightProperty);
        private set => SetValue(MenuMaxHeightProperty, value);
    }

    public static readonly DependencyProperty MenuMaxHeightProperty =
        ElementBase.Property<NDropdownItem, double>(nameof(MenuMaxHeightProperty), 320d);

    protected override void OnClick()
    {
        if (EntryKind == NDropdownEntryKind.Option
            && Entry is not null
            && !Entry.HasChildren
            && OwnerDropdown is not null)
        {
            OwnerDropdown.HandleEntryInvoked(Entry, closeAfterInvoke: false);
        }

        base.OnClick();
    }

    internal void ApplyEntry(
        DropdownEntry entry,
        double prefixWidth,
        double suffixWidth,
        NDropdownSize size,
        double menuMaxHeight,
        object? dataContext)
    {
        Entry = entry;
        DataContext = dataContext;
        Items.Clear();

        var isGroupHeader = entry.Kind == NDropdownEntryKind.GroupHeader;

        EntryKind = entry.Kind;
        Header = entry.Kind == NDropdownEntryKind.Render ? null : entry.Label;
        Icon = entry.Icon;
        SuffixContent = entry.Suffix;
        RenderContent = entry.Content;
        HasSuffix = entry.Kind == NDropdownEntryKind.Option && entry.Suffix is not null;
        OptionHeight = GetOptionHeight(size);
        OptionFontSize = isGroupHeader ? GetOptionFontSize(size) - 1d : GetOptionFontSize(size);
        OptionIconSize = GetOptionIconSize(size);
        PrefixWidth = isGroupHeader ? Math.Max(8d, prefixWidth / 2d) : prefixWidth;
        SuffixWidth = isGroupHeader ? 8d : suffixWidth;
        MenuMaxHeight = menuMaxHeight;
        IsCheckable = false;
        StaysOpenOnClick = entry.Kind != NDropdownEntryKind.Option || entry.HasChildren;
        IsEnabled = entry.Kind != NDropdownEntryKind.Option || !entry.Disabled;
        IsHitTestVisible = entry.Kind == NDropdownEntryKind.Option && !entry.Disabled;
        Focusable = false;
        Cursor = entry.Kind == NDropdownEntryKind.Option && !entry.Disabled ? Cursors.Hand : Cursors.Arrow;

        ApplyIconPresentation(Icon);
        ApplyIconPresentation(SuffixContent);
        RefreshSelectionStateRecursive();
    }

    internal void RefreshSelectionStateRecursive()
    {
        if (EntryKind != NDropdownEntryKind.Option || Entry is null || OwnerDropdown is null)
        {
            IsCurrentSelection = false;
            IsSelectionPath = false;
        }
        else
        {
            IsCurrentSelection = Entry.Key is not null && OwnerDropdown.IsSelectedKey(Entry.Key);
            IsSelectionPath = !IsCurrentSelection && Entry.HasChildren && OwnerDropdown.ContainsSelectedDescendant(Entry);
        }

        foreach (var item in Items)
        {
            if (item is NDropdownItem child)
            {
                child.RefreshSelectionStateRecursive();
            }
        }
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
