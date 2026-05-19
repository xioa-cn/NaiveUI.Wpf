using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Markup;

namespace NaiveUI.NControls.Controls;

public enum NDropdownTrigger
{
    Hover,
    Click,
    Manual,
    ContextMenu
}

public enum NDropdownPlacement
{
    Top,
    TopStart,
    TopEnd,
    Right,
    RightStart,
    RightEnd,
    Bottom,
    BottomStart,
    BottomEnd,
    Left,
    LeftStart,
    LeftEnd
}

public enum NDropdownSize
{
    Small,
    Medium,
    Large,
    Huge
}

public enum NDropdownEntryKind
{
    Option,
    Divider,
    GroupHeader,
    Render
}

public abstract class NDropdownOptionBase
{
    public object? Key { get; set; }

    public bool Show { get; set; } = true;
}

[ContentProperty(nameof(Children))]
public class NDropdownOption : NDropdownOptionBase
{
    public object? Label { get; set; }

    public object? Icon { get; set; }

    public object? Suffix { get; set; }

    public bool Disabled { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ObservableCollection<NDropdownOptionBase> Children { get; } = [];
}

[ContentProperty(nameof(Children))]
public class NDropdownGroupOption : NDropdownOptionBase
{
    public object? Label { get; set; }

    public object? Icon { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ObservableCollection<NDropdownOptionBase> Children { get; } = [];
}

public sealed class NDropdownDividerOption : NDropdownOptionBase
{
}

[ContentProperty(nameof(Content))]
public sealed class NDropdownRenderOption : NDropdownOptionBase
{
    public object? Content { get; set; }
}

public sealed class NDropdownSelectEventArgs : EventArgs
{
    public NDropdownSelectEventArgs(object? key, NDropdownOption option)
    {
        Key = key;
        Option = option;
    }

    public object? Key { get; }

    public NDropdownOption Option { get; }
}

internal sealed class DropdownEntry
{
    public DropdownEntry(
        NDropdownEntryKind kind,
        object? key = null,
        object? label = null,
        object? icon = null,
        object? suffix = null,
        object? content = null,
        bool disabled = false,
        NDropdownOption? sourceOption = null,
        IReadOnlyList<DropdownEntry>? children = null)
    {
        Kind = kind;
        Key = key;
        Label = label;
        Icon = icon;
        Suffix = suffix;
        Content = content;
        Disabled = disabled;
        SourceOption = sourceOption;
        Children = children ?? [];
    }

    public NDropdownEntryKind Kind { get; }

    public object? Key { get; }

    public object? Label { get; }

    public object? Icon { get; }

    public object? Suffix { get; }

    public object? Content { get; }

    public bool Disabled { get; }

    public NDropdownOption? SourceOption { get; }

    public IReadOnlyList<DropdownEntry> Children { get; }

    public bool HasChildren => Children.Count > 0;

    public bool ContainsKey(object? value)
    {
        if (value is null)
        {
            return false;
        }

        if (Key is not null && Equals(Key, value))
        {
            return true;
        }

        foreach (var child in Children)
        {
            if (child.ContainsKey(value))
            {
                return true;
            }
        }

        return false;
    }
}
