using System.Windows;
using System.Windows.Controls;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

[TemplatePart(Name = MessageHostPartName, Type = typeof(Panel))]
public class NMessageProvider : ContentControl
{
    private const string MessageHostPartName = "PART_MessageHost";
    private Panel? messageHostPart;

    static NMessageProvider()
    {
        ElementBase.DefaultStyle<NMessageProvider>(DefaultStyleKeyProperty);
    }

    public NMessageProvider()
    {
        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        messageHostPart = GetTemplateChild(MessageHostPartName) as Panel;
    }

    internal bool TryGetMessageHostPanel(out Panel panel)
    {
        ApplyTemplate();
        panel = messageHostPart!;
        return panel is not null;
    }

    public INMessageApi UseMessage()
    {
        return NMessage.UseMessage(this);
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        NMessageManager.RegisterProvider(this);
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        NMessageManager.UnregisterProvider(this);
    }
}
