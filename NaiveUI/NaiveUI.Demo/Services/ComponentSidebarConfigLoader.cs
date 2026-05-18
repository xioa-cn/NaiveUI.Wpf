using System.IO;
using System.Text.Json;
using NaiveUI.Demo.Models;

namespace NaiveUI.Demo.Services;

internal static class ComponentSidebarConfigLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static ComponentSidebarConfig Load(string relativePath = "Data\\ComponentSidebar.json")
    {
        try
        {
            var fullPath = Path.Combine(AppContext.BaseDirectory, relativePath);
            if (!File.Exists(fullPath))
            {
                return new ComponentSidebarConfig();
            }

            var json = File.ReadAllText(fullPath);
            return JsonSerializer.Deserialize<ComponentSidebarConfig>(json, JsonOptions) ?? new ComponentSidebarConfig();
        }
        catch (IOException)
        {
            return new ComponentSidebarConfig();
        }
        catch (UnauthorizedAccessException)
        {
            return new ComponentSidebarConfig();
        }
        catch (JsonException)
        {
            return new ComponentSidebarConfig();
        }
        catch (NotSupportedException)
        {
            return new ComponentSidebarConfig();
        }
    }
}
