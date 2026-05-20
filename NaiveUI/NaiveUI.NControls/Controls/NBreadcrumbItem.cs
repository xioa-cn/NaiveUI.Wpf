using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public class NBreadcrumbItem : ContentControl
{
    internal NBreadcrumb? ParentBreadcrumb { get; set; }

    static NBreadcrumbItem()
    {
        ElementBase.DefaultStyle<NBreadcrumbItem>(DefaultStyleKeyProperty);
    }

    public NBreadcrumbItem()
    {
        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;
        UpdateInteractionState();
    }

    public string? Separator
    {
        get => (string?)GetValue(SeparatorProperty);
        set => SetValue(SeparatorProperty, value);
    }

    public static readonly DependencyProperty SeparatorProperty =
        ElementBase.Property<NBreadcrumbItem, string?>(nameof(SeparatorProperty), null, OnItemPropertyChanged);

    public bool ShowSeparator
    {
        get => (bool)GetValue(ShowSeparatorProperty);
        set => SetValue(ShowSeparatorProperty, value);
    }

    public static readonly DependencyProperty ShowSeparatorProperty =
        ElementBase.Property<NBreadcrumbItem, bool>(nameof(ShowSeparatorProperty), true, OnItemPropertyChanged);

    public string? Href
    {
        get => (string?)GetValue(HrefProperty);
        set => SetValue(HrefProperty, value);
    }

    public static readonly DependencyProperty HrefProperty =
        ElementBase.Property<NBreadcrumbItem, string?>(nameof(HrefProperty), null);

    public bool Clickable
    {
        get => (bool)GetValue(ClickableProperty);
        set => SetValue(ClickableProperty, value);
    }

    public static readonly DependencyProperty ClickableProperty =
        ElementBase.Property<NBreadcrumbItem, bool>(nameof(ClickableProperty), true, OnActionPropertyChanged);

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly DependencyProperty CommandProperty =
        ElementBase.Property<NBreadcrumbItem, ICommand?>(nameof(CommandProperty), null, OnCommandChanged);

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public static readonly DependencyProperty CommandParameterProperty =
        ElementBase.Property<NBreadcrumbItem, object?>(nameof(CommandParameterProperty), null, OnActionPropertyChanged);

    public object? SeparatorContent
    {
        get => GetValue(SeparatorContentProperty);
        set => SetValue(SeparatorContentProperty, value);
    }

    public static readonly DependencyProperty SeparatorContentProperty =
        ElementBase.Property<NBreadcrumbItem, object?>(nameof(SeparatorContentProperty), null, OnItemPropertyChanged);

    public bool IsLast
    {
        get => (bool)GetValue(IsLastProperty);
        private set => SetValue(IsLastProperty, value);
    }

    public static readonly DependencyProperty IsLastProperty =
        ElementBase.Property<NBreadcrumbItem, bool>(nameof(IsLastProperty), false);

    public bool ShowResolvedSeparator
    {
        get => (bool)GetValue(ShowResolvedSeparatorProperty);
        private set => SetValue(ShowResolvedSeparatorProperty, value);
    }

    public static readonly DependencyProperty ShowResolvedSeparatorProperty =
        ElementBase.Property<NBreadcrumbItem, bool>(nameof(ShowResolvedSeparatorProperty), true);

    public object? ResolvedSeparatorContent
    {
        get => GetValue(ResolvedSeparatorContentProperty);
        private set => SetValue(ResolvedSeparatorContentProperty, value);
    }

    public static readonly DependencyProperty ResolvedSeparatorContentProperty =
        ElementBase.Property<NBreadcrumbItem, object?>(nameof(ResolvedSeparatorContentProperty), "/");

    public bool IsPointerPressed
    {
        get => (bool)GetValue(IsPointerPressedProperty);
        private set => SetValue(IsPointerPressedProperty, value);
    }

    public static readonly DependencyProperty IsPointerPressedProperty =
        ElementBase.Property<NBreadcrumbItem, bool>(nameof(IsPointerPressedProperty), false);

    public bool CanInvoke
    {
        get => (bool)GetValue(CanInvokeProperty);
        private set => SetValue(CanInvokeProperty, value);
    }

    public static readonly DependencyProperty CanInvokeProperty =
        ElementBase.Property<NBreadcrumbItem, bool>(nameof(CanInvokeProperty), true);

    public static readonly RoutedEvent ClickEvent =
        ElementBase.RoutedEvent<NBreadcrumbItem, RoutedEventHandler>(nameof(ClickEvent));

    public event RoutedEventHandler Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);

        if (CanInvoke && !IsInteractiveChildSource(e.OriginalSource as DependencyObject))
        {
            IsPointerPressed = true;
        }
    }

    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonUp(e);
        IsPointerPressed = false;

        if (!CanInvoke || IsInteractiveChildSource(e.OriginalSource as DependencyObject))
        {
            return;
        }

        RaiseEvent(new RoutedEventArgs(ClickEvent, this));
        ExecuteCommand();

        if (!string.IsNullOrWhiteSpace(Href))
        {
            TryOpenHref(Href);
        }
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        IsPointerPressed = false;
    }

    internal void UpdateResolvedState(bool isLast, string breadcrumbSeparator)
    {
        IsLast = isLast;
        ResolvedSeparatorContent = ResolveSeparatorContent(breadcrumbSeparator);
        ShowResolvedSeparator = !isLast && ShowSeparator;
        UpdateInteractionState();
    }

    private static void OnItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NBreadcrumbItem item)
        {
            item.ParentBreadcrumb?.RefreshItemsState();
        }
    }

    private static void OnActionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NBreadcrumbItem item)
        {
            item.UpdateInteractionState();
        }
    }

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NBreadcrumbItem item)
        {
            item.HandleCommandChanged(e.OldValue as ICommand, e.NewValue as ICommand);
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
        CanInvoke = Clickable && !IsLast && CanExecuteCommand();
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

    private object ResolveSeparatorContent(string breadcrumbSeparator)
    {
        if (SeparatorContent is not null)
        {
            return SeparatorContent;
        }

        return Separator ?? breadcrumbSeparator;
    }

    private static bool IsInteractiveChildSource(DependencyObject? source)
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

    private static void TryOpenHref(string href)
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
