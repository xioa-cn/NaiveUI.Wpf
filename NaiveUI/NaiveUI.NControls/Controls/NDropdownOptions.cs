using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;

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

internal static class DropdownBindingHelper
{
    public static object? ResolveDependencyPropertyValue(DependencyObject target, DependencyProperty property, object? fallbackValue, NDropdown dropdown)
    {
        if (BindingOperations.GetBindingBase(target, property) is Binding binding)
        {
            return ResolveBindingValue(binding, dropdown);
        }

        return fallbackValue;
    }

    private static object? ResolveBindingValue(Binding binding, NDropdown dropdown)
    {
        if (binding.Source is not null)
        {
            return EvaluatePath(binding.Source, binding.Path?.Path);
        }

        if (!string.IsNullOrWhiteSpace(binding.ElementName))
        {
            var element = FindElementByName(dropdown, binding.ElementName);
            return EvaluatePath(element, binding.Path?.Path);
        }

        if (binding.RelativeSource is not null)
        {
            var relativeSource = binding.RelativeSource;
            return relativeSource.Mode switch
            {
                RelativeSourceMode.Self => EvaluatePath(dropdown, binding.Path?.Path),
                RelativeSourceMode.FindAncestor => EvaluatePath(FindAncestor(dropdown, relativeSource.AncestorType, relativeSource.AncestorLevel), binding.Path?.Path),
                _ => null
            };
        }

        return EvaluatePath(dropdown.DataContext, binding.Path?.Path);
    }

    private static FrameworkElement? FindElementByName(FrameworkElement start, string elementName)
    {
        FrameworkElement? current = start;
        while (current is not null)
        {
            if (current.FindName(elementName) is FrameworkElement match)
            {
                return match;
            }

            current = GetParent(current);
        }

        return null;
    }

    private static DependencyObject? FindAncestor(DependencyObject start, Type? ancestorType, int ancestorLevel)
    {
        if (ancestorType is null)
        {
            return null;
        }

        var level = Math.Max(1, ancestorLevel);
        var matches = 0;
        var current = GetParent(start);

        while (current is not null)
        {
            if (ancestorType.IsInstanceOfType(current))
            {
                matches++;
                if (matches == level)
                {
                    return current;
                }
            }

            current = GetParent(current);
        }

        return null;
    }

    private static FrameworkElement? GetParent(DependencyObject current)
    {
        if (LogicalTreeHelper.GetParent(current) is FrameworkElement logicalParent)
        {
            return logicalParent;
        }

        if (current is Visual || current is Visual3D)
        {
            return VisualTreeHelper.GetParent(current) as FrameworkElement;
        }

        return null;
    }

    private static object? EvaluatePath(object? source, string? path)
    {
        if (source is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            return source;
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
}

public abstract class NDropdownOptionBase : DependencyObject
{
    public object? Key { get; set; }

    public bool Show { get; set; } = true;
}

public class NDropdownOption : NDropdownOptionBase
{
    public object? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(object), typeof(NDropdownOption), new PropertyMetadata(null));

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(NDropdownOption), new PropertyMetadata(null));

    public object? Suffix
    {
        get => GetValue(SuffixProperty);
        set => SetValue(SuffixProperty, value);
    }

    public static readonly DependencyProperty SuffixProperty =
        DependencyProperty.Register(nameof(Suffix), typeof(object), typeof(NDropdownOption), new PropertyMetadata(null));

    public bool Disabled { get; set; }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(NDropdownOption), new PropertyMetadata(null));

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(NDropdownOption), new PropertyMetadata(null));

    public event EventHandler<NDropdownOptionClickEventArgs>? Click;

    internal object? ResolveLabel(NDropdown dropdown)
    {
        return DropdownBindingHelper.ResolveDependencyPropertyValue(this, LabelProperty, Label, dropdown);
    }

    internal object? ResolveIcon(NDropdown dropdown)
    {
        return DropdownBindingHelper.ResolveDependencyPropertyValue(this, IconProperty, Icon, dropdown);
    }

    internal object? ResolveSuffix(NDropdown dropdown)
    {
        return DropdownBindingHelper.ResolveDependencyPropertyValue(this, SuffixProperty, Suffix, dropdown);
    }

    internal bool TryInvoke(NDropdown dropdown)
    {
        return TryInvoke(dropdown, Key, this);
    }

    internal bool TryInvoke(NDropdown dropdown, object? invokedKey, NDropdownOption invokedOption)
    {
        if (Disabled)
        {
            return false;
        }

        var args = new NDropdownOptionClickEventArgs(dropdown, invokedKey, invokedOption);
        var command = ResolveCommand(dropdown);
        var hasExplicitCommandParameter = ReadLocalValue(CommandParameterProperty) != DependencyProperty.UnsetValue;

        Click?.Invoke(this, args);

        if (command is null)
        {
            return true;
        }

        if (hasExplicitCommandParameter)
        {
            TryExecuteCommand(command, ResolveCommandParameter(dropdown));
            return true;
        }

        if (TryExecuteCommand(command, args))
        {
            return true;
        }

        if (TryExecuteCommand(command, invokedKey))
        {
            return true;
        }

        TryExecuteCommand(command, null);
        return true;
    }

    private static bool TryExecuteCommand(ICommand command, object? parameter)
    {
        if (!command.CanExecute(parameter))
        {
            return false;
        }

        command.Execute(parameter);
        return true;
    }

    private ICommand? ResolveCommand(NDropdown dropdown)
    {
        if (Command is not null)
        {
            return Command;
        }

        if (BindingOperations.GetBindingBase(this, CommandProperty) is not null)
        {
            return DropdownBindingHelper.ResolveDependencyPropertyValue(this, CommandProperty, Command, dropdown) as ICommand;
        }

        return null;
    }

    private object? ResolveCommandParameter(NDropdown dropdown)
    {
        return DropdownBindingHelper.ResolveDependencyPropertyValue(this, CommandParameterProperty, CommandParameter, dropdown);
    }
}

[ContentProperty(nameof(Options))]
public class NDropdownCombineOption : NDropdownOptionBase
{
    public object? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(object), typeof(NDropdownCombineOption), new PropertyMetadata(null));

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(NDropdownCombineOption), new PropertyMetadata(null));

    public object? Suffix
    {
        get => GetValue(SuffixProperty);
        set => SetValue(SuffixProperty, value);
    }

    public static readonly DependencyProperty SuffixProperty =
        DependencyProperty.Register(nameof(Suffix), typeof(object), typeof(NDropdownCombineOption), new PropertyMetadata(null));

    public bool Disabled { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ObservableCollection<NDropdownOptionBase> Options { get; } = [];

    internal object? ResolveLabel(NDropdown dropdown)
    {
        return DropdownBindingHelper.ResolveDependencyPropertyValue(this, LabelProperty, Label, dropdown);
    }

    internal object? ResolveIcon(NDropdown dropdown)
    {
        return DropdownBindingHelper.ResolveDependencyPropertyValue(this, IconProperty, Icon, dropdown);
    }

    internal object? ResolveSuffix(NDropdown dropdown)
    {
        return DropdownBindingHelper.ResolveDependencyPropertyValue(this, SuffixProperty, Suffix, dropdown);
    }
}

[ContentProperty(nameof(Options))]
public class NDropdownGroupOption : NDropdownOptionBase
{
    public object? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(object), typeof(NDropdownGroupOption), new PropertyMetadata(null));

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(NDropdownGroupOption), new PropertyMetadata(null));

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ObservableCollection<NDropdownOptionBase> Options { get; } = [];

    internal object? ResolveLabel(NDropdown dropdown)
    {
        return DropdownBindingHelper.ResolveDependencyPropertyValue(this, LabelProperty, Label, dropdown);
    }

    internal object? ResolveIcon(NDropdown dropdown)
    {
        return DropdownBindingHelper.ResolveDependencyPropertyValue(this, IconProperty, Icon, dropdown);
    }
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

public sealed class NDropdownOptionClickEventArgs : EventArgs
{
    public NDropdownOptionClickEventArgs(NDropdown dropdown, object? key, NDropdownOption option)
    {
        Dropdown = dropdown;
        Key = key;
        Option = option;
    }

    public NDropdown Dropdown { get; }

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
