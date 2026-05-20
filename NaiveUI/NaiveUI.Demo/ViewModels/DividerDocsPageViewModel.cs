using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NaiveUI.Demo.Models;
using NaiveUI.Demo.Services;

namespace NaiveUI.Demo.ViewModels;

public sealed class DividerDocsPageViewModel : ViewModelBase
{
    public DividerDocsPageViewModel(string selectedKey = "divider")
    {
        SidebarCategories = ComponentSidebarViewModelFactory.Create(selectedKey);
        OutlineItems = DocOutlineItem.Create(
            ("基础", "SectionBasic"),
            ("标题位置", "SectionTitlePlacement"),
            ("虚线", "SectionDashed"),
            ("垂直分割", "SectionVertical"),
            ("线条粗细", "SectionThickness"),
            ("自定义标题内容", "SectionCustomTitle"),
            ("接口说明", "SectionApi"),
            ("分割线属性", "SectionDividerProps"),
            ("分割线插槽", "SectionDividerSlots"));

        DividerPropsRows =
        [
            new ApiDocRow { Name = "Dashed", Type = "bool", DefaultValue = "false", Description = "是否将横向分割线显示为虚线。" },
            new ApiDocRow { Name = "Vertical", Type = "bool", DefaultValue = "false", Description = "是否切换为垂直分割线。启用后会按 Naive UI 的方式忽略标题内容。" },
            new ApiDocRow { Name = "Height", Type = "double", DefaultValue = "NaN", Description = "在 Vertical 为 true 时设置垂直分割线高度；未设置时回退到 FontSize。" },
            new ApiDocRow { Name = "LineThickness", Type = "double", DefaultValue = "1", Description = "设置分割线粗细，横向与纵向模式都会生效。" },
            new ApiDocRow { Name = "TitlePlacement", Type = "NDividerTitlePlacement", DefaultValue = "Center", Description = "设置标题位置，可选 Left、Center、Right。" },
            new ApiDocRow { Name = "Content", Type = "object", DefaultValue = "null", Description = "默认标题内容，对应 Naive UI 的 default slot。" },
            new ApiDocRow { Name = "BorderBrush", Type = "Brush", DefaultValue = "Theme.Border.Brush", Description = "设置分割线颜色。" },
            new ApiDocRow { Name = "Foreground", Type = "Brush", DefaultValue = "Theme.Text.Primary.Brush", Description = "设置标题文字颜色。" },
            new ApiDocRow { Name = "FontSize", Type = "double", DefaultValue = "16", Description = "设置标题字号，同时影响垂直分割线的默认高度。" }
        ];

        DividerSlotsRows =
        [
            new ApiDocRow { Name = "默认插槽", Type = "object", DefaultValue = "null", Description = "分割线标题内容。可直接放文本，也可放自定义 UI 元素。" }
        ];
    }

    public ObservableCollection<ComponentSidebarCategoryViewModel> SidebarCategories { get; }

    public IReadOnlyList<DocOutlineItem> OutlineItems { get; }

    public IReadOnlyList<ApiDocRow> DividerPropsRows { get; }

    public IReadOnlyList<ApiDocRow> DividerSlotsRows { get; }

    public void SelectSidebarItem(ComponentSidebarItemViewModel targetItem)
    {
        foreach (var item in SidebarCategories.SelectMany(category => category.Items))
        {
            item.IsSelected = item == targetItem;
        }
    }
}
