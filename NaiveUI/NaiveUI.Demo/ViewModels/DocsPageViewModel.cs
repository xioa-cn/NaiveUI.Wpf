using System.Collections.ObjectModel;
using System.Linq;
using NaiveUI.Demo.Services;
using NaiveUI.Demo.Views.Pages;
using NaiveUI.NControls.Tools;

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
            ["breadcrumb"] = static () => new BreadcrumbDocsPage(),
            ["card"] = static () => new CardDocsPage(),
            ["divider"] = static () => new DividerDocsPage(),
            ["carousel"] = static () => new CarouselDocsPage(),
            ["collapse"] = static () => new CollapseDocsPage(),
            ["dropdown"] = static () => new DropdownDocsPage(),
            ["ellipsis"] = static () => new EllipsisDocsPage(),
            ["gradient-text"] = static () => new GradientTextDocsPage(),
            ["icon"] = static () => new IconDocsPage(),
            ["float-button"] = static () => new FloatButtonDocsPage(),
            ["badge"] = static () => new BadgeDocsPage(),
            ["tooltip"] = static () => new TooltipDocsPage(),
            ["page-header"] = static () => new PageHeaderDocsPage(),
            ["tag"] = static () => new TagDocsPage(),
            ["typography"] = static () => new TypographyDocsPage(),
            ["watermark"] = static () => new WatermarkDocsPage()
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

    public void ShowComponent(ComponentSidebarItemViewModel component)
    {
        if (!componentPageFactories.ContainsKey(component.Key))
        {
            NElMessage.Error($"页面 {component.Title} 正在开发中！");
            return;
        }

        foreach (var item in SidebarCategories.SelectMany(category => category.Items))
        {
            item.IsSelected = string.Equals(item.Key, component.Key, StringComparison.OrdinalIgnoreCase);
        }

        CurrentContentView = ResolveComponentPage(component.Key);
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
