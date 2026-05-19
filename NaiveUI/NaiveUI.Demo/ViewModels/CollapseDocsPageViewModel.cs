using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NaiveUI.Demo.Models;
using NaiveUI.Demo.Services;

namespace NaiveUI.Demo.ViewModels;

public sealed class CollapseDocsPageViewModel : ViewModelBase
{
    public CollapseDocsPageViewModel(string selectedKey = "collapse")
    {
        SidebarCategories = ComponentSidebarViewModelFactory.Create(selectedKey);

        CollapsePropsRows =
        [
            new ApiDocRow { Name = "Accordion", Type = "bool", DefaultValue = "false", Description = "是否启用手风琴模式。启用后同一时间只允许展开一个面板。" },
            new ApiDocRow { Name = "ArrowPlacement", Type = "NCollapseArrowPlacement", DefaultValue = "Left", Description = "设置箭头位置。Left 表示箭头在左侧，Right 表示箭头在右侧。" },
            new ApiDocRow { Name = "DisplayDirective", Type = "NCollapseDisplayDirective", DefaultValue = "If", Description = "设置内容区域的显示策略。If 表示折叠时移出视觉树；Show 表示折叠时保留内容，仅隐藏高度。" },
            new ApiDocRow { Name = "TriggerAreas", Type = "NCollapseTriggerAreas", DefaultValue = "Main, Arrow", Description = "设置哪些区域可以触发展开收起。可选 Main、Arrow、Extra，支持组合。" },
            new ApiDocRow { Name = "ExpandedNames", Type = "IList", DefaultValue = "null", Description = "设置当前展开项名称集合。可用于受控展开状态。" },
            new ApiDocRow { Name = "DefaultExpandedNames", Type = "IList", DefaultValue = "null", Description = "设置默认展开项名称集合。适合非受控的初始状态。" },
            new ApiDocRow { Name = "Background", Type = "Brush", DefaultValue = "Transparent", Description = "设置折叠面板容器背景。" },
            new ApiDocRow { Name = "BorderBrush", Type = "Brush", DefaultValue = "Theme.Border.Brush", Description = "设置折叠面板容器边框颜色。" },
            new ApiDocRow { Name = "BorderThickness", Type = "Thickness", DefaultValue = "0", Description = "设置折叠面板容器边框厚度。" }
        ];

        CollapseItemPropsRows =
        [
            new ApiDocRow { Name = "Title", Type = "string", DefaultValue = "\"\"", Description = "设置折叠项标题文本。" },
            new ApiDocRow { Name = "Header", Type = "object", DefaultValue = "null", Description = "自定义折叠项头部内容。设置后优先于 Title 展示。" },
            new ApiDocRow { Name = "HeaderExtra", Type = "object", DefaultValue = "null", Description = "设置头部右侧额外内容。" },
            new ApiDocRow { Name = "CollapseName", Type = "string", DefaultValue = "\"\"", Description = "设置折叠项标识，对应 Naive UI 中的 name。由于 WPF 已存在 Name 属性，这里使用 CollapseName 参与 ExpandedNames / DefaultExpandedNames 匹配。" },
            new ApiDocRow { Name = "Disabled", Type = "bool", DefaultValue = "false", Description = "是否禁用当前折叠项。" },
            new ApiDocRow { Name = "DisplayDirective", Type = "NCollapseItemDisplayDirective", DefaultValue = "Inherit", Description = "设置当前折叠项的显示策略。Inherit 表示继承父级；If 与 Show 会覆盖父级策略。" },
            new ApiDocRow { Name = "Content", Type = "object", DefaultValue = "null", Description = "设置折叠项主体内容。" }
        ];

        CollapseSlotsRows =
        [
            new ApiDocRow { Name = "默认插槽", Type = "object", DefaultValue = "null", Description = "折叠面板默认插槽，用于放置多个 NCollapseItem。" }
        ];

        CollapseItemSlotsRows =
        [
            new ApiDocRow { Name = "Content", Type = "object", DefaultValue = "null", Description = "折叠项主体内容插槽。" },
            new ApiDocRow { Name = "Header", Type = "object", DefaultValue = "null", Description = "折叠项头部插槽。" },
            new ApiDocRow { Name = "HeaderExtra", Type = "object", DefaultValue = "null", Description = "折叠项头部右侧插槽。" }
        ];
    }

    public ObservableCollection<ComponentSidebarCategoryViewModel> SidebarCategories { get; }

    public IReadOnlyList<ApiDocRow> CollapsePropsRows { get; }

    public IReadOnlyList<ApiDocRow> CollapseItemPropsRows { get; }

    public IReadOnlyList<ApiDocRow> CollapseSlotsRows { get; }

    public IReadOnlyList<ApiDocRow> CollapseItemSlotsRows { get; }

    public void SelectSidebarItem(ComponentSidebarItemViewModel targetItem)
    {
        foreach (var item in SidebarCategories.SelectMany(category => category.Items))
        {
            item.IsSelected = item == targetItem;
        }
    }
}
