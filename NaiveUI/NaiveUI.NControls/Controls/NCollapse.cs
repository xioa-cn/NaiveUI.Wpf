using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public enum NCollapseArrowPlacement
{
    Left,
    Right
}

public enum NCollapseDisplayDirective
{
    If,
    Show
}

[Flags]
public enum NCollapseTriggerAreas
{
    None = 0,
    Main = 1,
    Arrow = 2,
    Extra = 4,
    All = Main | Arrow | Extra
}

public sealed class NCollapseItemHeaderClickEventArgs : EventArgs
{
    public NCollapseItemHeaderClickEventArgs(NCollapseItem item, string itemName, bool isExpanded, NCollapseTriggerAreas triggerArea)
    {
        Item = item;
        ItemName = itemName;
        IsExpanded = isExpanded;
        TriggerArea = triggerArea;
    }

    public NCollapseItem Item { get; }

    public string ItemName { get; }

    public bool IsExpanded { get; }

    public NCollapseTriggerAreas TriggerArea { get; }
}

public class NCollapse : ItemsControl
{
    private readonly List<NCollapseItem> registeredItems = [];
    private bool defaultExpandedNamesApplied;

    static NCollapse()
    {
        ElementBase.DefaultStyle<NCollapse>(DefaultStyleKeyProperty);
    }

    public NCollapse()
    {
        SetCurrentValue(DefaultExpandedNamesProperty, new List<string>());
        Loaded += HandleLoaded;
    }

    public bool Accordion
    {
        get => (bool)GetValue(AccordionProperty);
        set => SetValue(AccordionProperty, value);
    }

    public static readonly DependencyProperty AccordionProperty =
        ElementBase.Property<NCollapse, bool>(nameof(AccordionProperty), false, OnCollapseConfigChanged);

    public NCollapseArrowPlacement ArrowPlacement
    {
        get => (NCollapseArrowPlacement)GetValue(ArrowPlacementProperty);
        set => SetValue(ArrowPlacementProperty, value);
    }

    public static readonly DependencyProperty ArrowPlacementProperty =
        ElementBase.Property<NCollapse, NCollapseArrowPlacement>(nameof(ArrowPlacementProperty), NCollapseArrowPlacement.Left, OnCollapseConfigChanged);

    public NCollapseDisplayDirective DisplayDirective
    {
        get => (NCollapseDisplayDirective)GetValue(DisplayDirectiveProperty);
        set => SetValue(DisplayDirectiveProperty, value);
    }

    public static readonly DependencyProperty DisplayDirectiveProperty =
        ElementBase.Property<NCollapse, NCollapseDisplayDirective>(nameof(DisplayDirectiveProperty), NCollapseDisplayDirective.If, OnCollapseConfigChanged);

    public NCollapseTriggerAreas TriggerAreas
    {
        get => (NCollapseTriggerAreas)GetValue(TriggerAreasProperty);
        set => SetValue(TriggerAreasProperty, value);
    }

    public static readonly DependencyProperty TriggerAreasProperty =
        ElementBase.Property<NCollapse, NCollapseTriggerAreas>(nameof(TriggerAreasProperty), NCollapseTriggerAreas.Main | NCollapseTriggerAreas.Arrow, OnCollapseConfigChanged);

    public IList? ExpandedNames
    {
        get => (IList?)GetValue(ExpandedNamesProperty);
        set => SetValue(ExpandedNamesProperty, value);
    }

    public static readonly DependencyProperty ExpandedNamesProperty =
        DependencyProperty.Register(
            nameof(ExpandedNames),
            typeof(IList),
            typeof(NCollapse),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnExpandedNamesChanged));

    public IList? DefaultExpandedNames
    {
        get => (IList?)GetValue(DefaultExpandedNamesProperty);
        set => SetValue(DefaultExpandedNamesProperty, value);
    }

    public static readonly DependencyProperty DefaultExpandedNamesProperty =
        ElementBase.Property<NCollapse, IList?>(nameof(DefaultExpandedNamesProperty), null, OnDefaultExpandedNamesChanged);

    public event EventHandler<NCollapseItemHeaderClickEventArgs>? ItemHeaderClick;

    protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        RefreshRegisteredItems();
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        ApplyDefaultExpandedNamesIfNeeded();
        RefreshRegisteredItems();
    }

    internal void ToggleItem(NCollapseItem item, NCollapseTriggerAreas triggerArea)
    {
        if (item.Disabled)
        {
            return;
        }

        var itemName = item.ResolveItemName();
        var expandedNames = GetNormalizedExpandedNames();
        var wasExpanded = ContainsName(expandedNames, itemName);

        if (Accordion)
        {
            expandedNames.Clear();
            if (!wasExpanded)
            {
                expandedNames.Add(itemName);
            }
        }
        else if (wasExpanded)
        {
            RemoveName(expandedNames, itemName);
        }
        else
        {
            expandedNames.Add(itemName);
        }

        defaultExpandedNamesApplied = true;
        SetCurrentValue(ExpandedNamesProperty, expandedNames);
        SyncExpandedStates(animate: true);

        ItemHeaderClick?.Invoke(this, new NCollapseItemHeaderClickEventArgs(item, itemName, !wasExpanded, triggerArea));
    }

    private static void OnCollapseConfigChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NCollapse collapse)
        {
            return;
        }

        if (e.Property == AccordionProperty)
        {
            collapse.CoerceExpandedNamesForAccordion();
        }

        collapse.SyncRegisteredItemsConfiguration();
        collapse.SyncExpandedStates(animate: false);
    }

    private static void OnExpandedNamesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NCollapse collapse)
        {
            return;
        }

        collapse.defaultExpandedNamesApplied = true;
        collapse.CoerceExpandedNamesForAccordion();
        collapse.SyncExpandedStates(animate: false);
    }

    private static void OnDefaultExpandedNamesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NCollapse collapse)
        {
            return;
        }

        collapse.defaultExpandedNamesApplied = false;
        collapse.ApplyDefaultExpandedNamesIfNeeded();
        collapse.SyncExpandedStates(animate: false);
    }

    private void RefreshRegisteredItems()
    {
        foreach (var item in registeredItems)
        {
            item.DetachCollapse(this);
        }

        registeredItems.Clear();

        foreach (var item in Items)
        {
            if (item is not NCollapseItem collapseItem)
            {
                continue;
            }

            collapseItem.AttachCollapse(this);
            registeredItems.Add(collapseItem);
        }

        SyncRegisteredItemsConfiguration();
        SyncExpandedStates(animate: false);
    }

    private void SyncRegisteredItemsConfiguration()
    {
        foreach (var item in registeredItems)
        {
            item.ApplyCollapseConfiguration(ArrowPlacement, DisplayDirective, TriggerAreas);
        }
    }

    private void SyncExpandedStates(bool animate)
    {
        var expandedNames = GetNormalizedExpandedNames();

        foreach (var item in registeredItems)
        {
            item.SetExpanded(ContainsName(expandedNames, item.ResolveItemName()), animate);
        }
    }

    private void ApplyDefaultExpandedNamesIfNeeded()
    {
        if (defaultExpandedNamesApplied || ExpandedNames is not null || DefaultExpandedNames is null || DefaultExpandedNames.Count == 0)
        {
            return;
        }

        defaultExpandedNamesApplied = true;
        SetCurrentValue(ExpandedNamesProperty, NormalizeNames(DefaultExpandedNames, Accordion));
    }

    private void CoerceExpandedNamesForAccordion()
    {
        if (!Accordion || ExpandedNames is null)
        {
            return;
        }

        var normalized = NormalizeNames(ExpandedNames, true);
        if (HasSameNames(ExpandedNames, normalized))
        {
            return;
        }

        SetCurrentValue(ExpandedNamesProperty, normalized);
    }

    private List<string> GetNormalizedExpandedNames()
    {
        var source = ExpandedNames ?? DefaultExpandedNames;
        return NormalizeNames(source, Accordion);
    }

    private static List<string> NormalizeNames(IList? source, bool accordion)
    {
        var results = new List<string>();
        if (source is null)
        {
            return results;
        }

        foreach (var entry in source)
        {
            var text = entry switch
            {
                null => null,
                string stringValue => stringValue,
                _ => entry.ToString()
            };

            if (string.IsNullOrWhiteSpace(text) || ContainsName(results, text))
            {
                continue;
            }

            results.Add(text);
            if (accordion)
            {
                break;
            }
        }

        return results;
    }

    private static bool ContainsName(List<string> items, string target)
    {
        foreach (var item in items)
        {
            if (string.Equals(item, target, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static void RemoveName(List<string> items, string target)
    {
        for (var i = items.Count - 1; i >= 0; i--)
        {
            if (string.Equals(items[i], target, StringComparison.Ordinal))
            {
                items.RemoveAt(i);
            }
        }
    }

    private static bool HasSameNames(IList source, List<string> normalized)
    {
        if (source.Count != normalized.Count)
        {
            return false;
        }

        for (var i = 0; i < source.Count; i++)
        {
            var current = source[i]?.ToString();
            if (!string.Equals(current, normalized[i], StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }
}
