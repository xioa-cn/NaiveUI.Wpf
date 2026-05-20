using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NaiveUI.Demo.Models;
using NaiveUI.Demo.Services;

namespace NaiveUI.Demo.ViewModels;

public sealed class CardDocsPageViewModel: ViewModelBase
{
    public CardDocsPageViewModel(string selectedKey = "card")
    {
        SidebarCategories = ComponentSidebarViewModelFactory.Create(selectedKey);
        OutlineItems = DocOutlineItem.Create(
            ("基础", "SectionBasic"),
            ("标题与额外操作", "SectionHeaderExtra"),
            ("封面", "SectionCover"),
            ("分段底部", "SectionSegmented"),
            ("悬浮效果", "SectionHoverable"),
            ("内嵌样式", "SectionEmbedded"),
            ("尺寸", "SectionSizes"),
            ("加载中", "SectionLoading"),
            ("API", "SectionApi"),
            ("Card Props", "SectionCardProps"),
            ("Card Slots", "SectionCardSlots"));

        CardPropsRows =
        [
            new ApiDocRow { Name = "Title", Type = "string", DefaultValue = "\"\"", Description = "设置卡片标题文本。" },
            new ApiDocRow { Name = "Header", Type = "object", DefaultValue = "null", Description = "自定义卡片头部内容。设置后优先于 Title 展示。" },
            new ApiDocRow { Name = "HeaderExtra", Type = "object", DefaultValue = "null", Description = "设置卡片头部右侧额外内容。" },
            new ApiDocRow { Name = "Cover", Type = "object", DefaultValue = "null", Description = "设置卡片顶部封面区域。" },
            new ApiDocRow { Name = "Content", Type = "object", DefaultValue = "null", Description = "设置卡片主体内容。" },
            new ApiDocRow { Name = "Action", Type = "object", DefaultValue = "null", Description = "设置卡片底部操作区内容。" },
            new ApiDocRow { Name = "Footer", Type = "object", DefaultValue = "null", Description = "设置卡片底部说明区内容。" },
            new ApiDocRow { Name = "Size", Type = "NCardSize", DefaultValue = "Medium", Description = "设置卡片尺寸。支持 Small、Medium、Large。" },
            new ApiDocRow { Name = "Bordered", Type = "bool", DefaultValue = "true", Description = "是否显示边框。" },
            new ApiDocRow { Name = "Hoverable", Type = "bool", DefaultValue = "false", Description = "是否启用悬浮抬升与阴影效果。" },
            new ApiDocRow { Name = "Embedded", Type = "bool", DefaultValue = "false", Description = "是否使用嵌入式浅色背景。" },
            new ApiDocRow { Name = "Segmented", Type = "bool", DefaultValue = "false", Description = "是否让底部 action 与 footer 区域采用更明显的分段样式。" },
            new ApiDocRow { Name = "IsLoading", Type = "bool", DefaultValue = "false", Description = "是否显示骨架屏加载状态。" },
            new ApiDocRow { Name = "Background", Type = "Brush", DefaultValue = "Theme.Surface.0.Brush", Description = "设置卡片背景色。" },
            new ApiDocRow { Name = "BorderBrush", Type = "Brush", DefaultValue = "Theme.Border.Brush", Description = "设置卡片边框颜色。" },
            new ApiDocRow { Name = "CornerRadius", Type = "CornerRadius", DefaultValue = "3", Description = "设置卡片圆角。" }
        ];

        CardSlotsRows =
        [
            new ApiDocRow { Name = "Content", Type = "object", DefaultValue = "null", Description = "卡片主体插槽。" },
            new ApiDocRow { Name = "Header", Type = "object", DefaultValue = "null", Description = "卡片头部插槽。" },
            new ApiDocRow { Name = "HeaderExtra", Type = "object", DefaultValue = "null", Description = "卡片头部右侧插槽。" },
            new ApiDocRow { Name = "Cover", Type = "object", DefaultValue = "null", Description = "卡片封面插槽。" },
            new ApiDocRow { Name = "Action", Type = "object", DefaultValue = "null", Description = "卡片操作区插槽。" },
            new ApiDocRow { Name = "Footer", Type = "object", DefaultValue = "null", Description = "卡片底部说明区插槽。" }
        ];
    }

    public ObservableCollection<ComponentSidebarCategoryViewModel> SidebarCategories { get; }

    public IReadOnlyList<DocOutlineItem> OutlineItems { get; }

    public IReadOnlyList<ApiDocRow> CardPropsRows { get; }

    public IReadOnlyList<ApiDocRow> CardSlotsRows { get; }

    public void SelectSidebarItem(ComponentSidebarItemViewModel targetItem)
    {
        foreach (var item in SidebarCategories.SelectMany(category => category.Items))
        {
            item.IsSelected = item == targetItem;
        }
    }
}
