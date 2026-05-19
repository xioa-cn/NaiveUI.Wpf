using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public class NDropdownMenu : ItemsControl
{
    private readonly List<NDropdownItem> registeredItems = [];
    private IReadOnlyList<DropdownEntry> currentEntries = [];

    static NDropdownMenu()
    {
        ElementBase.DefaultStyle<NDropdownMenu>(DefaultStyleKeyProperty);
    }

    internal NDropdown? OwnerDropdown { get; set; }

    internal double ResolvedPrefixWidth { get; private set; } = 14d;

    internal double ResolvedSuffixWidth { get; private set; } = 10d;

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is NDropdownItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new NDropdownItem();
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is not NDropdownItem dropdownItem || item is not DropdownEntry entry)
        {
            return;
        }

        if (!registeredItems.Contains(dropdownItem))
        {
            registeredItems.Add(dropdownItem);
        }

        dropdownItem.OwnerDropdown = OwnerDropdown;
        dropdownItem.ParentMenu = this;
        dropdownItem.Entry = entry;
        dropdownItem.RefreshFromEntry();
    }

    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        if (element is NDropdownItem dropdownItem)
        {
            dropdownItem.CloseSubmenuRecursive();
            dropdownItem.ParentMenu = null;
            dropdownItem.OwnerDropdown = null;
            dropdownItem.Entry = null;
            registeredItems.Remove(dropdownItem);
        }

        base.ClearContainerForItemOverride(element, item);
    }

    protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        OwnerDropdown?.NotifyMenuPointerEnter();
    }

    protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        OwnerDropdown?.NotifyMenuPointerLeave();
    }

    internal void SetEntries(IReadOnlyList<DropdownEntry> entries)
    {
        currentEntries = entries;
        UpdateResolvedLayoutMetrics();
        ItemsSource = entries;
    }

    internal void HandleItemHover(NDropdownItem current)
    {
        CloseSubmenusExcept(current);
    }

    internal void CloseSubmenusExcept(NDropdownItem? current)
    {
        foreach (var item in registeredItems)
        {
            if (!ReferenceEquals(item, current))
            {
                item.CloseSubmenuRecursive();
            }
        }
    }

    internal void CloseAllSubmenusRecursive()
    {
        foreach (var item in registeredItems)
        {
            item.CloseSubmenuRecursive();
        }
    }

    internal void RefreshSelectionStatesRecursive()
    {
        foreach (var item in registeredItems)
        {
            item.RefreshSelectionStateRecursive();
        }
    }

    internal double UpdateRequiredWidthRecursive()
    {
        UpdateLayout();

        const double defaultMinWidth = 144d;
        var requiredWidth = defaultMinWidth;

        foreach (var item in registeredItems)
        {
            requiredWidth = Math.Max(requiredWidth, item.MeasureRequiredWidth());
            item.UpdateSubmenuWidthRecursive();
        }

        if (Math.Abs(MinWidth - requiredWidth) > 0.5d)
        {
            SetCurrentValue(MinWidthProperty, requiredWidth);
            UpdateLayout();
        }

        return requiredWidth;
    }

    private void UpdateResolvedLayoutMetrics()
    {
        var size = OwnerDropdown?.Size ?? NDropdownSize.Medium;
        var hasIcons = ContainsIcons(currentEntries);
        var hasSubmenu = ContainsSubmenus(currentEntries);

        ResolvedPrefixWidth = GetPrefixWidth(size, hasIcons);
        ResolvedSuffixWidth = GetSuffixWidth(size, hasSubmenu, MeasureMaxSuffixWidth(currentEntries));

        foreach (var item in registeredItems)
        {
            item.RefreshFromEntry();
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
            ? size is NDropdownSize.Large or NDropdownSize.Huge ? 36d : 32d
            : 14d;

        if (maxSuffixContentWidth <= 0d)
        {
            return baseWidth;
        }

        return Math.Max(baseWidth, maxSuffixContentWidth + 16d);
    }
}
