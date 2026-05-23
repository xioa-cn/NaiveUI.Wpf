using System.Windows;

namespace NaiveUI.NControls.Tools;

public static class NElMessage
{
    private const int DefaultDuration = 3000;

    public static void Show(string message, string type = "info", int duration = DefaultDuration, Window? window = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        NMessage
            .UseMessage(window)
            .Create(message, ResolveType(type), duration);
    }

    public static void Success(string message, int duration = DefaultDuration, Window? window = null)
    {
        NMessage.UseMessage(window).Success(message, duration);
    }

    public static void Warning(string message, int duration = DefaultDuration, Window? window = null)
    {
        NMessage.UseMessage(window).Warning(message, duration);
    }

    public static void Error(string message, int duration = DefaultDuration, Window? window = null)
    {
        NMessage.UseMessage(window).Error(message, duration);
    }

    public static void Info(string message, int duration = DefaultDuration, Window? window = null)
    {
        NMessage.UseMessage(window).Info(message, duration);
    }

    public static NMessageReactive Loading(string message, int duration = 0, Window? window = null)
    {
        return NMessage.UseMessage(window).Loading(message, duration);
    }

    private static NMessageType ResolveType(string type)
    {
        return type.ToLowerInvariant() switch
        {
            "success" => NMessageType.Success,
            "warning" => NMessageType.Warning,
            "error" => NMessageType.Error,
            "loading" => NMessageType.Loading,
            _ => NMessageType.Info
        };
    }
}
