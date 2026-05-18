namespace NaiveUI.Demo.Models;

public sealed class ComponentSidebarConfig
{
    public List<ComponentSidebarCategoryConfig> Categories { get; set; } = [];
}

public sealed class ComponentSidebarCategoryConfig
{
    public string Title { get; set; } = string.Empty;

    public int Count { get; set; }

    public List<ComponentSidebarItemConfig> Items { get; set; } = [];
}

public sealed class ComponentSidebarItemConfig
{
    public string Key { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Subtitle { get; set; } = string.Empty;

    public string? BadgeText { get; set; }

    public bool IsSelected { get; set; }
}
