using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public class NDropdownMenu : ContextMenu
{
    public const string SeparatorStyleKey = "DropdownSeparatorStyle";

    static NDropdownMenu()
    {
        ElementBase.DefaultStyle<NDropdownMenu>(DefaultStyleKeyProperty);
    }

    internal NDropdown? OwnerDropdown
    {
        get => ownerDropdown;
        set
        {
            ownerDropdown = value;
            PlacementTarget = value;
            DataContext = value?.DataContext;
            SetCurrentValue(MaxHeightProperty, value?.MaxMenuHeight ?? 320d);
        }
    }

    private NDropdown? ownerDropdown;

    public bool ShowArrowVisual
    {
        get => (bool)GetValue(ShowArrowVisualProperty);
        internal set => SetValue(ShowArrowVisualProperty, value);
    }

    public static readonly DependencyProperty ShowArrowVisualProperty =
        ElementBase.Property<NDropdownMenu, bool>(nameof(ShowArrowVisualProperty), false);

    public Thickness MenuContentMargin
    {
        get => (Thickness)GetValue(MenuContentMarginProperty);
        internal set => SetValue(MenuContentMarginProperty, value);
    }

    public static readonly DependencyProperty MenuContentMarginProperty =
        ElementBase.Property<NDropdownMenu, Thickness>(nameof(MenuContentMarginProperty), new Thickness(0));

    public double ArrowLeft
    {
        get => (double)GetValue(ArrowLeftProperty);
        internal set => SetValue(ArrowLeftProperty, value);
    }

    public static readonly DependencyProperty ArrowLeftProperty =
        ElementBase.Property<NDropdownMenu, double>(nameof(ArrowLeftProperty), 0d);

    public double ArrowTop
    {
        get => (double)GetValue(ArrowTopProperty);
        internal set => SetValue(ArrowTopProperty, value);
    }

    public static readonly DependencyProperty ArrowTopProperty =
        ElementBase.Property<NDropdownMenu, double>(nameof(ArrowTopProperty), 0d);

    internal void SetEntries(IReadOnlyList<DropdownEntry> entries)
    {
        DataContext = OwnerDropdown?.DataContext;
        SetCurrentValue(MaxHeightProperty, OwnerDropdown?.MaxMenuHeight ?? 320d);

        Items.Clear();
        foreach (var element in BuildMenuElements(entries))
        {
            Items.Add(element);
        }
    }

    internal void RefreshSelectionStatesRecursive()
    {
        foreach (var item in EnumerateDropdownItems(Items))
        {
            item.RefreshSelectionStateRecursive();
        }
    }

    internal void UpdateChromeLayout(bool showArrow, Thickness menuMargin, double arrowLeft, double arrowTop)
    {
        ShowArrowVisual = showArrow;
        MenuContentMargin = menuMargin;
        ArrowLeft = arrowLeft;
        ArrowTop = arrowTop;
    }

    private List<object> BuildMenuElements(IReadOnlyList<DropdownEntry> entries)
    {
        var size = OwnerDropdown?.Size ?? NDropdownSize.Medium;
        var prefixWidth = GetPrefixWidth(size, ContainsIcons(entries));
        var suffixWidth = GetSuffixWidth(size, ContainsSubmenus(entries), MeasureMaxSuffixWidth(entries));
        var results = new List<object>(entries.Count);

        foreach (var entry in entries)
        {
            if (entry.Kind == NDropdownEntryKind.Divider)
            {
                var separator = new Separator();
                separator.SetResourceReference(StyleProperty, SeparatorStyleKey);
                results.Add(separator);
                continue;
            }

            var item = new NDropdownItem
            {
                OwnerDropdown = OwnerDropdown
            };

            item.ApplyEntry(
                entry,
                prefixWidth,
                suffixWidth,
                size,
                OwnerDropdown?.MaxMenuHeight ?? 320d,
                OwnerDropdown?.DataContext);

            if (entry.HasChildren)
            {
                foreach (var child in BuildMenuElements(entry.Children))
                {
                    item.Items.Add(child);
                }
            }

            results.Add(item);
        }

        return results;
    }

    private static IEnumerable<NDropdownItem> EnumerateDropdownItems(ItemCollection items)
    {
        foreach (var item in items)
        {
            if (item is not NDropdownItem dropdownItem)
            {
                continue;
            }

            yield return dropdownItem;

            foreach (var child in EnumerateDropdownItems(dropdownItem.Items))
            {
                yield return child;
            }
        }
    }

    private static bool ContainsIcons(IReadOnlyList<DropdownEntry> entries)
    {
        foreach (var entry in entries)
        {
            if (entry.Icon is not null)
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsSubmenus(IReadOnlyList<DropdownEntry> entries)
    {
        foreach (var entry in entries)
        {
            if (entry.Kind == NDropdownEntryKind.Option && entry.HasChildren)
            {
                return true;
            }
        }

        return false;
    }

    private static double MeasureMaxSuffixWidth(IReadOnlyList<DropdownEntry> entries)
    {
        var maxWidth = 0d;

        foreach (var entry in entries)
        {
            if (entry.Kind != NDropdownEntryKind.Option || entry.Suffix is null)
            {
                continue;
            }

            maxWidth = Math.Max(maxWidth, MeasureContentWidth(entry.Suffix));
        }

        return maxWidth;
    }

    private static double MeasureContentWidth(object content)
    {
        if (content is FrameworkElement element)
        {
            element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            return element.DesiredSize.Width;
        }

        var presenter = new ContentPresenter
        {
            Content = content
        };

        presenter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        return presenter.DesiredSize.Width;
    }

    private static double GetPrefixWidth(NDropdownSize size, bool hasIcons)
    {
        if (!hasIcons)
        {
            return size is NDropdownSize.Large or NDropdownSize.Huge ? 16d : 14d;
        }

        return size is NDropdownSize.Large or NDropdownSize.Huge ? 40d : 36d;
    }

    private static double GetSuffixWidth(NDropdownSize size, bool hasSubmenu, double maxSuffixContentWidth)
    {
        var baseWidth = hasSubmenu
            ? size is NDropdownSize.Large or NDropdownSize.Huge ? 28d : 24d
            : 14d;

        if (maxSuffixContentWidth <= 0d)
        {
            return baseWidth;
        }

        var contentPadding = hasSubmenu ? 28d : 16d;
        return Math.Max(baseWidth, maxSuffixContentWidth + contentPadding);
    }
}
