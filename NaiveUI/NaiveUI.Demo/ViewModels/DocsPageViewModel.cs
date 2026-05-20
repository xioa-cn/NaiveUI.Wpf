using System.Collections.ObjectModel;
using System.Linq;
using NaiveUI.Demo.Services;
using NaiveUI.Demo.Views.Pages;

namespace NaiveUI.Demo.ViewModels;

public sealed class DocsPageViewModel : ViewModelBase
{
    private readonly Dictionary<string, Func<object>> componentPageFactories;
    private readonly Dictionary<string, object> componentPageCache = new(StringComparer.OrdinalIgnoreCase);
    private object currentContentView;

    public DocsPageViewModel(string initialComponentKey = "button")
    {
        componentPageFactories = new Dictionary<string, Func<object>>(StringComparer.OrdinalIgnoreCase)
        {
            ["button"] = static () => new ButtonDocsPage(),
            ["avatar"] = static () => new AvatarDocsPage(),
            ["card"] = static () => new CardDocsPage(),
            ["divider"] = static () => new DividerDocsPage(),
            ["carousel"] = static () => new CarouselDocsPage(),
            ["collapse"] = static () => new CollapseDocsPage(),
            ["dropdown"] = static () => new DropdownDocsPage(),
            ["ellipsis"] = static () => new EllipsisDocsPage(),
            ["gradient-text"] = static () => new GradientTextDocsPage()
        };

        SidebarCategories = ComponentSidebarViewModelFactory.Create(initialComponentKey);
        currentContentView = ResolveComponentPage(initialComponentKey);
    }

    public ObservableCollection<ComponentSidebarCategoryViewModel> SidebarCategories { get; }

    public object CurrentContentView
    {
        get => currentContentView;
        private set => SetProperty(ref currentContentView, value);
    }

    public void ShowComponent(string componentKey)
    {
        if (!componentPageFactories.ContainsKey(componentKey))
        {
            return;
        }

        foreach (var item in SidebarCategories.SelectMany(category => category.Items))
        {
            item.IsSelected = string.Equals(item.Key, componentKey, StringComparison.OrdinalIgnoreCase);
        }

        CurrentContentView = ResolveComponentPage(componentKey);
    }

    private object ResolveComponentPage(string componentKey)
    {
        if (componentPageCache.TryGetValue(componentKey, out var existingPage))
        {
            return existingPage;
        }

        if (!componentPageFactories.TryGetValue(componentKey, out var factory))
        {
            factory = componentPageFactories["button"];
            componentKey = "button";
        }

        var page = factory();
        componentPageCache[componentKey] = page;
        return page;
    }
}
