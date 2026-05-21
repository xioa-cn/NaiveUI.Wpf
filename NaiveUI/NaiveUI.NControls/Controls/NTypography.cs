using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public enum NTypographyType
{
    Default,
    Success,
    Info,
    Warning,
    Error
}

public enum NTypographyHeaderPrefix
{
    None,
    Bar
}

public class NText : ContentControl
{
    static NText()
    {
        ElementBase.DefaultStyle<NText>(DefaultStyleKeyProperty);
    }

    public NTypographyType Type
    {
        get => (NTypographyType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public static readonly DependencyProperty TypeProperty =
        ElementBase.Property<NText, NTypographyType>(nameof(TypeProperty), NTypographyType.Default);

    public bool Strong
    {
        get => (bool)GetValue(StrongProperty);
        set => SetValue(StrongProperty, value);
    }

    public static readonly DependencyProperty StrongProperty =
        ElementBase.Property<NText, bool>(nameof(StrongProperty), false);

    public bool Italic
    {
        get => (bool)GetValue(ItalicProperty);
        set => SetValue(ItalicProperty, value);
    }

    public static readonly DependencyProperty ItalicProperty =
        ElementBase.Property<NText, bool>(nameof(ItalicProperty), false);

    public bool Underline
    {
        get => (bool)GetValue(UnderlineProperty);
        set => SetValue(UnderlineProperty, value);
    }

    public static readonly DependencyProperty UnderlineProperty =
        ElementBase.Property<NText, bool>(nameof(UnderlineProperty), false);

    public bool Delete
    {
        get => (bool)GetValue(DeleteProperty);
        set => SetValue(DeleteProperty, value);
    }

    public static readonly DependencyProperty DeleteProperty =
        ElementBase.Property<NText, bool>(nameof(DeleteProperty), false);

    public bool Code
    {
        get => (bool)GetValue(CodeProperty);
        set => SetValue(CodeProperty, value);
    }

    public static readonly DependencyProperty CodeProperty =
        ElementBase.Property<NText, bool>(nameof(CodeProperty), false);

    public int Depth
    {
        get => (int)GetValue(DepthProperty);
        set => SetValue(DepthProperty, value);
    }

    public static readonly DependencyProperty DepthProperty =
        ElementBase.Property<NText, int>(nameof(DepthProperty), 0);
}

public class NP : ContentControl
{
    static NP()
    {
        ElementBase.DefaultStyle<NP>(DefaultStyleKeyProperty);
    }

    public int Depth
    {
        get => (int)GetValue(DepthProperty);
        set => SetValue(DepthProperty, value);
    }

    public static readonly DependencyProperty DepthProperty =
        ElementBase.Property<NP, int>(nameof(DepthProperty), 0);
}

public abstract class NTypographyHeaderBase : ContentControl
{
    protected NTypographyHeaderBase()
    {
        UpdateResolvedPrefixBarHeight();
    }

    public bool AlignText
    {
        get => (bool)GetValue(AlignTextProperty);
        set => SetValue(AlignTextProperty, value);
    }

    public static readonly DependencyProperty AlignTextProperty =
        ElementBase.Property<NTypographyHeaderBase, bool>(nameof(AlignTextProperty), false, OnHeaderPropertyChanged);

    public NTypographyType Type
    {
        get => (NTypographyType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public static readonly DependencyProperty TypeProperty =
        ElementBase.Property<NTypographyHeaderBase, NTypographyType>(nameof(TypeProperty), NTypographyType.Default);

    public NTypographyHeaderPrefix Prefix
    {
        get => (NTypographyHeaderPrefix)GetValue(PrefixProperty);
        set => SetValue(PrefixProperty, value);
    }

    public static readonly DependencyProperty PrefixProperty =
        ElementBase.Property<NTypographyHeaderBase, NTypographyHeaderPrefix>(nameof(PrefixProperty), NTypographyHeaderPrefix.None);

    public double ResolvedPrefixBarHeight
    {
        get => (double)GetValue(ResolvedPrefixBarHeightProperty);
        private set => SetValue(ResolvedPrefixBarHeightProperty, value);
    }

    public static readonly DependencyProperty ResolvedPrefixBarHeightProperty =
        ElementBase.Property<NTypographyHeaderBase, double>(nameof(ResolvedPrefixBarHeightProperty), double.NaN);

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == FontSizeProperty)
        {
            UpdateResolvedPrefixBarHeight();
        }
    }

    private static void OnHeaderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NTypographyHeaderBase header && e.Property == AlignTextProperty)
        {
            header.UpdateResolvedPrefixBarHeight();
        }
    }

    private void UpdateResolvedPrefixBarHeight()
    {
        ResolvedPrefixBarHeight = AlignText
            ? Math.Max(14d, FontSize)
            : double.NaN;
    }
}

public class NH1 : NTypographyHeaderBase
{
    static NH1()
    {
        ElementBase.DefaultStyle<NH1>(DefaultStyleKeyProperty);
    }
}

public class NH2 : NTypographyHeaderBase
{
    static NH2()
    {
        ElementBase.DefaultStyle<NH2>(DefaultStyleKeyProperty);
    }
}

public class NH3 : NTypographyHeaderBase
{
    static NH3()
    {
        ElementBase.DefaultStyle<NH3>(DefaultStyleKeyProperty);
    }
}

public class NH4 : NTypographyHeaderBase
{
    static NH4()
    {
        ElementBase.DefaultStyle<NH4>(DefaultStyleKeyProperty);
    }
}

public class NH5 : NTypographyHeaderBase
{
    static NH5()
    {
        ElementBase.DefaultStyle<NH5>(DefaultStyleKeyProperty);
    }
}

public class NH6 : NTypographyHeaderBase
{
    static NH6()
    {
        ElementBase.DefaultStyle<NH6>(DefaultStyleKeyProperty);
    }
}

public class NBlockquote : ContentControl
{
    static NBlockquote()
    {
        ElementBase.DefaultStyle<NBlockquote>(DefaultStyleKeyProperty);
    }

    public bool AlignText
    {
        get => (bool)GetValue(AlignTextProperty);
        set => SetValue(AlignTextProperty, value);
    }

    public static readonly DependencyProperty AlignTextProperty =
        ElementBase.Property<NBlockquote, bool>(nameof(AlignTextProperty), false);
}

public abstract class NTypographyListBase : ItemsControl
{
    protected NTypographyListBase()
    {
        Loaded += HandleLoaded;
    }

    public bool AlignText
    {
        get => (bool)GetValue(AlignTextProperty);
        set => SetValue(AlignTextProperty, value);
    }

    public static readonly DependencyProperty AlignTextProperty =
        ElementBase.Property<NTypographyListBase, bool>(nameof(AlignTextProperty), false, OnListPropertyChanged);

    protected abstract bool IsOrdered { get; }

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        RefreshItemsState();
    }

    private static void OnListPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NTypographyListBase list)
        {
            list.RefreshItemsState();
        }
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        RefreshItemsState();
    }

    private void RefreshItemsState()
    {
        for (var i = 0; i < Items.Count; i++)
        {
            if (Items[i] is NLi item)
            {
                item.UpdateListState(i + 1, IsOrdered, AlignText);
            }
        }
    }
}

public class NUl : NTypographyListBase
{
    static NUl()
    {
        ElementBase.DefaultStyle<NUl>(DefaultStyleKeyProperty);
    }

    protected override bool IsOrdered => false;
}

public class NOl : NTypographyListBase
{
    static NOl()
    {
        ElementBase.DefaultStyle<NOl>(DefaultStyleKeyProperty);
    }

    protected override bool IsOrdered => true;
}

public class NLi : ContentControl
{
    static NLi()
    {
        ElementBase.DefaultStyle<NLi>(DefaultStyleKeyProperty);
    }

    public string MarkerText
    {
        get => (string)GetValue(MarkerTextProperty);
        private set => SetValue(MarkerTextProperty, value);
    }

    public static readonly DependencyProperty MarkerTextProperty =
        ElementBase.Property<NLi, string>(nameof(MarkerTextProperty), "\u2022");

    public bool AlignText
    {
        get => (bool)GetValue(AlignTextProperty);
        private set => SetValue(AlignTextProperty, value);
    }

    public static readonly DependencyProperty AlignTextProperty =
        ElementBase.Property<NLi, bool>(nameof(AlignTextProperty), false);

    internal void UpdateListState(int index, bool isOrdered, bool alignText)
    {
        MarkerText = isOrdered ? $"{index}." : "\u2022";
        AlignText = alignText;
    }
}

public class NHr : NDivider
{
    static NHr()
    {
        ElementBase.DefaultStyle<NHr>(DefaultStyleKeyProperty);
    }
}

public class NA : ContentControl
{
    public static readonly RoutedEvent ClickEvent =
        ElementBase.RoutedEvent<NA, RoutedEventHandler>(nameof(ClickEvent));

    private int clickHandlerCount;

    static NA()
    {
        ElementBase.DefaultStyle<NA>(DefaultStyleKeyProperty);
    }

    public NA()
    {
        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;
        UpdateInteractionState();
    }

    public string? Href
    {
        get => (string?)GetValue(HrefProperty);
        set => SetValue(HrefProperty, value);
    }

    public static readonly DependencyProperty HrefProperty =
        ElementBase.Property<NA, string?>(nameof(HrefProperty), null, OnActionPropertyChanged);

    public string? Target
    {
        get => (string?)GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    public static readonly DependencyProperty TargetProperty =
        ElementBase.Property<NA, string?>(nameof(TargetProperty), null);

    public bool Clickable
    {
        get => (bool)GetValue(ClickableProperty);
        set => SetValue(ClickableProperty, value);
    }

    public static readonly DependencyProperty ClickableProperty =
        ElementBase.Property<NA, bool>(nameof(ClickableProperty), true, OnActionPropertyChanged);

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly DependencyProperty CommandProperty =
        ElementBase.Property<NA, ICommand?>(nameof(CommandProperty), null, OnCommandChanged);

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public static readonly DependencyProperty CommandParameterProperty =
        ElementBase.Property<NA, object?>(nameof(CommandParameterProperty), null, OnActionPropertyChanged);

    public bool IsPointerPressed
    {
        get => (bool)GetValue(IsPointerPressedProperty);
        private set => SetValue(IsPointerPressedProperty, value);
    }

    public static readonly DependencyProperty IsPointerPressedProperty =
        ElementBase.Property<NA, bool>(nameof(IsPointerPressedProperty), false);

    public bool CanInvoke
    {
        get => (bool)GetValue(CanInvokeProperty);
        private set => SetValue(CanInvokeProperty, value);
    }

    public static readonly DependencyProperty CanInvokeProperty =
        ElementBase.Property<NA, bool>(nameof(CanInvokeProperty), true);

    public event RoutedEventHandler Click
    {
        add
        {
            AddHandler(ClickEvent, value);
            clickHandlerCount++;
            UpdateInteractionState();
        }
        remove
        {
            RemoveHandler(ClickEvent, value);
            clickHandlerCount = Math.Max(0, clickHandlerCount - 1);
            UpdateInteractionState();
        }
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);

        if (CanInvoke && !NTypographyInteractionHelper.IsInteractiveChildSource(e.OriginalSource as DependencyObject))
        {
            IsPointerPressed = true;
        }
    }

    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonUp(e);
        IsPointerPressed = false;

        if (!CanInvoke || NTypographyInteractionHelper.IsInteractiveChildSource(e.OriginalSource as DependencyObject))
        {
            return;
        }

        RaiseEvent(new RoutedEventArgs(ClickEvent, this));
        ExecuteCommand();

        if (!string.IsNullOrWhiteSpace(Href))
        {
            NTypographyInteractionHelper.TryOpenHref(Href);
        }
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        IsPointerPressed = false;
    }

    private static void OnActionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NA anchor)
        {
            anchor.UpdateInteractionState();
        }
    }

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NA anchor)
        {
            anchor.HandleCommandChanged(e.OldValue as ICommand, e.NewValue as ICommand);
        }
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        HookCommand(Command);
        UpdateInteractionState();
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        UnhookCommand(Command);
    }

    private void HandleCommandChanged(ICommand? oldCommand, ICommand? newCommand)
    {
        if (ReferenceEquals(oldCommand, newCommand))
        {
            UpdateInteractionState();
            return;
        }

        if (IsLoaded)
        {
            UnhookCommand(oldCommand);
            HookCommand(newCommand);
        }

        UpdateInteractionState();
    }

    private void HookCommand(ICommand? command)
    {
        if (command is null)
        {
            return;
        }

        command.CanExecuteChanged += HandleCommandCanExecuteChanged;
    }

    private void UnhookCommand(ICommand? command)
    {
        if (command is null)
        {
            return;
        }

        command.CanExecuteChanged -= HandleCommandCanExecuteChanged;
    }

    private void HandleCommandCanExecuteChanged(object? sender, EventArgs e)
    {
        UpdateInteractionState();
    }

    private void UpdateInteractionState()
    {
        CanInvoke = Clickable && (clickHandlerCount > 0 || Command is not null || !string.IsNullOrWhiteSpace(Href));
        if (Command is not null)
        {
            CanInvoke = CanInvoke && CanExecuteCommand();
        }
    }

    private bool CanExecuteCommand()
    {
        if (Command is null)
        {
            return true;
        }

        return Command is RoutedCommand routedCommand
            ? routedCommand.CanExecute(CommandParameter, this)
            : Command.CanExecute(CommandParameter);
    }

    private void ExecuteCommand()
    {
        if (Command is null)
        {
            return;
        }

        if (Command is RoutedCommand routedCommand)
        {
            if (!routedCommand.CanExecute(CommandParameter, this))
            {
                return;
            }

            routedCommand.Execute(CommandParameter, this);
            return;
        }

        if (!Command.CanExecute(CommandParameter))
        {
            return;
        }

        Command.Execute(CommandParameter);
    }
}

internal static class NTypographyInteractionHelper
{
    internal static bool IsInteractiveChildSource(DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is ButtonBase
                || source is TextBoxBase
                || source is ListBoxItem
                || source is ComboBoxItem
                || source is MenuItem)
            {
                return true;
            }

            source = source switch
            {
                Visual visual => VisualTreeHelper.GetParent(visual),
                System.Windows.Media.Media3D.Visual3D visual3D => VisualTreeHelper.GetParent(visual3D),
                FrameworkContentElement contentElement => contentElement.Parent,
                _ => null
            };
        }

        return false;
    }

    internal static void TryOpenHref(string href)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = href,
                UseShellExecute = true
            });
        }
        catch
        {
        }
    }
}
