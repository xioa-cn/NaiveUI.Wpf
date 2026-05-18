using System.Collections.ObjectModel;

namespace NaiveUI.Demo.ViewModels;

public sealed class ComponentSidebarCategoryViewModel
{
    public string Title { get; init; } = string.Empty;

    public int Count { get; init; }

    public ObservableCollection<ComponentSidebarItemViewModel> Items { get; init; } = [];

    public string HeaderText => $"{Title} ({Count})";
}
