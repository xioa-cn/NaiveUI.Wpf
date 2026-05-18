using System.Collections.ObjectModel;
using System.Linq;
using NaiveUI.Demo.ViewModels;

namespace NaiveUI.Demo.Services;

internal static class ComponentSidebarViewModelFactory
{
    public static ObservableCollection<ComponentSidebarCategoryViewModel> Create(string selectedKey)
    {
        var config = ComponentSidebarConfigLoader.Load();
        return new ObservableCollection<ComponentSidebarCategoryViewModel>(
            config.Categories.Select(category => new ComponentSidebarCategoryViewModel
            {
                Title = category.Title,
                Count = category.Count,
                Items = new ObservableCollection<ComponentSidebarItemViewModel>(
                    category.Items.Select(item => new ComponentSidebarItemViewModel
                    {
                        Key = item.Key,
                        Title = item.Title,
                        Subtitle = item.Subtitle,
                        BadgeText = item.BadgeText,
                        IsSelected = item.Key == selectedKey || (string.IsNullOrWhiteSpace(selectedKey) && item.IsSelected)
                    }))
            }));
    }
}
